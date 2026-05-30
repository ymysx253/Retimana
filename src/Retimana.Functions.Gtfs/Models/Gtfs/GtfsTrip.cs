using CsvHelper.Configuration.Attributes;

namespace Retimana.Functions.Gtfs.Models.Gtfs;

/// <summary>
/// GTFS trips.txt の1行。バスの1便（運行）。
/// </summary>
public class GtfsTrip
{
    [Name("route_id")]
    public string RouteId { get; set; } = string.Empty;

    [Name("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [Name("trip_id")]
    public string TripId { get; set; } = string.Empty;

    [Name("trip_headsign")]
    public string? TripHeadsign { get; set; }

    [Name("trip_short_name")]
    public string? TripShortName { get; set; }

    [Name("direction_id")]
    [Optional]
    public int? DirectionId { get; set; }

    [Name("block_id")]
    public string? BlockId { get; set; }

    [Name("shape_id")]
    public string? ShapeId { get; set; }

    [Name("wheelchair_accessible")]
    [Optional]
    public int? WheelchairAccessible { get; set; }

    [Name("bikes_allowed")]
    [Optional]
    public int? BikesAllowed { get; set; }

    [Name("jp_trip_desc")]
    [Optional]
    public string? JpTripDesc { get; set; }

    [Name("jp_office_id")]
    [Optional]
    public string? JpOfficeId { get; set; }

    [Name("jp_pattern_id")]
    [Optional]
    public string? JpPatternId { get; set; }
}
