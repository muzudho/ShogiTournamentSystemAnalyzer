using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static partial class Program
{
    static bool ValidateFinalStageParticipants(IReadOnlyList<Player> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (participants.Count != 16) { errorMessage = $"本戦参加者は 16 名で入力してください。現在は {participants.Count} 名です。"; return false; }

        if (groupMap.Count != participants.Count) { errorMessage = $"グループ対応CSVの人数が一致していません。選手一覧CSVは {participants.Count} 名、グループ対応CSVは {groupMap.Count} 名です。"; return false; }

        foreach (var participant in participants)
        {
            if (!groupMap.ContainsKey(participant.Name)) { errorMessage = $"選手 '{participant.Name}' のグループが指定されていません。"; return false; }
        }

        var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
        if (apexCount > 8) { errorMessage = $"Apex は 8 名以下で入力してください。現在は {apexCount} 名です。"; return false; }

        return true;
    }

    static bool ValidateFinalStageParticipants(IReadOnlyList<Player> participants, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (participants.Count != 16) { errorMessage = $"本戦参加者は 16 名で入力してください。現在は {participants.Count} 名です。"; return false; }

        return true;
    }

    static bool ValidateAdditionalApexParticipants(IReadOnlyList<Player> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, IReadOnlyList<Player> additionalApexParticipants, out string errorMessage)
    {
        errorMessage = string.Empty;

        var knownNames = new HashSet<string>(participants.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var participant in additionalApexParticipants)
        {
            if (knownNames.Contains(participant.Name)) { errorMessage = $"本戦不出場Apex一覧の選手 '{participant.Name}' は本戦参加者と重複しています。"; return false; }

            if (groupMap.ContainsKey(participant.Name)) { errorMessage = $"本戦不出場Apex一覧の選手 '{participant.Name}' はグループ対応CSVにも含まれています。"; return false; }
        }

        return true;
    }

    static bool ValidateAdditionalApexParticipants(IReadOnlyList<Player> participants, IReadOnlyList<Player> additionalApexParticipants, out string errorMessage)
    {
        errorMessage = string.Empty;

        var knownNames = new HashSet<string>(participants.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var participant in additionalApexParticipants)
        {
            if (knownNames.Contains(participant.Name)) { errorMessage = $"本戦不出場Apex一覧の選手 '{participant.Name}' は本戦参加者と重複しています。"; return false; }
        }

        return true;
    }

    static bool ValidateFinalStageMatches(IReadOnlyList<Player> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, IReadOnlyList<Match> matches, out string errorMessage)
    {
        errorMessage = string.Empty;

        for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
        {
            var match = matches[matchIndex];
            var blackParticipant = participants[match.FirstPlayer];
            var whiteParticipant = participants[match.SecondPlayer];

            var blackGroup = groupMap[blackParticipant.Name];
            var whiteGroup = groupMap[whiteParticipant.Name];

            if (blackGroup == whiteGroup) { errorMessage = $"{matchIndex + 1} 局目の対局 '{blackParticipant.Name} vs {whiteParticipant.Name}' は同グループ同士です。"; return false; }

            if (blackGroup != FinalStageGroup.Innov) { errorMessage = $"{matchIndex + 1} 局目の黒番 '{blackParticipant.Name}' は Innov である必要があります。"; return false; }

            if (whiteGroup != FinalStageGroup.Apex) { errorMessage = $"{matchIndex + 1} 局目の白番 '{whiteParticipant.Name}' は Apex である必要があります。"; return false; }
        }

        return true;
    }

    static bool ValidateFinalStageMatches(IReadOnlyList<Player> participants, IReadOnlyList<Match> matches, out string errorMessage)
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

