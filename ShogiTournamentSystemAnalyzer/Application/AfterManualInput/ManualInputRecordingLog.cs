/*
 * ［アプリケーション　＞　手動入力後　＞　手動入力録音ログ］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

using ShogiTournamentSystemAnalyzer.Application.Shared;

internal static class ManualInputRecordingLog
{
    internal static string BuildDefaultPath()
    {
        return Path.Combine(
            RepositoryPaths.OutputPath,
            "TournamentUser",
            "RecordingLog",
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
        return "manual_input_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".録音ログ.txt";
    }

    static bool LooksLikeDirectoryPath(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            || path.EndsWith(Path.AltDirectorySeparatorChar)
            || string.IsNullOrEmpty(Path.GetExtension(path));
    }
}
