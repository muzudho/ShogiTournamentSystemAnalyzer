internal readonly record struct FinalStageModeContext(
    double BlackAdvantagePercent,
    double BlackAdvantageRating,
    IReadOnlyList<Participant> Participants,
    FinalStageGroupingMode GroupingMode,
    TournamentRuleSetMode TournamentRuleSetMode,
    IReadOnlyDictionary<string, FinalStageGroup>? GroupMap,
    IReadOnlyList<Participant> AdditionalApexParticipants,
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
