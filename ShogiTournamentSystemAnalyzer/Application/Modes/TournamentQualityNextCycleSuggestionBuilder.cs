/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using System.Globalization;

internal static class TournamentQualityNextCycleSuggestionBuilder
{
    internal static TournamentQualityNextCycleSuggestion BuildForSweep(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        IReadOnlyList<TournamentQualitySweepReportRow> sweepRows,
        bool stoppedByTimeout)
    {
        var playerCount = input.Players.Count;
        var matchCount = input.Matches.Count;
        var start = executionOptions.SweepOptions.StartPercent;
        var end = executionOptions.SweepOptions.EndPercent;
        var step = executionOptions.SweepOptions.StepPercent;
        var currentPointCount = Math.Max(1, (int)Math.Floor((end - start) / step) + 1);
        var completedPointCount = sweepRows.Count;
        var completionRate = Math.Clamp((double)completedPointCount / currentPointCount, 0.0, 1.0);
        var width = Math.Max(step, end - start);
        var workloadScore = CalculateWorkloadScore(playerCount, matchCount, executionOptions.SimulationCount, currentPointCount);
        var workloadScale = workloadScore switch
        {
            >= 200_000_000 => 0.60,
            >= 50_000_000 => 0.75,
            >= 10_000_000 => 0.90,
            _ => 1.00,
        };
        var widthScale = stoppedByTimeout
            ? completionRate switch
            {
                <= 0.20 => 0.25,
                <= 0.40 => 0.40,
                <= 0.70 => 0.60,
                _ => 0.80,
            }
            : 1.10 * workloadScale;
        var stepScale = stoppedByTimeout
            ? completionRate switch
            {
                <= 0.20 => 4.0,
                <= 0.40 => 3.0,
                <= 0.70 => 2.0,
                _ => 1.5,
            }
            : workloadScale < 1.0 ? 1.5 : 1.0;
        var suggestedEnd = Math.Min(100.0, start + Math.Max(step, width * widthScale));
        var suggestedStep = Math.Max(step, Math.Ceiling(step * stepScale));
        var suggestedStart = completionRate <= 0.40
            ? start
            : Math.Max(start, suggestedEnd - Math.Max(suggestedStep * 2.0, Math.Ceiling(width * 0.30)));
        var bestRow = sweepRows.Count > 0
            ? sweepRows.OrderByDescending(CalculateSweepRowScore).First()
            : default;
        var hasMeasuredBest = sweepRows.Count > 0;
        var pinpointPercent = hasMeasuredBest
            ? bestRow.FirstPlayerWinRatePercent
            : Math.Round(Math.Min(suggestedEnd, suggestedStart + (suggestedEnd - suggestedStart) / 2.0), 2);
        var focusStart = Math.Round(Math.Max(start, pinpointPercent - suggestedStep), 2);
        var focusEnd = Math.Round(Math.Min(end, pinpointPercent + suggestedStep), 2);
        var neighborPercents = BuildNeighborPercents(pinpointPercent, suggestedStep, start, end);
        var suggestedSettings = new List<string>
        {
            $"開始する先手勝率(%) = {suggestedStart.ToString("F2", CultureInfo.InvariantCulture)}",
            $"終了する先手勝率(%) = {suggestedEnd.ToString("F2", CultureInfo.InvariantCulture)}",
            $"刻み幅(%) = {suggestedStep.ToString("F2", CultureInfo.InvariantCulture)}",
            $"ベスト候補(%) = {pinpointPercent.ToString("F2", CultureInfo.InvariantCulture)}",
            $"近傍候補(%) = {string.Join(" / ", neighborPercents.Select(percent => percent.ToString("F2", CultureInfo.InvariantCulture)))}",
            $"再探索するなら範囲(%) = {focusStart.ToString("F2", CultureInfo.InvariantCulture)} ～ {focusEnd.ToString("F2", CultureInfo.InvariantCulture)}"
        };

        if (hasMeasuredBest)
        {
            suggestedSettings.Add($"実測ベースの最良値 = Spearman {bestRow.SpearmanCorrelation:F4}, 平均順位ずれ {bestRow.MeanAbsoluteRankError:F3}, Top8残留 {bestRow.AverageTop8Retention:F3}");
        }

        if (stoppedByTimeout)
        {
            var targetPointCount = Math.Max(1, (int)Math.Ceiling(currentPointCount * Math.Max(0.25, completionRate * 1.5)));
            suggestedSettings.Add($"目安の評価点数 = {targetPointCount}");
        }

        if (workloadScore >= 10_000_000)
        {
            suggestedSettings.Add($"軽量確認の目安 = 選手 {playerCount} 人 / 対局 {matchCount} 件なので、まず 1 点または 3 点だけ確認");
        }

        var reason = stoppedByTimeout
            ? $"今回の計算点数は {completedPointCount}/{currentPointCount} 件（完了率 {(completionRate * 100.0).ToString("F0", CultureInfo.InvariantCulture)}%）で、選手数 {playerCount} 人・対局数 {matchCount} 件の負荷と、計算済み範囲で最も良かった {pinpointPercent.ToString("F2", CultureInfo.InvariantCulture)}% 付近を組み合わせた案です。"
            : $"今回の範囲は完走できました。実測で最も良かった {pinpointPercent.ToString("F2", CultureInfo.InvariantCulture)}% とその近傍を次の比較候補にできます。選手数 {playerCount} 人・対局数 {matchCount} 件なので、狭い範囲で再確認しやすいです。";

        return new TournamentQualityNextCycleSuggestion("次回の n%スイープ提案", suggestedSettings, reason);
    }

