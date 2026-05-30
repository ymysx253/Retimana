using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Retimana.Functions.Gtfs.Configuration;
using Retimana.Functions.Gtfs.Models.Cosmos;

namespace Retimana.Functions.Gtfs.Services;

public interface ICosmosBulkImporter
{
    /// <summary>
    /// Cosmos の DB / Container を未作成なら作成する。
    /// </summary>
    Task EnsureContainerExistsAsync(CancellationToken ct = default);

    /// <summary>
    /// ドキュメント群を Cosmos にバルクアップサートする。
    /// </summary>
    Task UpsertBatchAsync<T>(IEnumerable<T> documents, CancellationToken ct = default)
        where T : BusDataDocument;
}

public class CosmosBulkImporter : ICosmosBulkImporter
{
    private readonly CosmosClient _client;
    private readonly CosmosSettings _settings;
    private readonly ILogger<CosmosBulkImporter> _logger;
    private Container? _container;

    public CosmosBulkImporter(CosmosClient client, IOptions<CosmosSettings> settings, ILogger<CosmosBulkImporter> logger)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task EnsureContainerExistsAsync(CancellationToken ct = default)
    {
        var dbResponse = await _client.CreateDatabaseIfNotExistsAsync(_settings.DatabaseName, cancellationToken: ct);
        var db = dbResponse.Database;
        _logger.LogInformation("Cosmos Database ready: {Db}", _settings.DatabaseName);

        var containerProps = new ContainerProperties(_settings.ContainerName, partitionKeyPath: "/cityCode")
        {
            // 地理空間インデックス: location フィールド (GeoJSON Point) で範囲検索を可能にする
            IndexingPolicy = new IndexingPolicy
            {
                IndexingMode = IndexingMode.Consistent,
                Automatic = true,
                IncludedPaths = { new IncludedPath { Path = "/*" } },
                SpatialIndexes = { new SpatialPath
                {
                    Path = "/location/*",
                    SpatialTypes = { SpatialType.Point }
                } }
            }
        };

        var containerResponse = await db.CreateContainerIfNotExistsAsync(containerProps, cancellationToken: ct);
        _container = containerResponse.Container;
        _logger.LogInformation("Cosmos Container ready: {Container} (PartitionKey: /cityCode)", _settings.ContainerName);
    }

    public async Task UpsertBatchAsync<T>(IEnumerable<T> documents, CancellationToken ct = default)
        where T : BusDataDocument
    {
        if (_container is null)
        {
            throw new InvalidOperationException("EnsureContainerExistsAsync を先に呼んでください");
        }

        var tasks = new List<Task>();
        var count = 0;
        var failures = 0;

        foreach (var doc in documents)
        {
            ct.ThrowIfCancellationRequested();
            count++;

            tasks.Add(_container.UpsertItemAsync(doc, new PartitionKey(doc.cityCode), cancellationToken: ct)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Interlocked.Increment(ref failures);
                        _logger.LogWarning(t.Exception, "Cosmos upsert 失敗: id={Id}", doc.id);
                    }
                }, TaskScheduler.Default));

            // バックプレッシャ: 100件ごとに完了待ち
            if (tasks.Count >= 100)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }

        _logger.LogInformation("Cosmos upsert 完了: type={Type}, count={Count}, failures={Failures}",
            typeof(T).Name, count, failures);
    }
}
