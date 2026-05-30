using CsvHelper.Configuration.Attributes;

namespace Retimana.Functions.Gtfs.Models.Gtfs;

/// <summary>
/// GTFS calendar.txt の1行。曜日・期間ベースのサービス定義。
/// </summary>
public class GtfsCalendar
{
    [Name("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [Name("monday")]
    public int Monday { get; set; }

    [Name("tuesday")]
    public int Tuesday { get; set; }

    [Name("wednesday")]
    public int Wednesday { get; set; }

    [Name("thursday")]
    public int Thursday { get; set; }

    [Name("friday")]
    public int Friday { get; set; }

    [Name("saturday")]
    public int Saturday { get; set; }

    [Name("sunday")]
    public int Sunday { get; set; }

    [Name("start_date")]
    public string StartDate { get; set; } = string.Empty;

    [Name("end_date")]
    public string EndDate { get; set; } = string.Empty;
}
