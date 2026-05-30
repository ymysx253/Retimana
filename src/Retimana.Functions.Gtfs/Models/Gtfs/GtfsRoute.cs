using CsvHelper.Configuration.Attributes;

namespace Retimana.Functions.Gtfs.Models.Gtfs;

/// <summary>
/// GTFS routes.txt の1行。バス路線情報。
/// </summary>
public class GtfsRoute
{
    [Name("route_id")]
    public string RouteId { get; set; } = string.Empty;

    [Name("agency_id")]
    public string? AgencyId { get; set; }

    [Name("route_short_name")]
    public string? RouteShortName { get; set; }

    [Name("route_long_name")]
    public string? RouteLongName { get; set; }

    [Name("route_desc")]
    public string? RouteDesc { get; set; }

    [Name("route_type")]
    public int RouteType { get; set; }

    [Name("route_url")]
    public string? RouteUrl { get; set; }

    [Name("route_color")]
    public string? RouteColor { get; set; }

    [Name("route_text_color")]
    public string? RouteTextColor { get; set; }

    [Name("jp_parent_route_id")]
    [Optional]
    public string? JpParentRouteId { get; set; }
}
