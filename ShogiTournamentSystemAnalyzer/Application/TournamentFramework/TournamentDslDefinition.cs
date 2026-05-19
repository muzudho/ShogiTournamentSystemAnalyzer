sealed record class TournamentDslDefinition(
    string TimeAxis,
    string DefaultMatchResultResolver,
    IReadOnlyList<StageEntry> Stages,
    IReadOnlyList<string> FlowSteps,
    IReadOnlyDictionary<int, string> PairingRuleNames,
    string OverallRankingRuleName,
    string TerminationRuleName);
