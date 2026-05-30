namespace Retimana.Functions.Gtfs.Models.Cosmos;

/// <summary>
/// 便（trip）の各バス停での発着時刻ドキュメント。
/// 京都全体で65万件以上になるためバルクインサート + Trip単位の集約検索を想定。
/// </summary>
public class StopTimeDocument : BusDataDocument
{
    public override string type => "stopTime";

    public string tripId { get; set; } = string.Empty;
    public string stopId { get; set; } = string.Empty;
    public int stopSequence { get; set; }
    public string? arrivalTime { get; set; }
    public string? departureTime { get; set; }
    public string? stopHeadsign { get; set; }
    public int? pickupType { get; set; }
    public int? dropOffType { get; set; }
}
