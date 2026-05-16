readonly record struct QualityPlayerRow(
    string Name,
    string Group,
    double OriginalRating,
    int EloRank,
    double ExpectedOverallPlace,
    double OverallPlaceDeltaFromEloRank,
    double OverallTop1Probability,
    double OverallTop8Probability);

readonly record struct QualitySummary(
    double SpearmanCorrelation,
    double MeanAbsoluteRankError,
    double AverageTop8Retention,
    double EloTop1OverallTop1Probability,
    string MostPenalizedPlayerName,
    double MostPenalizedDelta,
    string MostAdvantagedPlayerName,
    double MostAdvantagedDelta);

readonly record struct QualityEvaluationRun(
    IReadOnlyList<QualityPlayerRow> PlayerRows,
    QualitySummary Summary,
    string CalculationMode);

readonly record struct QualitySweepOptions(
    bool IsEnabled,
    double StartPercent,
    double EndPercent,
    double StepPercent);

readonly record struct QualitySweepRow(
    double BlackAdvantagePercent,
    double SpearmanCorrelation,
    double MeanAbsoluteRankError,
    double AverageTop8Retention,
    double EloTop1OverallTop1Probability,
    string MostPenalizedPlayerName,
    double MostPenalizedDelta,
    string MostAdvantagedPlayerName,
    double MostAdvantagedDelta);

readonly record struct ExperimentalReportGroupingOptions(
    bool IsEnabled,
    ExperimentalReportOutcome? Outcome,
    string EvaluationMemo);

