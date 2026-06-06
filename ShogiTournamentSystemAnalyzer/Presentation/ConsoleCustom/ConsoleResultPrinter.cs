/*
 * ［プレゼンテーション　＞　コンソール改］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System;
using System.Globalization;

internal static class ConsoleResultPrinter
{
    static void PrintAdjustmentCycleGuide(string scenarioLabel, bool timedOut, bool zeroResults, int? evaluatedPointCount = null)
    {
        if (!timedOut && !zeroResults) return;

        Console.WriteLine("調整サイクル用メモ:");
        Console.WriteLine($"- 今回の {scenarioLabel} は {(zeroResults ? "0件" : "途中まで")} で止まりました。");
        if (evaluatedPointCount.HasValue)
        {
            Console.WriteLine($"- 今回計算できた点数: {evaluatedPointCount.Value}");
        }

        Console.WriteLine("- 次回は次のどれかを短くすると回しやすいです。");
        Console.WriteLine("  1. 先手勝率の終了値を手前にする");
        Console.WriteLine("  2. 刻み幅を大きくする");
        Console.WriteLine("  3. シミュレーション試行回数を減らす");
        Console.WriteLine("  4. 対局数や対象条件を絞る\n");
    }

    static void PrintNextCycleSuggestion(TournamentQualityNextCycleSuggestion suggestion)
    {
        if (string.IsNullOrWhiteSpace(suggestion.Title)) return;

        Console.WriteLine($"{suggestion.Title}:");
        foreach (var setting in suggestion.SuggestedSettings)
        {
            Console.WriteLine($"- {setting}");
        }

        if (!string.IsNullOrWhiteSpace(suggestion.Reason))
        {
            Console.WriteLine($"- 理由: {suggestion.Reason}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データの要約表示だ。
    /// </summary>
    internal static void PrintTournamentQualityReportSummaryMetrics(TournamentQualityReportSummary summary)
    {
        Console.WriteLine("品質評価サマリー:");
        Console.WriteLine($"- Spearman 相関: {summary.SpearmanCorrelation.ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"- 平均順位ずれ: {summary.MeanAbsoluteRankError.ToString("F3", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"- Elo上位8名の総合上位8位残留人数（平均）: {summary.AverageTop8Retention.ToString("F3", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"- Elo1位の総合1位確率: {SimulationRatingMath.FormatPercent(summary.EloTop1OverallTop1Probability)}");
        Console.WriteLine($"- 最大不利益: {summary.MostPenalizedPlayerName} ({summary.MostPenalizedDelta.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture)})");
        Console.WriteLine($"- 最大利益: {summary.MostAdvantagedPlayerName} ({summary.MostAdvantagedDelta.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture)})\n");
    }

    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データの表示だ。
    /// </summary>
    internal static void PrintTournamentQualityReportSummary(TournamentQualityReportData tournamentQualityReportData)
    {
        PrintTournamentQualityReportSummaryMetrics(tournamentQualityReportData.Summary);
        PrintAdjustmentCycleGuide(
            scenarioLabel: "品質評価",
            timedOut: tournamentQualityReportData.CalculationMode.Contains("時間切れ", StringComparison.Ordinal),
            zeroResults: tournamentQualityReportData.PlayerRows.Count == 0);
        PrintNextCycleSuggestion(tournamentQualityReportData.Suggestion);
    }

    internal static void PrintTournamentQualityReportPlayerHighlightsRows(IReadOnlyList<TournamentQualityReportPlayerRow> playerRows)
    {
        if (playerRows.Count == 0)
        {
            Console.WriteLine("品質評価 選手別ハイライト:");
            Console.WriteLine("結果行は 0 件です。条件を短くして再試行してください。\n");
            return;
        }

        Console.WriteLine("品質評価 選手別ハイライト:");
        Console.WriteLine("Elo順位  名前                 期待総合順位   ずれ      総合1位確率   総合上位8確率");

        foreach (var row in playerRows.Take(8))
        {
            Console.WriteLine(
                row.EloRank.ToString(CultureInfo.InvariantCulture).PadLeft(6)
                + "  " + row.Name.PadRight(20)
                + row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
                + row.OverallPlaceDeltaFromEloRank.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture).PadLeft(10)
                + SimulationRatingMath.FormatPercent(row.OverallTop1Probability).PadLeft(14)
                + SimulationRatingMath.FormatPercent(row.OverallTop8Probability).PadLeft(14));
        }

        Console.WriteLine();
    }

    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データの選手別ハイライト表示だ。
    /// </summary>
    internal static void PrintTournamentQualityReportPlayerHighlights(TournamentQualityReportData tournamentQualityReportData)
    {
        PrintTournamentQualityReportPlayerHighlightsRows(tournamentQualityReportData.PlayerRows);
    }

    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データのスイープ表示だ。
    /// </summary>
    internal static void PrintTournamentQualitySweepReportRows(TournamentQualitySweepReportData tournamentQualitySweepReportData)
    {
        PrintTournamentQualitySweepReportTable(tournamentQualitySweepReportData.SweepRows);
        PrintAdjustmentCycleGuide(
            scenarioLabel: "n%スイープ",
            timedOut: tournamentQualitySweepReportData.StoppedByTimeout,
            zeroResults: tournamentQualitySweepReportData.SweepRows.Count == 0,
            evaluatedPointCount: tournamentQualitySweepReportData.SweepRows.Count);
        PrintNextCycleSuggestion(tournamentQualitySweepReportData.Suggestion);
    }

    internal static void PrintTournamentQualitySweepReportTable(IReadOnlyList<TournamentQualitySweepReportRow> sweepRows)
    {
        Console.WriteLine("n%スイープ結果:");
        if (sweepRows.Count == 0)
        {
            Console.WriteLine("結果行は 0 件です。条件を短くして再試行してください。\n");
            return;
        }

        Console.WriteLine("先手勝率    Spearman   平均順位ずれ   上位8残留   Elo1位総合1位");

        foreach (var row in sweepRows)
        {
            Console.WriteLine(
                row.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture).PadLeft(8)
                + "%"
                + row.SpearmanCorrelation.ToString("F4", CultureInfo.InvariantCulture).PadLeft(12)
                + row.MeanAbsoluteRankError.ToString("F3", CultureInfo.InvariantCulture).PadLeft(14)
                + row.AverageTop8Retention.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
                + ((row.EloTop1OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture) + "%").PadLeft(16));
        }

        Console.WriteLine();
    }

    static void PrintSimulationResultSummary(CalculationResult result, double firstPlayerWinRatePercent)
    {
        Console.WriteLine($"計算方法: {result.Mode}\n");
        Console.WriteLine($"同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%\n");
    }

    static int ResolveSimulationResultNameWidth(IReadOnlyList<GeneralSimulationResultRow> resultRows)
    {
        return Math.Max(6, resultRows.Max(x => x.CommonData.Name.Length) + 2);
    }

    static string BuildSimulationResultCommonHeader(int nameWidth)
    {
        return "対局者".PadRight(nameWidth)
            + "元Elo".PadLeft(10)
            + "実効Elo".PadLeft(10)
            + "差分".PadLeft(10)
            + "先手".PadLeft(8)
            + "後手".PadLeft(8);
    }

    static string BuildSimulationResultCommonColumns(GeneralSimulationResultRow row, int nameWidth)
    {
        var commonData = row.CommonData;
        return commonData.Name.PadRight(nameWidth)
            + SimulationRatingMath.FormatRating(commonData.OriginalRating).PadLeft(10)
            + SimulationRatingMath.FormatRating(commonData.EffectiveRating).PadLeft(10)
            + SimulationRatingMath.FormatSignedRating(commonData.RatingDelta).PadLeft(10)
            + commonData.FirstPlayerCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
            + commonData.SecondPlayerCount.ToString(CultureInfo.InvariantCulture).PadLeft(8);
    }

    static double GetMetric(GeneralSimulationResultRow row, string key)
    {
        if (row.Metrics.TryGetValue(key, out var metric))
        {
            return metric.Value;
        }

        throw new InvalidOperationException($"シミュレーション結果行に必要な metric がありません: {key}");
    }

    static string GetFreeColumn(GeneralSimulationResultRow row, string key)
    {
        var column = row.FreeColumns.FirstOrDefault(column => string.Equals(column.Key, key, StringComparison.Ordinal));
        if (!string.IsNullOrEmpty(column.Key))
        {
            return column.DisplayValue;
        }

        throw new InvalidOperationException($"シミュレーション結果行に必要な自由形式列がありません: {key}");
    }

    internal static void PrintResult(int playerCount, CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<StandardResultRow> resultRows)
    {
        PrintChampionshipResult(result, firstPlayerWinRatePercent, resultRows.Select(row => row.ToGeneralResultRow()).ToArray());
    }

    internal static void PrintResult(int playerCount, CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<GeneralSimulationResultRow> resultRows)
    {
        PrintChampionshipResult(result, firstPlayerWinRatePercent, resultRows);
    }

    static void PrintChampionshipResult(CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<GeneralSimulationResultRow> resultRows)
    {
        PrintSimulationResultSummary(result, firstPlayerWinRatePercent);

        if (PrintGeneralSimulationZeroResultIfNeeded(result, resultRows, "シミュレーション")) return;

        var nameWidth = ResolveSimulationResultNameWidth(resultRows);
        var header = BuildSimulationResultCommonHeader(nameWidth)
            + "先手勝率".PadLeft(12)
            + "後手勝率".PadLeft(12)
            + "優勝確率".PadLeft(12)
            + "平均順位".PadLeft(12);

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var row in resultRows)
        {
            var commonData = row.CommonData;
            var line = BuildSimulationResultCommonColumns(row, nameWidth)
                + SimulationRatingMath.FormatOptionalPercent(commonData.FirstPlayerWinRate).PadLeft(12)
                + SimulationRatingMath.FormatOptionalPercent(commonData.SecondPlayerWinRate).PadLeft(12)
                + SimulationRatingMath.FormatPercent(GetMetric(row, "championshipProbability")).PadLeft(12)
                + GetMetric(row, "averagePlace").ToString("F3", CultureInfo.InvariantCulture).PadLeft(12);

            Console.WriteLine(line);
        }

        Console.WriteLine();
        PrintAdjustmentCycleGuide("シミュレーション", timedOut: result.Mode.Contains("時間切れ", StringComparison.Ordinal), zeroResults: false);
    }

    static bool PrintGeneralSimulationZeroResultIfNeeded(CalculationResult result, IReadOnlyCollection<GeneralSimulationResultRow> resultRows, string scenarioLabel)
    {
        if (resultRows.Count > 0) return false;

        Console.WriteLine("結果行は 0 件です。条件を短くして再試行してください。\n");
        PrintAdjustmentCycleGuide(scenarioLabel, timedOut: result.Mode.Contains("時間切れ", StringComparison.Ordinal), zeroResults: true);
        return true;
    }

    internal static void PrintRepresentativeExecutionRanking(IReadOnlyList<RepresentativeExecutionRankRow> rows, TournamentRuleSetMode tournamentRuleSetMode)
    {
        Console.WriteLine($"代表実行順位（{TournamentRuleSetRule.GetLabel(tournamentRuleSetMode)}）:");
        var nameWidth = Math.Max(6, rows.Max(x => x.Name.Length) + 2);
        var header = "対局者".PadRight(nameWidth)
            + "勝点".PadLeft(8)
            + "順位帯".PadLeft(10)
            + "平均順位".PadLeft(12)
            + "1位確率".PadLeft(12);

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));
        foreach (var row in rows)
        {
            var line = row.Name.PadRight(nameWidth)
                + row.Points.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                + row.RankLabel.PadLeft(10)
                + row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
                + SimulationRatingMath.FormatPercent(row.FirstPlaceProbability).PadLeft(12);
            Console.WriteLine(line);
        }

        Console.WriteLine();
    }

    internal static void PrintFinalStageResult(CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<FinalStageResultRow> resultRows)
    {
        PrintGroupedOverallResult(result, firstPlayerWinRatePercent, resultRows.Select(row => row.ToGeneralResultRow()).ToArray());
    }

    internal static void PrintFinalStageResult(CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<GeneralSimulationResultRow> resultRows)
    {
        PrintGroupedOverallResult(result, firstPlayerWinRatePercent, resultRows);
    }

    static void PrintGroupedOverallResult(CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<GeneralSimulationResultRow> resultRows)
    {
        PrintSimulationResultSummary(result, firstPlayerWinRatePercent);

        if (PrintGeneralSimulationZeroResultIfNeeded(result, resultRows, "本戦シミュレーション")) return;

        var nameWidth = ResolveSimulationResultNameWidth(resultRows);
        var header = BuildSimulationResultCommonHeader(nameWidth)
            + "群".PadLeft(8)
            + "群1位".PadLeft(10)
            + "群平均".PadLeft(10)
            + "総合1位".PadLeft(10)
            + "総合平均".PadLeft(10);

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var row in resultRows)
        {
            var line = BuildSimulationResultCommonColumns(row, nameWidth)
                + GetFreeColumn(row, "group").PadLeft(8)
                + SimulationRatingMath.FormatPercent(GetMetric(row, "groupPlace1Probability")).PadLeft(10)
                + GetMetric(row, "groupPlaceAverage").ToString("F3", CultureInfo.InvariantCulture).PadLeft(10)
                + SimulationRatingMath.FormatPercent(GetMetric(row, "overallPlace1Probability")).PadLeft(10)
                + GetMetric(row, "overallPlaceAverage").ToString("F3", CultureInfo.InvariantCulture).PadLeft(10);

            Console.WriteLine(line);
        }

        Console.WriteLine();
        PrintAdjustmentCycleGuide("本戦シミュレーション", timedOut: result.Mode.Contains("時間切れ", StringComparison.Ordinal), zeroResults: false);
    }

    internal static void PrintMatchesCsv(IReadOnlyList<Player> players, IReadOnlyList<Match> matches)
    {
        PrintMatchesCsv(players, matches, "生成された対局CSV:");
    }

    internal static void PrintMatchesCsv(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, string title)
    {
        Console.WriteLine(title);
        Console.WriteLine("first,second");

        foreach (var match in matches)
        {
            Console.WriteLine($"{CsvOutputHelpers.EscapeCsv(players[match.FirstPlayer].Name)},{CsvOutputHelpers.EscapeCsv(players[match.SecondPlayer].Name)}");
        }

        Console.WriteLine();
    }
}

