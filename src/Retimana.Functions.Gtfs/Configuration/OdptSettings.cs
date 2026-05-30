namespace Retimana.Functions.Gtfs.Configuration;

/// <summary>
/// ODPT API 全体の認証情報。"Odpt" セクションにバインド。
/// </summary>
public class OdptSettings
{
    public const string SectionName = "Odpt";

    /// <summary>
    /// ODPT が発行する acl:consumerKey。dotnet user-secrets または環境変数で供給。
    /// </summary>
    public string ConsumerKey { get; set; } = string.Empty;
}
