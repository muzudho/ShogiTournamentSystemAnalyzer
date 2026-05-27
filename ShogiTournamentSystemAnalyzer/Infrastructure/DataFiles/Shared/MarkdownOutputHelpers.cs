/*
 * ［インフラストラクチャー　＞　データファイル　＞　共通ヘルパー］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;

internal static class MarkdownOutputHelpers
{
    internal static string BuildMarkdownFileLink(string markdownPath, string targetPath)
    {
        var markdownDirectory = Path.GetDirectoryName(Path.GetFullPath(markdownPath)) ?? Path.GetFullPath(".");
        var fullTargetPath = Path.GetFullPath(targetPath);
        var relativePath = Path.GetRelativePath(markdownDirectory, fullTargetPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
        var fileName = Path.GetFileName(targetPath);
        return $"[{fileName}]({relativePath})";
    }

    internal static string BuildMermaidCategoryList(IEnumerable<string> labels)
    {
        return string.Join(", ", labels.Select(EscapeMermaidLabel));
    }

    static string EscapeMermaidLabel(string label)
    {
        return "\"" + label.Replace("\"", "'") + "\"";
    }
}
