using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Retimana.Functions.Gtfs.Configuration;
using Retimana.Functions.Gtfs.Models.Cosmos;
using Retimana.Functions.Gtfs.Models.Gtfs;

namespace Retimana.Functions.Gtfs.Services;

public interface IGtfsIngestionPipeline
{
    /// <summary>
    /// 設定された全データソースについて、ZIP DL → 解凍 → CSV パース → Cosmos 取り込みを実行。
    /// </summary>
    Task RunAsync(CancellationToken ct = default);
}

/// <summary>
/// GTFS 取り込みの全体パイプライン。各データソースを直列に処理。
/// 1事業者分でメモリ・I/Oが大きいので並列実行は避ける。
/// </summary>
public class GtfsIngestionPipeline : IGtfsIngestionPipeline
{
    private readonly IGtfsDownloader _downloader;
    private readonly IGtfsExtractor _extractor;
    private readonly IGtfsCsvParser _parser;
    private readonly ICosmosBulkImporter _cosmos;
    private readonly GtfsSettings _settings;
    private readonly ILogger<GtfsIngestionPipeline> _logger;

    public GtfsIngestionPipeline(
        IGtfsDownloader downloader,
        IGtfsExtractor extractor,
        IGtfsCsvParser parser,
        ICosmosBulkImporter cosmos,
        IOptions<GtfsSettings> settings,
        ILogger<GtfsIngestionPipeline> logger)
    {
        _downloader = downloader;
        _extractor = extractor;
        _parser = parser;
        _cosmos = cosmos;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await _cosmos.EnsureContainerExistsAsync(ct);

        if (_settings.Sources.Count == 0)
        {
            _logger.LogWarning("GTFS Sources が未設定。何もしません。");
            return;
        }

        foreach (var source in _settings.Sources)
        {
            try
            {
                await IngestOneAsync(source, ct);
            }
            catch (Exception ex)
            {
                // 1事業者分が失敗しても他は続行（部分的にでもデータを更新する）
                _logger.LogError(ex, "事業者 {Operator} の取り込みに失敗", source.OperatorId);
            }
        }
    }

    private async Task IngestOneAsync(GtfsDataSource source, CancellationToken ct)
    {
        _logger.LogInformation("=== {Operator} 取り込み開始 ===", source.OperatorId);

        var zipPath = await _downloader.DownloadAsync(source, ct);
        var gtfsDir = _extractor.Extract(zipPath);

        // stops.txt → StopDocument
        var stops = _parser.StreamRows<GtfsStop>(gtfsDir, "stops.txt")
            .Select(s => new StopDocument
            {
                id = $"stop:{source.OperatorId}:{s.StopId}",
                cityCode = source.CityCode,
                operatorId = source.OperatorId,
                dataDate = source.DataDate,
                stopId = s.StopId,
                stopCode = s.StopCode,
                stopName = s.StopName,
                stopDesc = s.StopDesc,
                latitude = s.StopLat,
                longitude = s.StopLon,
                zoneId = s.ZoneId,
                locationType = s.LocationType,
                parentStation = s.ParentStation,
                wheelchairBoarding = s.WheelchairBoarding,
                platformCode = s.PlatformCode,
                location = new GeoJsonPoint
                {
                    coordinates = new[] { s.StopLon, s.StopLat }
                }
            });
        await _cosmos.UpsertBatchAsync(stops, ct);

        // routes.txt → RouteDocument
        var routes = _parser.StreamRows<GtfsRoute>(gtfsDir, "routes.txt")
            .Select(r => new RouteDocument
            {
                id = $"route:{source.OperatorId}:{r.RouteId}",
                cityCode = source.CityCode,
                operatorId = source.OperatorId,
                dataDate = source.DataDate,
                routeId = r.RouteId,
                agencyId = r.AgencyId,
                routeShortName = r.RouteShortName,
                routeLongName = r.RouteLongName,
                routeDesc = r.RouteDesc,
                routeType = r.RouteType,
                routeColor = r.RouteColor,
                routeTextColor = r.RouteTextColor,
                jpParentRouteId = r.JpParentRouteId,
            });
        await _cosmos.UpsertBatchAsync(routes, ct);

        // trips.txt → TripDocument
        var trips = _parser.StreamRows<GtfsTrip>(gtfsDir, "trips.txt")
            .Select(t => new TripDocument
            {
                id = $"trip:{source.OperatorId}:{t.TripId}",
                cityCode = source.CityCode,
                operatorId = source.OperatorId,
                dataDate = source.DataDate,
                tripId = t.TripId,
                routeId = t.RouteId,
                serviceId = t.ServiceId,
                tripHeadsign = t.TripHeadsign,
                tripShortName = t.TripShortName,
                directionId = t.DirectionId,
                blockId = t.BlockId,
                shapeId = t.ShapeId,
                jpPatternId = t.JpPatternId,
            });
        await _cosmos.UpsertBatchAsync(trips, ct);

        // calendar.txt → CalendarDocument
        var calendars = _parser.StreamRows<GtfsCalendar>(gtfsDir, "calendar.txt")
            .Select(c => new CalendarDocument
            {
                id = $"calendar:{source.OperatorId}:{c.ServiceId}",
                cityCode = source.CityCode,
                operatorId = source.OperatorId,
                dataDate = source.DataDate,
                serviceId = c.ServiceId,
                monday = c.Monday == 1,
                tuesday = c.Tuesday == 1,
                wednesday = c.Wednesday == 1,
                thursday = c.Thursday == 1,
                friday = c.Friday == 1,
                saturday = c.Saturday == 1,
                sunday = c.Sunday == 1,
                startDate = c.StartDate,
                endDate = c.EndDate,
            });
        await _cosmos.UpsertBatchAsync(calendars, ct);

        // stop_times.txt → StopTimeDocument (65万行クラス、ストリーミングで処理)
        var stopTimes = _parser.StreamRows<GtfsStopTime>(gtfsDir, "stop_times.txt")
            .Select(st => new StopTimeDocument
            {
                id = $"stopTime:{source.OperatorId}:{st.TripId}:{st.StopSequence}",
                cityCode = source.CityCode,
                operatorId = source.OperatorId,
                dataDate = source.DataDate,
                tripId = st.TripId,
                stopId = st.StopId,
                stopSequence = st.StopSequence,
                arrivalTime = st.ArrivalTime,
                departureTime = st.DepartureTime,
                stopHeadsign = st.StopHeadsign,
                pickupType = st.PickupType,
                dropOffType = st.DropOffType,
            });
        await _cosmos.UpsertBatchAsync(stopTimes, ct);

        _logger.LogInformation("=== {Operator} 取り込み完了 ===", source.OperatorId);
    }
}
