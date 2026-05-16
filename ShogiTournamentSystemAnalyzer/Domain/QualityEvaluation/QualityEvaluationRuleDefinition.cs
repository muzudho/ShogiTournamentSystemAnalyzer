internal readonly record struct QualityEvaluationRuleDefinition(
    FinalStageGroupingMode GroupingMode,
    TournamentRuleSetMode TournamentRuleSetMode,
    IReadOnlyDictionary<string, FinalStageGroup>? GroupMap,
    IReadOnlyList<Player> AdditionalApexParticipants,
    AdditionalApexPlacementMode AdditionalApexPlacementMode,
    int EffectiveAdditionalApexCount,
    BoundaryRescueMode BoundaryRescueMode,
    VariableTop8Mode VariableTop8Mode,
    int PromotedInnovCount)
{
    internal bool UsesFinalStageGrouping => GroupingMode == FinalStageGroupingMode.On;
}

