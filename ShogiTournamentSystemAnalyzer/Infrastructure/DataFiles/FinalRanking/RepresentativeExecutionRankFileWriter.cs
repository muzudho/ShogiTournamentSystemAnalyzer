/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries.FinalRanking.DataDefinitions;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System.Globalization;

/// <summary>
/// ［最終順位］境界のrepresentative順位表データファイル書き出し処理
/// </summary>
internal static class RepresentativeExecutionRankFileWriter
{
    const string RepresentativeExecutionRankTableTypeFileName = "RepresentativeExecutionRankTableType.json";

    static IReadOnlyList<string>? representativeExecutionRankFixedColumns;

    internal static IEnumerable<string> CreateCsv(
        TournamentRuleSetMode tournamentRuleSetMode,
        IReadOnlyList<RepresentativeExecutionRankRow> rows,
        string? overviewNote = null)
    {
        var specificHeaderColumns = GetRepresentativeExecutionRankFixedColumns().ToList();
        if (string.IsNullOrWhiteSpace(overviewNote)) specificHeaderColumns.Remove("note");

        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(specificHeaderColumns).Select(CsvOutputHelpers.EscapeCsv))
        };

        foreach (var row in rows)
        {
            var specificColumns = new List<string>
            {
                TournamentRuleSetRule.GetLabel(tournamentRuleSetMode),
                row.Name,
                row.Points.ToString(CultureInfo.InvariantCulture),
                row.RankLabel,
                row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture),
                (row.FirstPlaceProbability * 100).ToString("F2", CultureInfo.InvariantCulture)
            };

            if (!string.IsNullOrWhiteSpace(overviewNote))
            {
                specificColumns.Add(overviewNote);
            }

            var columns = CsvSchemaCommonColumns.BuildRowColumns(
                boundaryName: "FinalRanking",
                schemaName: "representativeExecutionRank",
                rowType: "data",
                specificColumns.ToArray());

            lines.Add(string.Join(",", columns.Select(CsvOutputHelpers.EscapeCsv)));
        }

        return lines;
    }

    internal static IEnumerable<string> CreateMarkdown(
        string outputMarkdownPath,
        string outputCsvPath,
        TournamentRuleSetMode tournamentRuleSetMode,
        IReadOnlyList<RepresentativeExecutionRankRow> rows,
        string? overviewNote = null,
        string? representativeMatchRecordsMarkdownPath = null)
    {
        var bestRow = rows
            .OrderBy(row => row.AveragePlace)
            .ThenByDescending(row => row.Points)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        var lines = new List<string>
        {
            "# representative順位表",
            string.Empty,
            "## 概要",
            $"- 結果CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 順位ルール: {TournamentRuleSetRule.GetLabel(tournamentRuleSetMode)}",
            $"- 対象選手数: {rows.Count}"
        };

        if (!string.IsNullOrWhiteSpace(representativeMatchRecordsMarkdownPath))
        {
            lines.Add($"- representative大会最終状態: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, representativeMatchRecordsMarkdownPath)}");
        }

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            lines.Add($"- 注記: {overviewNote}");
        }

        if (rows.Count > 0)
        {
            lines.AddRange(new[]
            {
                string.Empty,
                "## 注目ポイント",
                $"- representative 1位帯の先頭表示: **{bestRow.Name}**",
                $"- 勝点: **{bestRow.Points.ToString(CultureInfo.InvariantCulture)}**",
                $"- 順位帯: **{bestRow.RankLabel}**"
            });
        }

        lines.AddRange(new[]
        {
            string.Empty,
            "## 一覧表",
            "| 対局者 | 勝点 | 順位帯 | 平均順位 | 1位確率 |",
            "| --- | ---: | ---: | ---: | ---: |"
        });

        lines.AddRange(rows.Select(row =>
            $"| {row.Name} | {row.Points.ToString(CultureInfo.InvariantCulture)} | {row.RankLabel} | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} | {(row.FirstPlaceProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% |"));

        return lines;
    }

    static IReadOnlyList<string> GetRepresentativeExecutionRankFixedColumns()
    {
        return representativeExecutionRankFixedColumns ??= TableTypeDefinitionReader.Load(RepresentativeExecutionRankTableTypeFileName)
            .Data
            .Select(column => column.Name)
            .ToArray();
    }
}