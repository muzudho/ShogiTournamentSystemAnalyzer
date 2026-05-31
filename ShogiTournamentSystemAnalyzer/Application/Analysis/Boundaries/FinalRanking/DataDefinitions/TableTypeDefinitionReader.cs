/*
 * ［アプリケーション　＞　データ定義］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries.FinalRanking.DataDefinitions;

using System.Text.Json;
using ShogiTournamentSystemAnalyzer.Application.Shared;

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
        var directPath = Path.Combine(RepositoryPaths.DataPath, "Definitions", fileName);
        if (File.Exists(directPath)) return directPath;

        throw new FileNotFoundException($"テーブル定義JSONが見つかりません: {fileName}");
    }
}
