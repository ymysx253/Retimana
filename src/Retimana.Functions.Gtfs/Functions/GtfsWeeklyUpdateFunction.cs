using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Retimana.Functions.Gtfs.Services;

namespace Retimana.Functions.Gtfs.Functions;

/// <summary>
/// 週次でODPTからGTFS-JPデータを取り込むTimer Function。
/// 既定スケジュール: 毎週日曜 03:00 JST (= 18:00 UTC 土曜)
/// </summary>
public class GtfsWeeklyUpdateFunction
{
    private readonly IGtfsIngestionPipeline _pipeline;
    private readonly ILogger<GtfsWeeklyUpdateFunction> _logger;

    public GtfsWeeklyUpdateFunction(IGtfsIngestionPipeline pipeline, ILogger<GtfsWeeklyUpdateFunction> logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    [Function(nameof(GtfsWeeklyUpdateFunction))]
    public async Task RunAsync(
        [TimerTrigger("%Gtfs:DownloadIntervalCron%", RunOnStartup = false)] TimerInfo timer,
        CancellationToken ct)
    {
        var startedAt = DateTime.UtcNow;
        _logger.LogInformation("GTFS週次取り込み開始: scheduled={Scheduled}, next={Next}",
            timer.ScheduleStatus?.Last, timer.ScheduleStatus?.Next);

        try
        {
            await _pipeline.RunAsync(ct);
            _logger.LogInformation("GTFS週次取り込み正常終了: elapsed={Elapsed}",
                DateTime.UtcNow - startedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GTFS週次取り込み失敗");
            throw; // Functions 側でリトライ/失敗扱いさせる
        }
    }
}
