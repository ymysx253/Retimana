namespace Retimana.Functions.Gtfs.Models.Cosmos;

/// <summary>
/// サービス（service_id）の運行カレンダー。曜日フラグ + 有効期間。
/// </summary>
public class CalendarDocument : BusDataDocument
{
    public override string type => "calendar";

    public string serviceId { get; set; } = string.Empty;
    public bool monday { get; set; }
    public bool tuesday { get; set; }
    public bool wednesday { get; set; }
    public bool thursday { get; set; }
    public bool friday { get; set; }
    public bool saturday { get; set; }
    public bool sunday { get; set; }

    /// <summary>サービス開始日 YYYYMMDD</summary>
    public string startDate { get; set; } = string.Empty;

    /// <summary>サービス終了日 YYYYMMDD</summary>
    public string endDate { get; set; } = string.Empty;
}
