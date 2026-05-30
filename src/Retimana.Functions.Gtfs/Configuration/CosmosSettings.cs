namespace Retimana.Functions.Gtfs.Configuration;

/// <summary>
/// CosmosDB 接続情報。"Cosmos" セクションにバインド。
/// 開発: Azure Cosmos DB Emulator のエンドポイント+固定キー。
/// 本番: Azure 上の Cosmos アカウント。
/// </summary>
public class CosmosSettings
{
    public const string SectionName = "Cosmos";

    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "retimana";
    public string ContainerName { get; set; } = "bus-data";
}
