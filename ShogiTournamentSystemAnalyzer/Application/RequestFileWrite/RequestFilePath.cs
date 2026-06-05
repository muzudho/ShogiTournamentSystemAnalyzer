/*
 * ［アプリケーション　＞　要求ファイル書出　＞　要求ファイルパス］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;

using ShogiTournamentSystemAnalyzer.Application.Shared;

/// <summary>
/// 要求ファイルパス
/// </summary>
internal static class RequestFilePath
{
    internal static string BuildDefaultPath()
    {
        return Path.Combine(
            RepositoryPaths.OutputPath,
            "TournamentUser",
            "Request",
            BuildFileName());
    }

    internal static string ResolveOutputPath(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);
        if (Directory.Exists(fullPath) || LooksLikeDirectoryPath(inputPath))
        {
            return Path.Combine(fullPath, BuildFileName());
        }

        return EnsureRequestFileName(fullPath);
    }

    static string BuildFileName()
    {
        return "request_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".request.txt";
    }

    static string EnsureRequestFileName(string fullPath)
    {
        var directoryName = Path.GetDirectoryName(fullPath) ?? string.Empty;
        var fileName = Path.GetFileName(fullPath);
        if (fileName.EndsWith(".request.txt", StringComparison.OrdinalIgnoreCase)) return fullPath;

        var normalizedFileName = Path.GetExtension(fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFileNameWithoutExtension(fileName) + ".request.txt"
            : fileName + ".request.txt";

        return string.IsNullOrEmpty(directoryName)
            ? normalizedFileName
            : Path.Combine(directoryName, normalizedFileName);
    }

    static bool LooksLikeDirectoryPath(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            || path.EndsWith(Path.AltDirectorySeparatorChar)
            || string.IsNullOrEmpty(Path.GetExtension(path));
    }
}
