/*
 * ［アプリケーション　＞　要求ファイル作成］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;

using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;
using System.Text;

internal static class RequestFileCreate
{
    internal static string BuildDefaultPath()
    {
        return Path.Combine(
            RepositoryPaths.OutputPath,
            "TournamentUser",
            "Request",
            "manual_input_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
    }

    internal static string ResolveOutputPath(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);
        if (Directory.Exists(fullPath) || LooksLikeDirectoryPath(inputPath))
        {
            return Path.Combine(fullPath, "manual_input_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
        }

        return fullPath;
    }

    static bool LooksLikeDirectoryPath(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            || path.EndsWith(Path.AltDirectorySeparatorChar)
            || string.IsNullOrEmpty(Path.GetExtension(path));
    }
}

