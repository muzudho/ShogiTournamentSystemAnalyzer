/*
 * ［品質評価］のモデル定義
 */

/// <summary>
/// ［品質評価　＞　選手の行］だ。
/// </summary>
/// <param name="Name"></param>
/// <param name="Group"></param>
/// <param name="OriginalRating"></param>
/// <param name="EloRank"></param>
/// <param name="ExpectedOverallPlace"></param>
/// <param name="OverallPlaceDeltaFromEloRank"></param>
/// <param name="OverallTop1Probability"></param>
/// <param name="OverallTop8Probability"></param>
readonly record struct QualityPlayerRow(
    string Name,
    string Group,
    double OriginalRating,
    int EloRank,
    double ExpectedOverallPlace,
    double OverallPlaceDeltaFromEloRank,
    double OverallTop1Probability,
    double OverallTop8Probability);

/// <summary>
/// ［品質評価　＞　合計］だ。
/// </summary>
/// <param name="SpearmanCorrelation"></param>
/// <param name="MeanAbsoluteRankError"></param>
/// <param name="AverageTop8Retention"></param>
/// <param name="EloTop1OverallTop1Probability"></param>
/// <param name="MostPenalizedPlayerName"></param>
/// <param name="MostPenalizedDelta"></param>
/// <param name="MostAdvantagedPlayerName"></param>
/// <param name="MostAdvantagedDelta"></param>
readonly record struct QualitySummary(
    double SpearmanCorrelation,
    double MeanAbsoluteRankError,
    double AverageTop8Retention,
    double EloTop1OverallTop1Probability,
    string MostPenalizedPlayerName,
    double MostPenalizedDelta,
    string MostAdvantagedPlayerName,
    double MostAdvantagedDelta);

/// <summary>
/// ［品質評価　＞　実行］だ。
/// </summary>
/// <param name="PlayerRows">品質評価での選手の各行</param>
/// <param name="Summary">品質評価の合計</param>
/// <param name="CalculationMode">計算モード</param>
readonly record struct QualityEvaluationRun(
    IReadOnlyList<QualityPlayerRow> PlayerRows,
    QualitySummary Summary,
    string CalculationMode);

/// <summary>
/// ［品質評価　＞　スイープオプション］だ。
/// </summary>
/// <param name="IsEnabled"></param>
/// <param name="StartPercent"></param>
/// <param name="EndPercent"></param>
/// <param name="StepPercent"></param>
readonly record struct QualitySweepOptions(
    bool IsEnabled,
    double StartPercent,
    double EndPercent,
    double StepPercent);

/// <summary>
/// ［品質評価　＞　スイープ行］だ。
/// </summary>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="SpearmanCorrelation"></param>
/// <param name="MeanAbsoluteRankError"></param>
/// <param name="AverageTop8Retention"></param>
/// <param name="EloTop1OverallTop1Probability"></param>
/// <param name="MostPenalizedPlayerName"></param>
/// <param name="MostPenalizedDelta"></param>
/// <param name="MostAdvantagedPlayerName"></param>
/// <param name="MostAdvantagedDelta"></param>
readonly record struct QualitySweepRow(
    double FirstPlayerWinRatePercent,
    double SpearmanCorrelation,
    double MeanAbsoluteRankError,
    double AverageTop8Retention,
    double EloTop1OverallTop1Probability,
    string MostPenalizedPlayerName,
    double MostPenalizedDelta,
    string MostAdvantagedPlayerName,
    double MostAdvantagedDelta);

/// <summary>
/// ［品質評価　＞　実験的レポートグルーピングオプション］だ。
/// </summary>
/// <param name="IsEnabled"></param>
/// <param name="Outcome"></param>
/// <param name="EvaluationMemo"></param>
readonly record struct ExperimentalReportGroupingOptions(
    bool IsEnabled,
    ExperimentalReportOutcome? Outcome,
    string EvaluationMemo);

