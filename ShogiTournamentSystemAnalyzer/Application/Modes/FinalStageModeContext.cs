internal readonly record struct FinalStageModeContext(
    double BlackAdvantagePercent,
    double BlackAdvantageRating,
    IReadOnlyList<Player> Participants,
    FinalStageGroupingMode GroupingMode,
    TournamentRuleSetMode TournamentRuleSetMode,
    IReadOnlyDictionary<string, FinalStageGroup>? GroupMap,
    IReadOnlyList<Player> AdditionalApexParticipants,
    AdditionalApexPlacementMode AdditionalApexPlacementMode,
    int EffectiveAdditionalApexCount,
    BoundaryRescueMode BoundaryRescueMode,
    int ApexCount,
    int InnovCount,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches)
{
    internal bool UsesFinalStageGrouping => GroupingMode == FinalStageGroupingMode.On;
}

