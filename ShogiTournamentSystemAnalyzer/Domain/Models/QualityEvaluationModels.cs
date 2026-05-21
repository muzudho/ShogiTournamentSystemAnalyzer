/*
 * ［大会品質評価フロー］で使われるモデルの定義
 */

/// <summary>
/// ［大会品質評価フロー　＞　選手の行］だ。
/// </summary>
/// <param name="Name">選手名</param>
/// <param name="Group">グループ名</param>
/// <param name="OriginalRating">元のレーティング</param>
/// <param name="EloRank">Eloランク</param>
/// <param name="ExpectedOverallPlace">予想総合順位</param>
/// <param name="OverallPlaceDeltaFromEloRank">Eloランクからの総合順位の差</param>
/// <param name="OverallTop1Probability">総合トップ1の確率</param>
/// <param name="OverallTop8Probability">総合トップ8の確率</param>
readonly record struct TournamentQualityReportPlayerRow(
    string Name,
    string Group,
    double OriginalRating,
    int EloRank,
    double ExpectedOverallPlace,
    double OverallPlaceDeltaFromEloRank,
    double OverallTop1Probability,
    double OverallTop8Probability);

/// <summary>
/// ［大会品質評価フロー　＞　合計］だ。
/// </summary>
/// <param name="SpearmanCorrelation">スピアマン相関係数</param>
/// <param name="MeanAbsoluteRankError">平均絶対順位誤差</param>
/// <param name="AverageTop8Retention">平均トップ8保持率</param>
/// <param name="EloTop1OverallTop1Probability">Eloトップ1の総合トップ1確率</param>
/// <param name="MostPenalizedPlayerName">最もペナルティを受けた選手名</param>
/// <param name="MostPenalizedDelta">最もペナルティを受けた選手の差分</param>
/// <param name="MostAdvantagedPlayerName">最も有利になった選手名</param>
/// <param name="MostAdvantagedDelta">最も有利になった選手の差分</param>
readonly record struct TournamentQualityReportSummary(
    double SpearmanCorrelation,
    double MeanAbsoluteRankError,
    double AverageTop8Retention,
    double EloTop1OverallTop1Probability,
    string MostPenalizedPlayerName,
    double MostPenalizedDelta,
    string MostAdvantagedPlayerName,
    double MostAdvantagedDelta);

/// <summary>
/// ［大会品質評価フロー　＞　実行］だ。
/// </summary>
/// <param name="PlayerRows">品質評価での選手の各行</param>
/// <param name="Summary">品質評価の合計</param>
/// <param name="CalculationMode">計算モード</param>
readonly record struct TournamentQualityReportRun(
    IReadOnlyList<TournamentQualityReportPlayerRow> PlayerRows,
    TournamentQualityReportSummary Summary,
    string CalculationMode);

/// <summary>
/// ［大会品質評価フロー　＞　スイープオプション］だ。
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
/// ［大会品質評価フロー　＞　スイープ行］だ。
/// </summary>
/// <param name="FirstPlayerWinRatePercent">先手勝率(%)</param>
/// <param name="SpearmanCorrelation">スピアマン相関係数</param>
/// <param name="MeanAbsoluteRankError">平均絶対順位誤差</param>
/// <param name="AverageTop8Retention">平均トップ8保持率</param>
/// <param name="EloTop1OverallTop1Probability">Eloトップ1の総合トップ1確率</param>
/// <param name="MostPenalizedPlayerName">最もペナルティを受けた選手名</param>
/// <param name="MostPenalizedDelta">最もペナルティを受けた選手の差分</param>
/// <param name="MostAdvantagedPlayerName">最も有利になった選手名</param>
/// <param name="MostAdvantagedDelta">最も有利になった選手の差分</param>
readonly record struct TournamentQualitySweepReportRow(
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
/// ［大会品質評価フロー　＞　実験的レポートグルーピングオプション］だ。
/// </summary>
/// <param name="IsEnabled"></param>
/// <param name="Outcome"></param>
/// <param name="EvaluationMemo"></param>
readonly record struct ExperimentalReportGroupingOptions(
    bool IsEnabled,
    ExperimentalReportOutcome? Outcome,
    string EvaluationMemo);

