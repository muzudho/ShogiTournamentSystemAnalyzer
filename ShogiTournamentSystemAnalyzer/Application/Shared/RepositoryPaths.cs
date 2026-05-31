/*
 * ［アプリケーション　＞　共有　＞　リポジトリーパス］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

internal static class RepositoryPaths
{
    internal static string RootPath => ResolveRootPath();

    internal static string DataPath => Path.Combine(RootPath, "Data");

    internal static string InputsPath => Path.Combine(RootPath, "Inputs");

    internal static string OutputPath => Path.Combine(RootPath, "Output");

    static string ResolveRootPath()
    {
        var candidates = EnumerateSelfAndParents(Path.GetFullPath("."))
            .Concat(EnumerateSelfAndParents(Path.GetFullPath(AppContext.BaseDirectory)));

        foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Directory.Exists(Path.Combine(path, ".git"))
                || File.Exists(Path.Combine(path, "ShogiTournamentSystemAnalyzer.slnx")))
            {
                return path;
            }
        }

        return Path.GetFullPath(".");
    }

    static IEnumerable<string> EnumerateSelfAndParents(string startPath)
    {
        var current = startPath;
        while (!string.IsNullOrWhiteSpace(current))
        {
            yield return current;
            current = Path.GetDirectoryName(current);
        }
    }
}
