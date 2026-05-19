internal static partial class Program
{
    static bool TryParsePlayerEntries(IReadOnlyList<string> lines, out List<PlayerEntry> players, out string errorMessage)
    {
        players = new List<PlayerEntry>();
        errorMessage = string.Empty;
        if (lines.Count == 0)
        {
            errorMessage = "選手入力がありません。";
            return false;
        }

        var startIndex = 0;
        var firstColumns = SplitCsvLine(lines[0]);
        if (LooksLikePlayerEntryHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = SplitCsvLine(lines[i]);
            if (columns.Count < 3)
            {
                errorMessage = $"{i + 1} 行目は 3 列以上必要です。";
                return false;
            }

            if (!int.TryParse(columns[0].Trim(), out var playerId))
            {
                errorMessage = $"{i + 1} 行目の playerId を整数で入力してください。";
                return false;
            }

            var name = columns[1].Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = $"{i + 1} 行目の名前が空です。";
                return false;
            }

            if (!TryParseDouble(columns[2], out var rating))
            {
                errorMessage = $"{i + 1} 行目の rating を数値で入力してください。";
                return false;
            }

            players.Add(new PlayerEntry(playerId, name, rating));
        }

        if (players.Count == 0)
        {
            errorMessage = "選手は 1 人以上必要です。";
            return false;
        }

        if (players.GroupBy(player => player.PlayerId).Any(group => group.Count() > 1))
        {
            errorMessage = "playerId が重複しています。";
            return false;
        }

        return true;
    }

    static bool TryParseStageEntries(IReadOnlyList<string> lines, out List<StageEntry> stages, out string errorMessage)
    {
        stages = new List<StageEntry>();
        errorMessage = string.Empty;
        if (lines.Count == 0)
        {
            errorMessage = "ステージ入力がありません。";
            return false;
        }

        var startIndex = 0;
        var firstColumns = SplitCsvLine(lines[0]);
        if (LooksLikeStageEntryHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = SplitCsvLine(lines[i]);
            if (columns.Count < 5)
            {
                errorMessage = $"{i + 1} 行目は 5 列以上必要です。";
                return false;
            }

            if (!int.TryParse(columns[0].Trim(), out var stageId))
            {
                errorMessage = $"{i + 1} 行目の stageId を整数で入力してください。";
                return false;
            }

            var stageName = columns[1].Trim();
            var stageType = columns[2].Trim();
            int? parentStageId = null;
            var parentStageIdText = columns[3].Trim();
            if (!string.IsNullOrWhiteSpace(parentStageIdText))
            {
                if (!int.TryParse(parentStageIdText, out var parentStageIdValue))
                {
                    errorMessage = $"{i + 1} 行目の parentStageId を整数で入力してください。";
                    return false;
                }

                parentStageId = parentStageIdValue;
            }

            if (!int.TryParse(columns[4].Trim(), out var orderNo))
            {
                errorMessage = $"{i + 1} 行目の orderNo を整数で入力してください。";
                return false;
            }

            stages.Add(new StageEntry(stageId, stageName, stageType, parentStageId, orderNo));
        }

        if (stages.Count == 0)
        {
            errorMessage = "ステージは 1 件以上必要です。";
            return false;
        }

        if (stages.GroupBy(stage => stage.StageId).Any(group => group.Count() > 1))
        {
            errorMessage = "stageId が重複しています。";
            return false;
        }

        return true;
    }

    static bool TryParseTournamentMatchRecords(IReadOnlyList<string> lines, out List<TournamentMatchRecord> matches, out string errorMessage)
    {
        matches = new List<TournamentMatchRecord>();
        errorMessage = string.Empty;
        if (lines.Count == 0)
        {
            errorMessage = "対局記録入力がありません。";
            return false;
        }

        var startIndex = 0;
        var firstColumns = SplitCsvLine(lines[0]);
        if (LooksLikeTournamentMatchRecordHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = SplitCsvLine(lines[i]);
            if (columns.Count < 9)
            {
                errorMessage = $"{i + 1} 行目は 9 列以上必要です。";
                return false;
            }

            if (!int.TryParse(columns[0].Trim(), out var matchId)
                || !int.TryParse(columns[1].Trim(), out var stageId)
                || !int.TryParse(columns[2].Trim(), out var firstPlayerId)
                || !int.TryParse(columns[3].Trim(), out var secondPlayerId)
                || !int.TryParse(columns[4].Trim(), out var startTime)
                || !int.TryParse(columns[5].Trim(), out var endTime))
            {
                errorMessage = $"{i + 1} 行目の ID / time 列を整数で入力してください。";
                return false;
            }

            if (!TryParseMatchStatus(columns[6].Trim(), out var status))
            {
                errorMessage = $"{i + 1} 行目の status '{columns[6].Trim()}' が不正です。";
                return false;
            }

            if (!TryParseMatchResultType(columns[7].Trim(), out var resultType))
            {
                errorMessage = $"{i + 1} 行目の resultType '{columns[7].Trim()}' が不正です。";
                return false;
            }

            int? roundNo = null;
            var roundText = columns[8].Trim();
            if (!string.IsNullOrWhiteSpace(roundText))
            {
                if (!int.TryParse(roundText, out var roundNoValue))
                {
                    errorMessage = $"{i + 1} 行目の roundNo を整数で入力してください。";
                    return false;
                }

                roundNo = roundNoValue;
            }

            matches.Add(new TournamentMatchRecord(matchId, stageId, firstPlayerId, secondPlayerId, startTime, endTime, status, resultType, roundNo));
        }

        if (matches.Count == 0)
        {
            errorMessage = "対局記録は 1 件以上必要です。";
            return false;
        }

        if (matches.GroupBy(match => match.MatchId).Any(group => group.Count() > 1))
        {
            errorMessage = "matchId が重複しています。";
            return false;
        }

        return true;
    }

    static bool TryParseMatchStatus(string value, out MatchStatus status)
    {
        if (Enum.TryParse<MatchStatus>(value, ignoreCase: true, out status))
        {
            return true;
        }

        status = default;
        return false;
    }

    static bool TryParseMatchResultType(string value, out MatchResultType resultType)
    {
        if (Enum.TryParse<MatchResultType>(value, ignoreCase: true, out resultType))
        {
            return true;
        }

        resultType = default;
        return false;
    }

    static bool LooksLikePlayerEntryHeaderRow(IReadOnlyList<string> columns)
    {
        return columns.Count >= 3
            && columns[0].Trim().Equals("playerId", StringComparison.OrdinalIgnoreCase)
            && columns[1].Trim().Equals("name", StringComparison.OrdinalIgnoreCase);
    }

    static bool LooksLikeStageEntryHeaderRow(IReadOnlyList<string> columns)
    {
        return columns.Count >= 5
            && columns[0].Trim().Equals("stageId", StringComparison.OrdinalIgnoreCase)
            && columns[1].Trim().Equals("stageName", StringComparison.OrdinalIgnoreCase);
    }

    static bool LooksLikeTournamentMatchRecordHeaderRow(IReadOnlyList<string> columns)
    {
        return columns.Count >= 9
            && columns[0].Trim().Equals("matchId", StringComparison.OrdinalIgnoreCase)
            && columns[1].Trim().Equals("stageId", StringComparison.OrdinalIgnoreCase);
    }
}
