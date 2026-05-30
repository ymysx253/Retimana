namespace Retimana.Functions.Gtfs.Models.Cosmos;

public class RouteDocument : BusDataDocument
{
    public override string type => "route";

    public string routeId { get; set; } = string.Empty;
    public string? agencyId { get; set; }
    public string? routeShortName { get; set; }
    public string? routeLongName { get; set; }
    public string? routeDesc { get; set; }
    public int routeType { get; set; }
    public string? routeColor { get; set; }
    public string? routeTextColor { get; set; }
    public string? jpParentRouteId { get; set; }
}
