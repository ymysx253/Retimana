using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Retimana.Functions.Gtfs.Configuration;

namespace Retimana.Functions.Gtfs.Services;

public interface IGtfsDownloader
{
    /// <summary>
    /// ODPT File API から GTFS-JP ZIP をダウンロードし、ローカル一時フォルダに保存する。
    /// </summary>
    /// <returns>保存した ZIP ファイルのフルパス</returns>
    Task<string> DownloadAsync(GtfsDataSource source, CancellationToken ct = default);
}

public class GtfsDownloader : IGtfsDownloader
{
    private readonly HttpClient _http;
    private readonly OdptSettings _odpt;
    private readonly ILogger<GtfsDownloader> _logger;

    public GtfsDownloader(HttpClient http, IOptions<OdptSettings> odpt, ILogger<GtfsDownloader> logger)
    {
        _http = http;
        _odpt = odpt.Value;
        _logger = logger;
    }

    public async Task<string> DownloadAsync(GtfsDataSource source, CancellationToken ct = default)
    {
        // acl:consumerKey の ":" を URL エンコードしないため文字列補間で組み立てる
        // (QueryHelpers.AddQueryString は ":" を %3A にエスケープして API が拒否することがある)
        var url = source.ZipUrlTemplate
            .Replace("{date}", source.DataDate)
            .Replace("{apiKey}", _odpt.ConsumerKey);

        _logger.LogInformation("GTFS ZIP DL 開始: operator={Operator}, date={Date}",
            source.OperatorId, source.DataDate);

        var tempDir = Path.Combine(Path.GetTempPath(), "retimana-gtfs", source.OperatorId);
        Directory.CreateDirectory(tempDir);
        var zipPath = Path.Combine(tempDir, $"{source.OperatorId}-{source.DataDate}.zip");

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using (var inStream = await response.Content.ReadAsStreamAsync(ct))
        await using (var outStream = File.Create(zipPath))
        {
            await inStream.CopyToAsync(outStream, ct);
        }

        var sizeMb = new FileInfo(zipPath).Length / 1024.0 / 1024.0;
        _logger.LogInformation("GTFS ZIP DL 完了: {Path} ({SizeMb:F1} MB)", zipPath, sizeMb);

        return zipPath;
    }
}
