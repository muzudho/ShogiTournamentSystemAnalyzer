/*
 * ［インフラストラクチャー　＞　大会最終状態という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFinalState;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System.Globalization;

/// <summary>
/// ［大会最終状態］境界のデータファイルを作成するクラスだぜ（＾▽＾）！
/// </summary>
internal static class TournamentFinalStateDataFileWriter
{
    static string EscapeCsv(string value) => CsvOutputHelpers.EscapeCsv(value);

    internal static IEnumerable<string> CreateTournamentMatchRecordCsv(IReadOnlyList<StageEntry> stages, IReadOnlyList<PlayerEntry> players, IReadOnlyList<TournamentMatchRecord> matchRecords, string? overviewNote = null)
    {
        var stageNameById = stages.ToDictionary(stage => stage.StageId, stage => stage.StageName);
        var playerNameById = players.ToDictionary(player => player.PlayerId, player => player.Name);
        var specificHeaderColumns = new List<string>
        {
            "matchId",
            "stageId",
            "stageName",
            "firstPlayerId",
            "firstPlayerName",
            "secondPlayerId",
            "secondPlayerName",
            "startTime",
            "endTime",
            "status",
            "resultType",
            "roundNo"
        };

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            specificHeaderColumns.Add("note");
        }

        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(specificHeaderColumns).Select(EscapeCsv))
        };

        foreach (var match in matchRecords.OrderBy(match => match.StartTime).ThenBy(match => match.MatchId))
        {
            var stageName = stageNameById.TryGetValue(match.StageId, out var stage) ? stage : string.Empty;
            var firstPlayerName = playerNameById.TryGetValue(match.FirstPlayerId, out var firstPlayer) ? firstPlayer : string.Empty;
            var secondPlayerName = playerNameById.TryGetValue(match.SecondPlayerId, out var secondPlayer) ? secondPlayer : string.Empty;

            var specificColumns = new List<string>
            {
                match.MatchId.ToString(CultureInfo.InvariantCulture),
                match.StageId.ToString(CultureInfo.InvariantCulture),
                stageName,
                match.FirstPlayerId.ToString(CultureInfo.InvariantCulture),
                firstPlayerName,
                match.SecondPlayerId.ToString(CultureInfo.InvariantCulture),
                secondPlayerName,
                match.StartTime.ToString(CultureInfo.InvariantCulture),
                match.EndTime.ToString(CultureInfo.InvariantCulture),
                match.Status.ToString(),
                match.ResultType.ToString(),
                match.RoundNo?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            };

            if (!string.IsNullOrWhiteSpace(overviewNote))
            {
                specificColumns.Add(overviewNote);
            }

            var columns = CsvSchemaCommonColumns.BuildRowColumns(
                boundaryName: "TournamentFinalState",
                schemaName: "tournamentMatchRecord",
                rowType: "data",
                specificColumns.ToArray());

            lines.Add(string.Join(",", columns.Select(EscapeCsv)));
        }

        return lines;
    }

    internal static IEnumerable<string> CreateTournamentMatchRecordMarkdown(
        string outputMarkdownPath,
        string outputCsvPath,
        IReadOnlyList<StageEntry> stages,
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyList<TournamentMatchRecord> matchRecords,
        string? overviewNote = null,
        string? aggregateResultMarkdownPath = null,
        string? representativeRankingMarkdownPath = null)
    {
        var stageNameById = stages.ToDictionary(stage => stage.StageId, stage => stage.StageName);
        var playerNameById = players.ToDictionary(player => player.PlayerId, player => player.Name);
        var orderedMatches = matchRecords
            .OrderBy(match => match.StartTime)
            .ThenBy(match => match.MatchId)
            .ToArray();
        var finishedCount = orderedMatches.Count(match => match.Status == MatchStatus.Finished);
        var cancelledCount = orderedMatches.Count(match => match.Status == MatchStatus.Cancelled);

        var lines = new List<string>
        {
            "# 大会最終状態テーブル",
            string.Empty,
            "## 概要",
            $"- 結果CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 総対局数: {orderedMatches.Length}",
            $"- ステージ数: {stages.Count}",
            $"- 完了対局数: {finishedCount}",
            $"- 中止対局数: {cancelledCount}",
            string.Empty,
            "## 一覧表",
            "| 対局番号 | ステージ | 先手 | 後手 | 開始 | 終了 | 状態 | 結果 | ラウンド |",
            "| ---: | --- | --- | --- | ---: | ---: | --- | --- | ---: |"
        };

        if (!string.IsNullOrWhiteSpace(aggregateResultMarkdownPath))
        {
            lines.Insert(8, $"- aggregate順位表: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, aggregateResultMarkdownPath)}");
        }

        if (!string.IsNullOrWhiteSpace(representativeRankingMarkdownPath))
        {
            lines.Insert(8, $"- representative順位表: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, representativeRankingMarkdownPath)}");
        }

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            lines.Insert(8, $"- 注記: {overviewNote}");
        }

        foreach (var match in orderedMatches)
        {
            var stageName = stageNameById.TryGetValue(match.StageId, out var stage) ? stage : match.StageId.ToString(CultureInfo.InvariantCulture);
            var firstPlayerName = playerNameById.TryGetValue(match.FirstPlayerId, out var firstPlayer) ? firstPlayer : match.FirstPlayerId.ToString(CultureInfo.InvariantCulture);
            var secondPlayerName = playerNameById.TryGetValue(match.SecondPlayerId, out var secondPlayer) ? secondPlayer : match.SecondPlayerId.ToString(CultureInfo.InvariantCulture);
            var roundText = match.RoundNo?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            lines.Add($"| {match.MatchId.ToString(CultureInfo.InvariantCulture)} | {stageName} | {firstPlayerName} | {secondPlayerName} | {match.StartTime.ToString(CultureInfo.InvariantCulture)} | {match.EndTime.ToString(CultureInfo.InvariantCulture)} | {match.Status} | {match.ResultType} | {roundText} |");
        }

        return lines;
    }
}