    internal static TournamentQualityNextCycleSuggestion BuildForSingleRun(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        bool stoppedByTimeout,
        int completedPlayerRowCount)
    {
        var playerCount = input.Players.Count;
        var matchCount = input.Matches.Count;
        var workloadScore = CalculateWorkloadScore(playerCount, matchCount, executionOptions.SimulationCount, 1);
        var suggestedSettings = new List<string>();
        if (executionOptions.FirstPlayerWinRatePercent.HasValue)
        {
            var currentPercent = executionOptions.FirstPlayerWinRatePercent.Value;
            suggestedSettings.Add($"同Elo対局時の先手勝率(%) = {currentPercent.ToString("F2", CultureInfo.InvariantCulture)}");
            suggestedSettings.Add($"ピンポイント比較候補(%) = {Math.Round(currentPercent + 1.0, 2).ToString("F2", CultureInfo.InvariantCulture)}");
        }

        if (executionOptions.SimulationCount.HasValue)
        {
            var currentSimulationCount = executionOptions.SimulationCount.Value;
            var suggestedSimulationCount = stoppedByTimeout
                ? currentSimulationCount switch
                {
                    >= 500_000 => Math.Max(5_000, currentSimulationCount / 20),
                    >= 200_000 => Math.Max(5_000, currentSimulationCount / 10),
                    >= 50_000 => Math.Max(2_000, currentSimulationCount / 5),
                    _ => Math.Max(1_000, currentSimulationCount / 2),
                }
                : Math.Max(1_000, currentSimulationCount / 2);
            suggestedSettings.Add($"シミュレーション試行回数 = {suggestedSimulationCount:N0}");
            if (workloadScore >= 10_000_000)
            {
                var pinpointSimulationCount = Math.Max(1_000, suggestedSimulationCount / 2);
                suggestedSettings.Add($"まず一点だけ見る試行回数 = {pinpointSimulationCount:N0}");
            }
            if (stoppedByTimeout)
            {
                var quickTrialSimulationCount = Math.Max(1_000, suggestedSimulationCount / 2);
                suggestedSettings.Add($"さらに様子見するなら試行回数 = {quickTrialSimulationCount:N0}");
            }
        }

        if (playerCount >= 16 || matchCount >= 40)
        {
            suggestedSettings.Add($"軽量確認の見方 = 選手 {playerCount} 人 / 対局 {matchCount} 件では、先に 1 条件だけ再確認してから横比較");
        }

        if (suggestedSettings.Count == 0)
        {
            suggestedSettings.Add("対局数または対象条件を少し絞って再試行する");
        }

        var reason = stoppedByTimeout
            ? $"今回の品質評価は時間切れになりました。選手数 {playerCount} 人・対局数 {matchCount} 件の負荷を見て、まず一点確認しやすい試行回数まで落とす案です。取得できた選手行数は {completedPlayerRowCount} 件です。"
            : $"今回の条件で回せました。選手数 {playerCount} 人・対局数 {matchCount} 件なので、現条件とピンポイント候補を並べて比較できます。";

        return new TournamentQualityNextCycleSuggestion("次回の品質評価提案", suggestedSettings, reason);
    }

    static long CalculateWorkloadScore(
        int participantCount,
        int matchCount,
        int? simulationCount,
        int evaluationPointCount)
    {
        var simulationFactor = simulationCount ?? 1;
        return (long)Math.Max(1, participantCount)
            * Math.Max(1, matchCount)
            * Math.Max(1, simulationFactor)
            * Math.Max(1, evaluationPointCount);
    }

    static double CalculateSweepRowScore(TournamentQualitySweepReportRow row)
    {
        return row.SpearmanCorrelation * 1000.0
            + row.AverageTop8Retention * 100.0
            + row.EloTop1OverallTop1Probability * 100.0
            - row.MeanAbsoluteRankError * 120.0
            - Math.Max(0.0, row.MostPenalizedDelta) * 10.0;
    }

    static IReadOnlyList<double> BuildNeighborPercents(double centerPercent, double stepPercent, double minPercent, double maxPercent)
    {
        var offset = Math.Max(0.5, stepPercent / 2.0);
        return new[]
        {
            Math.Max(minPercent, centerPercent - offset),
            centerPercent,
            Math.Min(maxPercent, centerPercent + offset)
        }
        .Distinct()
        .OrderBy(percent => percent)
        .ToArray();
    }
}
