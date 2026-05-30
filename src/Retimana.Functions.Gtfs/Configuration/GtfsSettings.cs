namespace Retimana.Functions.Gtfs.Configuration;

/// <summary>
/// GTFS-JP データソース全体の設定。appsettings/local.settings.json の "Gtfs" セクションにバインドされる。
/// </summary>
public class GtfsSettings
{
    public const string SectionName = "Gtfs";

    /// <summary>
    /// 取り込み対象のデータソース一覧。京都MVPでは京都市営バスと京都バス株式会社の2件。
    /// </summary>
    public List<GtfsDataSource> Sources { get; set; } = new();
}

/// <summary>
/// 1事業者分のGTFS-JPデータソース。
/// </summary>
public class GtfsDataSource
{
    /// <summary>
    /// 事業者の識別子。CosmosDB documentId の prefix に使う。
    /// 例: "kyoto-city-bus", "kyoto-bus"
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// 都市コード。CosmosDB のパーティションキーに使う。例: "kyoto"
    /// </summary>
    public string CityCode { get; set; } = string.Empty;

    /// <summary>
    /// 表示用の事業者名（日本語）。例: "京都市交通局"
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// ODPT File API のZIP URL（{date} と {apiKey} をプレースホルダで含む）。
    /// 例: "https://api.odpt.org/api/v4/files/odpt/KyotoMunicipalTransportation/Kyoto_City_Bus_GTFS.zip?date={date}&acl:consumerKey={apiKey}"
    /// </summary>
    public string ZipUrlTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 現在対象とするGTFSデータ公開日（YYYYMMDD）。手動更新運用が前提。
    /// </summary>
    public string DataDate { get; set; } = string.Empty;
}
