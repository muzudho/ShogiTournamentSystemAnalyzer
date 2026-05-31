/*
 * ［アプリケーション　＞　要求ファイル作成］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;

using System.Text;
using ShogiTournamentSystemAnalyzer.Application.Shared;

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

    internal static void Write(string outputPath, IEnumerable<string> lines)
    {
        var directoryPath = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllLines(outputPath, lines, new UTF8Encoding(false));
        Console.WriteLine($"要求ファイルを作成しました: {outputPath}");
    }

    static bool LooksLikeDirectoryPath(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            || path.EndsWith(Path.AltDirectorySeparatorChar)
            || string.IsNullOrEmpty(Path.GetExtension(path));
    }
}

