namespace Retimana.Functions.Gtfs.Models.Cosmos;

/// <summary>
/// バス停（標柱）情報のCosmosドキュメント。
/// 緯度経度から最寄りバス停を検索するため、Cosmos地理空間インデックスを設定する想定。
/// </summary>
public class StopDocument : BusDataDocument
{
    public override string type => "stop";

    public string stopId { get; set; } = string.Empty;
    public string? stopCode { get; set; }
    public string stopName { get; set; } = string.Empty;
    public string? stopDesc { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string? zoneId { get; set; }
    public int? locationType { get; set; }
    public string? parentStation { get; set; }
    public int? wheelchairBoarding { get; set; }
    public string? platformCode { get; set; }

    /// <summary>
    /// CosmosDB の地理空間検索（ST_DISTANCE等）用GeoJSON Point。
    /// </summary>
    public GeoJsonPoint location { get; set; } = new();
}

/// <summary>
/// GeoJSON Point 形式。CosmosDBは内部でこの形式を地理空間インデックスに使う。
/// </summary>
public class GeoJsonPoint
{
    public string type { get; set; } = "Point";

    /// <summary>
    /// [longitude, latitude] の順（GeoJSON仕様）。
    /// </summary>
    public double[] coordinates { get; set; } = new double[2];
}
