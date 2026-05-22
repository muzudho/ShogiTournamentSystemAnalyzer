using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal readonly record struct StandardModeContext(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    IReadOnlyList<Player> AllParticipants,
    IReadOnlyList<Player> Participants,
    IReadOnlyList<Match> Matches)
{
    internal int ExcludedParticipantCount => AllParticipants.Count - Participants.Count;
}

