internal readonly record struct TournamentFrameworkModeContext(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed,
    int? SimulationCount,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    string? OutputPath);
