internal readonly record struct StandardModeContext(
    TournamentRuleSetMode TournamentRuleSetMode,
    double BlackAdvantagePercent,
    double BlackAdvantageRating,
    IReadOnlyList<Player> AllParticipants,
    IReadOnlyList<Player> Participants,
    IReadOnlyList<Match> Matches)
{
    internal int ExcludedParticipantCount => AllParticipants.Count - Participants.Count;
}

