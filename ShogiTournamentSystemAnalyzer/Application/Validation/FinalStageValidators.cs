/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Validation;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static class FinalStageValidators
{
    internal static bool ValidateFinalStagePlayers(IReadOnlyList<Player> players, IReadOnlyDictionary<string, FinalStageGroup> groupMap, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (players.Count != 16) { errorMessage = $"本戦参加者は 16 名で入力してください。現在は {players.Count} 名です。"; return false; }

        if (groupMap.Count != players.Count) { errorMessage = $"グループ対応CSVの人数が一致していません。選手一覧CSVは {players.Count} 名、グループ対応CSVは {groupMap.Count} 名です。"; return false; }

        foreach (var player in players)
        {
            if (!groupMap.ContainsKey(player.Name)) { errorMessage = $"選手 '{player.Name}' のグループが指定されていません。"; return false; }
        }

        var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
        if (apexCount > 8) { errorMessage = $"Apex は 8 名以下で入力してください。現在は {apexCount} 名です。"; return false; }

        return true;
    }

    internal static bool ValidateFinalStagePlayers(IReadOnlyList<Player> players, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (players.Count != 16) { errorMessage = $"本戦参加者は 16 名で入力してください。現在は {players.Count} 名です。"; return false; }

        return true;
    }

    internal static bool ValidateAdditionalApexPlayers(IReadOnlyList<Player> players, IReadOnlyDictionary<string, FinalStageGroup> groupMap, IReadOnlyList<Player> additionalApexPlayers, out string errorMessage)
    {
        errorMessage = string.Empty;

        var knownNames = new HashSet<string>(players.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var player in additionalApexPlayers)
        {
            if (knownNames.Contains(player.Name)) { errorMessage = $"本戦不出場Apex一覧の選手 '{player.Name}' は本戦参加者と重複しています。"; return false; }

            if (groupMap.ContainsKey(player.Name)) { errorMessage = $"本戦不出場Apex一覧の選手 '{player.Name}' はグループ対応CSVにも含まれています。"; return false; }
        }

        return true;
    }

    internal static bool ValidateAdditionalApexPlayers(IReadOnlyList<Player> players, IReadOnlyList<Player> additionalApexPlayers, out string errorMessage)
    {
        errorMessage = string.Empty;

        var knownNames = new HashSet<string>(players.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var player in additionalApexPlayers)
        {
            if (knownNames.Contains(player.Name)) { errorMessage = $"本戦不出場Apex一覧の選手 '{player.Name}' は本戦参加者と重複しています。"; return false; }
        }

        return true;
    }

    internal static bool ValidateFinalStageMatches(IReadOnlyList<Player> players, IReadOnlyDictionary<string, FinalStageGroup> groupMap, IReadOnlyList<Match> matches, out string errorMessage)
    {
        errorMessage = string.Empty;

        for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
        {
            var match = matches[matchIndex];
            var blackPlayer = players[match.FirstPlayer];
            var whitePlayer = players[match.SecondPlayer];

            var blackGroup = groupMap[blackPlayer.Name];
            var whiteGroup = groupMap[whitePlayer.Name];

            if (blackGroup == whiteGroup) { errorMessage = $"{matchIndex + 1} 局目の対局 '{blackPlayer.Name} vs {whitePlayer.Name}' は同グループ同士です。"; return false; }

            if (blackGroup != FinalStageGroup.Innov) { errorMessage = $"{matchIndex + 1} 局目の黒番 '{blackPlayer.Name}' は Innov である必要があります。"; return false; }

            if (whiteGroup != FinalStageGroup.Apex) { errorMessage = $"{matchIndex + 1} 局目の白番 '{whitePlayer.Name}' は Apex である必要があります。"; return false; }
        }

        return true;
    }

    internal static bool ValidateFinalStageMatches(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, out string errorMessage)
    {
        errorMessage = string.Empty;

        for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
        {
            var match = matches[matchIndex];
            if (match.FirstPlayer == match.SecondPlayer) { errorMessage = $"{matchIndex + 1} 局目で同じ選手が先手と後手の両方に指定されています。"; return false; }
        }

        return true;
    }
}

