using System.Text;

internal static partial class Program
{
    static string ResolveOutputCsvPath(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);
        if (Directory.Exists(fullPath)) return Path.Combine(fullPath, $"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        if (LooksLikeDirectoryPath(inputPath)) return Path.Combine(fullPath, $"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        return fullPath;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static bool LooksLikeDirectoryPath(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            || path.EndsWith(Path.AltDirectorySeparatorChar)
            || string.IsNullOrEmpty(Path.GetExtension(path));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseCsvPath"></param>
    /// <param name="fileNamePrefix"></param>
    /// <returns></returns>
    static string BuildSiblingOutputCsvPath(string baseCsvPath, string fileNamePrefix)
    {
        var directoryPath = Path.GetDirectoryName(baseCsvPath) ?? Path.GetFullPath(".");
        return Path.Combine(directoryPath, $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="basePath"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    static string ChangeOutputExtension(string basePath, string extension)
    {
        return Path.ChangeExtension(basePath, extension);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="players"></param>
    /// <param name="matches"></param>
    static void WriteReferenceMatchCsv(string outputCsvPath, IReadOnlyList<Player> players, IReadOnlyList<Match> matches)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var lines = new List<string>
        {
            "first,second"
        };

        lines.AddRange(matches.Select(match => $"{EscapeCsv(players[match.FirstPlayer].Name)},{EscapeCsv(players[match.SecondPlayer].Name)}"));
        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    static string EscapeCsv(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r')) return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

