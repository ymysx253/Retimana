using CsvHelper.Configuration.Attributes;

namespace Retimana.Functions.Gtfs.Models.Gtfs;

/// <summary>
/// GTFS stops.txt の1行。バス停（標柱）情報。
/// 仕様: https://gtfs.org/schedule/reference/#stopstxt
/// </summary>
public class GtfsStop
{
    [Name("stop_id")]
    public string StopId { get; set; } = string.Empty;

    [Name("stop_code")]
    public string? StopCode { get; set; }

    [Name("stop_name")]
    public string StopName { get; set; } = string.Empty;

    [Name("stop_desc")]
    public string? StopDesc { get; set; }

    [Name("stop_lat")]
    public double StopLat { get; set; }

    [Name("stop_lon")]
    public double StopLon { get; set; }

    [Name("zone_id")]
    public string? ZoneId { get; set; }

    [Name("stop_url")]
    public string? StopUrl { get; set; }

    [Name("location_type")]
    [Optional]
    public int? LocationType { get; set; }

    [Name("parent_station")]
    public string? ParentStation { get; set; }

    [Name("stop_timezone")]
    public string? StopTimezone { get; set; }

    [Name("wheelchair_boarding")]
    [Optional]
    public int? WheelchairBoarding { get; set; }

    [Name("platform_code")]
    public string? PlatformCode { get; set; }
}
