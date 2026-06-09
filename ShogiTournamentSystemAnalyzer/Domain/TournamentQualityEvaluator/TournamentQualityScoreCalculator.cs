/*
 * ［大会品質評価フロー域　＞　総合点計算］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class TournamentQualityScoreCalculator
{
    internal static TournamentQualityScoreBreakdown Calculate(
        TournamentQualityReportSummary summary,
        TournamentQualityScoreRule rule,
        int simulationCount)
    {
        Validate(rule);

        var spearmanScore = Clamp01((summary.SpearmanCorrelation + 1.0) / 2.0);
        var meanRankErrorScore = Clamp01(1.0 - summary.MeanAbsoluteRankError / rule.MeanRankErrorTolerance);
        var top8RetentionScore = Clamp01(summary.AverageTop8Retention / 8.0);
        var eloTop1WinScore = Clamp01(summary.EloTop1OverallTop1Probability);

        var spearmanPoints = CalculatePoints(spearmanScore, rule.SpearmanWeight);
        var meanRankErrorPoints = CalculatePoints(meanRankErrorScore, rule.MeanRankErrorWeight);
        var top8RetentionPoints = CalculatePoints(top8RetentionScore, rule.Top8RetentionWeight);
        var eloTop1WinPoints = CalculatePoints(eloTop1WinScore, rule.EloTop1WinWeight);
        var totalScore = spearmanPoints + meanRankErrorPoints + top8RetentionPoints + eloTop1WinPoints;

        return new TournamentQualityScoreBreakdown(
            totalScore,
            rule.ScoreMax,
            rule.PresetName,
            TournamentQualityScoreReliability.FromSimulationCount(simulationCount),
            rule.MeanRankErrorTolerance,
            spearmanScore,
            spearmanPoints,
            rule.SpearmanWeight,
            meanRankErrorScore,
            meanRankErrorPoints,
            rule.MeanRankErrorWeight,
            top8RetentionScore,
            top8RetentionPoints,
            rule.Top8RetentionWeight,
            eloTop1WinScore,
            eloTop1WinPoints,
            rule.EloTop1WinWeight);
    }

    internal static void Validate(TournamentQualityScoreRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.PresetName)) throw new OperationCanceledException("品質評価総合点のプリセット名が空です。");
        if (rule.ScoreMax < 1) throw new OperationCanceledException("品質評価総合点の満点は 1 以上にしてください。");
        if (rule.MeanRankErrorTolerance <= 0.0) throw new OperationCanceledException("品質評価総合点の平均順位ずれ許容値は 0 より大きくしてください。");
        if (rule.SpearmanWeight < 0 || rule.MeanRankErrorWeight < 0 || rule.Top8RetentionWeight < 0 || rule.EloTop1WinWeight < 0)
        {
            throw new OperationCanceledException("品質評価総合点の重みは 0 以上にしてください。");
        }

        var weightTotal = rule.SpearmanWeight + rule.MeanRankErrorWeight + rule.Top8RetentionWeight + rule.EloTop1WinWeight;
        if (weightTotal != rule.ScoreMax)
        {
            throw new OperationCanceledException($"品質評価総合点の重み合計 {weightTotal} が満点 {rule.ScoreMax} と一致しません。");
        }
    }

    static double Clamp01(double value)
    {
        return Math.Clamp(value, 0.0, 1.0);
    }

    static int CalculatePoints(double normalizedScore, int weight)
    {
        return (int)Math.Round(normalizedScore * weight, MidpointRounding.AwayFromZero);
    }
}