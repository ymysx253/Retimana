namespace Retimana.Functions.Gtfs.Models.Cosmos;

public class TripDocument : BusDataDocument
{
    public override string type => "trip";

    public string tripId { get; set; } = string.Empty;
    public string routeId { get; set; } = string.Empty;
    public string serviceId { get; set; } = string.Empty;
    public string? tripHeadsign { get; set; }
    public string? tripShortName { get; set; }
    public int? directionId { get; set; }
    public string? blockId { get; set; }
    public string? shapeId { get; set; }
    public string? jpPatternId { get; set; }
}
