internal readonly record struct TournamentFrameworkModeContext(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating);
