namespace Retimana.Functions.Gtfs.Models.Cosmos;

/// <summary>
/// CosmosDB "bus-data" コンテナの全ドキュメント共通フィールド。
/// パーティションキーは /cityCode、ドキュメントは type フィールドで判別する。
/// </summary>
public abstract class BusDataDocument
{
    /// <summary>
    /// ドキュメントID。フォーマット: "{type}:{operatorId}:{gtfsId}"
    /// 例: "stop:kyoto-city-bus:003310"
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// パーティションキー。例: "kyoto"
    /// </summary>
    public string cityCode { get; set; } = string.Empty;

    /// <summary>
    /// ドキュメント種別の識別子。"stop" | "route" | "trip" | "stopTime" | "calendar" 等。
    /// </summary>
    public abstract string type { get; }

    /// <summary>
    /// 事業者ID。例: "kyoto-city-bus", "kyoto-bus"
    /// </summary>
    public string operatorId { get; set; } = string.Empty;

    /// <summary>
    /// GTFSデータの公開日（YYYYMMDD）。バージョン管理用。
    /// </summary>
    public string dataDate { get; set; } = string.Empty;

    /// <summary>
    /// 取り込み時刻（UTC ISO8601）。
    /// </summary>
    public DateTime ingestedAt { get; set; } = DateTime.UtcNow;
}
