sealed record class TournamentFrameworkInputDefinition(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed);
