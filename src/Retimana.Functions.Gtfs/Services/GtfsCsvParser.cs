using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Retimana.Functions.Gtfs.Services;

public interface IGtfsCsvParser
{
    /// <summary>
    /// GTFS の CSV ファイル（拡張子 .txt）をストリーミング読みする。
    /// 65万行クラスの巨大ファイル（stop_times.txt）でもメモリ過剰消費しない。
    /// </summary>
    IEnumerable<T> StreamRows<T>(string gtfsDir, string fileName) where T : class;
}

public class GtfsCsvParser : IGtfsCsvParser
{
    private readonly ILogger<GtfsCsvParser> _logger;

    public GtfsCsvParser(ILogger<GtfsCsvParser> logger)
    {
        _logger = logger;
    }

    public IEnumerable<T> StreamRows<T>(string gtfsDir, string fileName) where T : class
    {
        var path = Path.Combine(gtfsDir, fileName);
        if (!File.Exists(path))
        {
            _logger.LogWarning("GTFS ファイル未存在: {Path}", path);
            yield break;
        }

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
        });

        foreach (var record in csv.GetRecords<T>())
        {
            yield return record;
        }
    }
}
