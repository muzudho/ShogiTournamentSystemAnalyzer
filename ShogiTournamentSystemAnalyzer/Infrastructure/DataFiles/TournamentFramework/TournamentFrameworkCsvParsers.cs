/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;

internal static class TournamentFrameworkCsvParsers
{
    internal static List<PlayerEntry> ReadPlayerEntriesFromCsvPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var lines = File.ReadAllLines(fullPath);
        if (!TryParsePlayerEntries(lines, out var players, out var errorMessage)) throw new OperationCanceledException($"選手一覧CSVの読み取りに失敗しました: {errorMessage} ({fullPath})");

        return players;
    }

    internal static List<StageEntry> ReadStageEntriesFromCsvPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var lines = File.ReadAllLines(fullPath);
        if (!TryParseStageEntries(lines, out var stages, out var errorMessage)) throw new OperationCanceledException($"ステージ一覧CSVの読み取りに失敗しました: {errorMessage} ({fullPath})");

        return stages;
    }

    internal static List<TournamentMatchRecord> ReadTournamentMatchRecordsFromCsvPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var lines = File.ReadAllLines(fullPath);
        if (!TryParseTournamentMatchRecords(lines, out var matches, out var errorMessage)) throw new OperationCanceledException($"大会対局記録CSVの読み取りに失敗しました: {errorMessage} ({fullPath})");

        return matches;
    }

    internal static bool TryParsePlayerEntries(IReadOnlyList<string> lines, out List<PlayerEntry> players, out string errorMessage)
    {
        players = new List<PlayerEntry>();
        errorMessage = string.Empty;
        if (lines.Count == 0) { errorMessage = "選手入力がありません。"; return false; }

        var startIndex = 0;
        var firstColumns = InputParsers.SplitCsvLine(lines[0]);
        if (LooksLikePlayerEntryHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = InputParsers.SplitCsvLine(lines[i]);
            if (columns.Count < 3) { errorMessage = $"{i + 1} 行目は 3 列以上必要です。"; return false; }
            if (!int.TryParse(columns[0].Trim(), out var playerId)) { errorMessage = $"{i + 1} 行目の playerId を整数で入力してください。"; return false; }

            var name = columns[1].Trim();
            if (string.IsNullOrWhiteSpace(name)) { errorMessage = $"{i + 1} 行目の名前が空です。"; return false; }
            if (!InputParsers.TryParseDouble(columns[2], out var rating)) { errorMessage = $"{i + 1} 行目の rating を数値で入力してください。"; return false; }

            players.Add(new PlayerEntry(playerId, name, rating));
        }

        if (players.Count == 0) { errorMessage = "選手は 1 人以上必要です。"; return false; }
        if (players.GroupBy(player => player.PlayerId).Any(group => group.Count() > 1)) { errorMessage = "playerId が重複しています。"; return false; }

        return true;
    }

    internal static bool TryParseStageEntries(IReadOnlyList<string> lines, out List<StageEntry> stages, out string errorMessage)
    {
        stages = new List<StageEntry>();
        errorMessage = string.Empty;
        if (lines.Count == 0) { errorMessage = "ステージ入力がありません。"; return false; }

        var startIndex = 0;
        var firstColumns = InputParsers.SplitCsvLine(lines[0]);
        if (LooksLikeStageEntryHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = InputParsers.SplitCsvLine(lines[i]);
            if (columns.Count < 5) { errorMessage = $"{i + 1} 行目は 5 列以上必要です。"; return false; }
            if (!int.TryParse(columns[0].Trim(), out var stageId)) { errorMessage = $"{i + 1} 行目の stageId を整数で入力してください。"; return false; }

            var stageName = columns[1].Trim();
            if (string.IsNullOrWhiteSpace(stageName)) { errorMessage = $"{i + 1} 行目の stageName が空です。"; return false; }
            var stageType = columns[2].Trim();
            if (string.IsNullOrWhiteSpace(stageType)) { errorMessage = $"{i + 1} 行目の stageType が空です。"; return false; }

            int? parentStageId = null;
            if (!string.IsNullOrWhiteSpace(columns[3]))
            {
                if (!int.TryParse(columns[3].Trim(), out var parsedParentStageId)) { errorMessage = $"{i + 1} 行目の parentStageId を整数で入力してください。"; return false; }

                parentStageId = parsedParentStageId;
            }
            if (!int.TryParse(columns[4].Trim(), out var order)) { errorMessage = $"{i + 1} 行目の orderNo を整数で入力してください。"; return false; }

            stages.Add(new StageEntry(stageId, stageName, stageType, parentStageId, order));
        }

        if (stages.Count == 0) { errorMessage = "ステージは 1 件以上必要です。"; return false; }
        if (stages.GroupBy(stage => stage.StageId).Any(group => group.Count() > 1)) { errorMessage = "stageId が重複しています。"; return false; }

        return true;
    }

    internal static bool TryParseTournamentMatchRecords(IReadOnlyList<string> lines, out List<TournamentMatchRecord> matches, out string errorMessage)
    {
        matches = new List<TournamentMatchRecord>();
        errorMessage = string.Empty;
        if (lines.Count == 0) { errorMessage = "大会対局記録がありません。"; return false; }

        var startIndex = 0;
        var firstColumns = InputParsers.SplitCsvLine(lines[0]);
        if (LooksLikeTournamentMatchRecordHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = InputParsers.SplitCsvLine(lines[i]);
            if (columns.Count < 11) { errorMessage = $"{i + 1} 行目は 11 列以上必要です。"; return false; }
            if (!int.TryParse(columns[0].Trim(), out var matchId)) { errorMessage = $"{i + 1} 行目の matchId を整数で入力してください。"; return false; }
            if (!int.TryParse(columns[1].Trim(), out var stageId)) { errorMessage = $"{i + 1} 行目の stageId を整数で入力してください。"; return false; }
            if (!int.TryParse(columns[3].Trim(), out var firstPlayerId)) { errorMessage = $"{i + 1} 行目の firstPlayerId を整数で入力してください。"; return false; }
            if (!int.TryParse(columns[5].Trim(), out var secondPlayerId)) { errorMessage = $"{i + 1} 行目の secondPlayerId を整数で入力してください。"; return false; }
            if (!int.TryParse(columns[7].Trim(), out var startTime)) { errorMessage = $"{i + 1} 行目の startTime を整数で入力してください。"; return false; }
            if (!int.TryParse(columns[8].Trim(), out var endTime)) { errorMessage = $"{i + 1} 行目の endTime を整数で入力してください。"; return false; }
            if (!Enum.TryParse<MatchStatus>(columns[9].Trim(), ignoreCase: true, out var status)) { errorMessage = $"{i + 1} 行目の status が不正です。"; return false; }
            if (!Enum.TryParse<MatchResultType>(columns[10].Trim(), ignoreCase: true, out var resultType)) { errorMessage = $"{i + 1} 行目の resultType が不正です。"; return false; }

            int? roundNo = null;
            if (columns.Count >= 12 && !string.IsNullOrWhiteSpace(columns[11]))
            {
                if (!int.TryParse(columns[11].Trim(), out var parsedRoundNo)) { errorMessage = $"{i + 1} 行目の roundNo を整数で入力してください。"; return false; }
                roundNo = parsedRoundNo;
            }

            matches.Add(new TournamentMatchRecord(matchId, stageId, firstPlayerId, secondPlayerId, startTime, endTime, status, resultType, roundNo));
        }

        return true;
    }

    static bool LooksLikePlayerEntryHeaderRow(IReadOnlyList<string> columns)
    {
        return columns.Count >= 3
            && string.Equals(columns[0].Trim(), "playerId", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[1].Trim(), "name", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[2].Trim(), "rating", StringComparison.OrdinalIgnoreCase);
    }

    static bool LooksLikeStageEntryHeaderRow(IReadOnlyList<string> columns)
    {
        return columns.Count >= 5
            && string.Equals(columns[0].Trim(), "stageId", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[1].Trim(), "stageName", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[2].Trim(), "stageType", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[3].Trim(), "parentStageId", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[4].Trim(), "orderNo", StringComparison.OrdinalIgnoreCase);
    }

    static bool LooksLikeTournamentMatchRecordHeaderRow(IReadOnlyList<string> columns)
    {
        return columns.Count >= 11
            && string.Equals(columns[0].Trim(), "matchId", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[1].Trim(), "stageId", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[3].Trim(), "firstPlayerId", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[5].Trim(), "secondPlayerId", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[9].Trim(), "status", StringComparison.OrdinalIgnoreCase)
            && string.Equals(columns[10].Trim(), "resultType", StringComparison.OrdinalIgnoreCase);
    }
}
