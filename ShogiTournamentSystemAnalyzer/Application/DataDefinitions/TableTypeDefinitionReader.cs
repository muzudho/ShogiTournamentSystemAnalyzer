/*
 * ［アプリケーション　＞　データ定義］
 */
namespace ShogiTournamentSystemAnalyzer.Application.DataDefinitions;

using System.Text.Json;

internal static class TableTypeDefinitionReader
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    internal static TableTypeDefinition Load(string fileName)
    {
        var fullPath = ResolveDefinitionFilePath(fileName);
        var json = File.ReadAllText(fullPath);
        var definition = JsonSerializer.Deserialize<TableTypeDefinition>(json, JsonOptions)
            ?? throw new InvalidOperationException($"テーブル定義JSONの読み取り結果が null です: {fullPath}");

        if (definition.Meta is null)
        {
            throw new InvalidOperationException($"テーブル定義JSONに Meta がありません: {fullPath}");
        }

        if (definition.Data is null)
        {
            throw new InvalidOperationException($"テーブル定義JSONに Data がありません: {fullPath}");
        }

        return definition;
    }

    static string ResolveDefinitionFilePath(string fileName)
    {
        foreach (var rootPath in EnumerateCandidateRoots())
        {
            var directPath = Path.Combine(rootPath, "Data", "Definitions", fileName);
            if (File.Exists(directPath)) return directPath;

            var nestedProjectPath = Path.Combine(rootPath, "ShogiTournamentSystemAnalyzer", "Data", "Definitions", fileName);
            if (File.Exists(nestedProjectPath)) return nestedProjectPath;
        }

        throw new FileNotFoundException($"テーブル定義JSONが見つかりません: {fileName}");
    }

    static IEnumerable<string> EnumerateCandidateRoots()
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentDirectory = Path.GetFullPath(".");
        var appBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

        foreach (var path in EnumerateSelfAndParents(currentDirectory).Concat(EnumerateSelfAndParents(appBaseDirectory)))
        {
            if (visited.Add(path)) yield return path;
        }
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