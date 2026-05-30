using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Retimana.Functions.Realtime.Functions;

/// <summary>
/// GTFS-RT (Realtime) を30秒ごとに ODPT API からポーリングして
/// CosmosDB のキャッシュコンテナに書き込む Timer Function。
/// 本実装は次回セッションで対応予定。今は骨組みのみ。
/// </summary>
public class GtfsRealtimePollerFunction
{
    private readonly ILogger<GtfsRealtimePollerFunction> _logger;

    public GtfsRealtimePollerFunction(ILogger<GtfsRealtimePollerFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(GtfsRealtimePollerFunction))]
    public Task RunAsync(
        [TimerTrigger("%Realtime:PollingIntervalCron%", RunOnStartup = false)] TimerInfo timer,
        CancellationToken ct)
    {
        _logger.LogInformation("GTFS-RT poller tick: scheduled={Scheduled}", timer.ScheduleStatus?.Last);
        // TODO: 次回セッションで実装
        //   1. ODPT GTFS-RT エンドポイントから protobuf を取得
        //   2. Google.Protobuf + GtfsRealtimeBindings でパース
        //   3. VehiclePosition / TripUpdate を Cosmos の realtime コンテナに upsert (TTL付き)
        //   4. SignalR でブラウザに push（ホストはWeb側）
        return Task.CompletedTask;
    }
}
