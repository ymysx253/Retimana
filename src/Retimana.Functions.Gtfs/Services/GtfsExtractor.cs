using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace Retimana.Functions.Gtfs.Services;

public interface IGtfsExtractor
{
    /// <summary>
    /// GTFS ZIP を一時ディレクトリに解凍する。
    /// </summary>
    /// <returns>解凍先ディレクトリのフルパス</returns>
    string Extract(string zipPath);
}

public class GtfsExtractor : IGtfsExtractor
{
    private readonly ILogger<GtfsExtractor> _logger;

    public GtfsExtractor(ILogger<GtfsExtractor> logger)
    {
        _logger = logger;
    }

    public string Extract(string zipPath)
    {
        var extractDir = Path.Combine(
            Path.GetDirectoryName(zipPath)!,
            Path.GetFileNameWithoutExtension(zipPath));

        if (Directory.Exists(extractDir))
        {
            Directory.Delete(extractDir, recursive: true);
        }
        Directory.CreateDirectory(extractDir);

        ZipFile.ExtractToDirectory(zipPath, extractDir);

        var files = Directory.GetFiles(extractDir);
        _logger.LogInformation("GTFS ZIP 解凍完了: {Path} ({FileCount} files)", extractDir, files.Length);

        return extractDir;
    }
}
