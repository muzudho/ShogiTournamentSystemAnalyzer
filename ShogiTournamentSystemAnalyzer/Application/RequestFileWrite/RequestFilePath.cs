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
            "ManualInputLog",
            BuildFileName());
    }

    internal static string ResolveOutputPath(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);
        if (Directory.Exists(fullPath) || LooksLikeDirectoryPath(inputPath))
        {
            return Path.Combine(fullPath, BuildFileName());
        }

        return fullPath;
    }

    static string BuildFileName()
    {
        return "manual_input_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".手入力ログ.txt";
    }

    static bool LooksLikeDirectoryPath(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            || path.EndsWith(Path.AltDirectorySeparatorChar)
            || string.IsNullOrEmpty(Path.GetExtension(path));
    }
}
