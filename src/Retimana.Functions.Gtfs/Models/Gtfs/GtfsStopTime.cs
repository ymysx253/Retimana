using CsvHelper.Configuration.Attributes;

namespace Retimana.Functions.Gtfs.Models.Gtfs;

/// <summary>
/// GTFS stop_times.txt の1行。便の各バス停での発着時刻。
/// 65万行クラスの巨大ファイルなのでストリーミング処理が必須。
/// </summary>
public class GtfsStopTime
{
    [Name("trip_id")]
    public string TripId { get; set; } = string.Empty;

    [Name("arrival_time")]
    public string? ArrivalTime { get; set; }

    [Name("departure_time")]
    public string? DepartureTime { get; set; }

    [Name("stop_id")]
    public string StopId { get; set; } = string.Empty;

    [Name("stop_sequence")]
    public int StopSequence { get; set; }

    [Name("stop_headsign")]
    public string? StopHeadsign { get; set; }

    [Name("pickup_type")]
    [Optional]
    public int? PickupType { get; set; }

    [Name("drop_off_type")]
    [Optional]
    public int? DropOffType { get; set; }

    [Name("shape_dist_traveled")]
    [Optional]
    public double? ShapeDistTraveled { get; set; }

    [Name("timepoint")]
    [Optional]
    public int? Timepoint { get; set; }
}
