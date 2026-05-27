/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Helpers;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Presentation.Console;
using System.Globalization;

internal static class TournamentQualityEvaluationExecutor
{
    /// <summary>
    /// ［大会品質評価フロー］の実行。単発評価か n% スイープ実験のどちらかを選択して実行します。
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ruleDefinition"></param>
    /// <param name="executionOptions"></param>
    internal static void RunTournamentQualitySweepExperiment(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions)
    {
        var tournamentQualitySweepReportData = ExecuteTournamentQualitySweepReport(input, ruleDefinition, executionOptions);

        ConsoleResultPrinter.PrintTournamentQualitySweepReportRows(tournamentQualitySweepReportData);
        if (tournamentQualitySweepReportData.StoppedByTimeout)
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切ったため、n% スイープは途中で終了しました。\n");
        }

        var outputOptions = TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualitySweepReportOutputOptions(ruleDefinition);
        TournamentQualityEvaluationOutputCoordinator.WriteTournamentQualitySweepReportOutputs(tournamentQualitySweepReportData, outputOptions);
    }

    /// <summary>
    /// ［大会品質評価フロー］を実行して［大会品質レポート（スイープ）］を返す。
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ruleDefinition"></param>
    /// <param name="executionOptions"></param>
    /// <returns></returns>
    static TournamentQualitySweepReportData ExecuteTournamentQualitySweepReport(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions)
    {
        var sweepRows = new List<TournamentQualitySweepReportRow>();
        using var simulationBudget = executionOptions.SimulationCount.HasValue ? SimulationTimeBudget.BeginSimulationBudget() : default;
        var stoppedByTimeout = false;
        for (var firstPlayerWinRatePercent = executionOptions.SweepOptions.StartPercent; firstPlayerWinRatePercent <= executionOptions.SweepOptions.EndPercent + 1e-9; firstPlayerWinRatePercent += executionOptions.SweepOptions.StepPercent)
        {
            if (!SimulationTimeBudget.HasApplicationTimeRemaining())
            {
                stoppedByTimeout = true;
                break;
            }

            var qualityEvaluationRun = ExecuteTournamentQualityEvaluationRun(
                input,
                ruleDefinition,
                executionOptions with { FirstPlayerWinRatePercent = firstPlayerWinRatePercent });

            sweepRows.Add(new TournamentQualitySweepReportRow(
                firstPlayerWinRatePercent,
                qualityEvaluationRun.Summary.SpearmanCorrelation,
                qualityEvaluationRun.Summary.MeanAbsoluteRankError,
                qualityEvaluationRun.Summary.AverageTop8Retention,
                qualityEvaluationRun.Summary.EloTop1OverallTop1Probability,
                qualityEvaluationRun.Summary.MostPenalizedPlayerName,
                qualityEvaluationRun.Summary.MostPenalizedDelta,
                qualityEvaluationRun.Summary.MostAdvantagedPlayerName,
                qualityEvaluationRun.Summary.MostAdvantagedDelta));

            if (qualityEvaluationRun.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
            {
                stoppedByTimeout = true;
                break;
            }
        }

        var suggestion = TournamentQualityNextCycleSuggestionBuilder.BuildForSweep(input, executionOptions, sweepRows, stoppedByTimeout);
        return BoundaryDataBuilders.BuildTournamentQualitySweepReportBoundaryData(sweepRows, stoppedByTimeout, suggestion);
    }

    /// <summary>
    /// ［大会品質評価フロー］を実行して［大会品質レポート］を返す。
    /// </summary>
    /// <param name="input">The quality evaluation input containing players and matches to evaluate.</param>
    /// <param name="ruleDefinition">The tournament rule definition specifying grouping mode, boundary rescue settings, and other tournament
    /// parameters.</param>
    /// <param name="executionOptions">The execution options controlling simulation count and first player win rate percentage.</param>
    /// <returns>A completed quality evaluation run containing player rows, quality summary, and the calculation mode used.</returns>
    static TournamentQualityReportRun ExecuteTournamentQualityEvaluationRun(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions)
    {
        var firstPlayerWinRatePercent = executionOptions.FirstPlayerWinRatePercent!.Value;
        var tournamentFinalState = ExecuteTournamentFinalStateForQualityEvaluation(input, ruleDefinition, executionOptions, firstPlayerWinRatePercent);
        var finalRankingRows = BuildFinalRankingRowsForQualityEvaluation(input, tournamentFinalState, firstPlayerWinRatePercent);
        var qualityPlayerRows = TournamentQualityEvaluationReportBuilder.BuildTournamentQualityReportPlayerRows(
            finalRankingRows,
            ruleDefinition.GroupMap,
            ruleDefinition.AdditionalApexPlayers,
            ruleDefinition.AdditionalApexPlacementMode,
            input.InnovExpectedRankOffsetMode,
            input.InnovExpectedRankOffsetCount);
        var qualitySummary = TournamentQualityEvaluationReportBuilder.BuildTournamentQualityReportSummary(qualityPlayerRows);
        var calculationMode = qualityPlayerRows.Count == 0 && !tournamentFinalState.Mode.Contains("時間切れ", StringComparison.Ordinal)
            ? tournamentFinalState.Mode + " (0回)"
            : tournamentFinalState.Mode;
        var suggestion = TournamentQualityNextCycleSuggestionBuilder.BuildForSingleRun(input, executionOptions, tournamentFinalState.Mode.Contains("時間切れ", StringComparison.Ordinal), qualityPlayerRows.Count);
        return new TournamentQualityReportRun(qualityPlayerRows, qualitySummary, calculationMode, suggestion);
    }

    static CalculationResult ExecuteTournamentFinalStateForQualityEvaluation(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        double firstPlayerWinRatePercent)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        using var simulationBudget = executionOptions.SimulationCount.HasValue ? SimulationTimeBudget.BeginSimulationBudget() : default;
        return ruleDefinition.GroupingMode == FinalStageGroupingMode.On
            ? executionOptions.SimulationCount.HasValue
                ? FinalStageCalculationEngine.CalculateFinalStageBySimulation(input.Players, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.PromotedInnovCount)
                : FinalStageCalculationEngine.CalculateFinalStageExactly(input.Players, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, ruleDefinition.PromotedInnovCount)
            : executionOptions.SimulationCount.HasValue
                ? StandardCalculationEngine.CalculateBySimulation(input.Players, input.Matches, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.TournamentRuleSetMode)
                : StandardCalculationEngine.CalculateExactly(input.Players, input.Matches, firstPlayerWinRateRating, ruleDefinition.TournamentRuleSetMode);
    }

    static IReadOnlyList<ResultRow> BuildFinalRankingRowsForQualityEvaluation(
        TournamentQualityEvaluationInput input,
        CalculationResult tournamentFinalState,
        double firstPlayerWinRatePercent)
    {
        return RankingResultRowBuilder.BuildResultRows(input.Players, input.Matches, tournamentFinalState, firstPlayerWinRatePercent);
    }

    /// <summary>
    /// ［大会品質評価フロー］を実行して［大会品質レポート］を返す。
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ruleDefinition"></param>
    /// <param name="executionOptions"></param>
    /// <returns></returns>
    internal static TournamentQualityReportData ExecuteTournamentQualityReport(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions)
    {
        var qualityEvaluationRun = ExecuteTournamentQualityEvaluationRun(input, ruleDefinition, executionOptions);
        return BoundaryDataBuilders.BuildTournamentQualityReportBoundaryData(qualityEvaluationRun);
    }

    internal static TournamentQualityEvaluationExecutionOptions ReadTournamentQualityEvaluationExecutionOptions(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var sweepOptions = ReadTournamentQualitySweepOptions();

        if (!sweepOptions.IsEnabled)
        {
            var firstPlayerWinRatePercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange(
                "同Elo対局時の先手勝率(%)を入力してください [51]: ",
                51.0,
                0.0,
                100.0);
            Console.WriteLine();

            int? simulationCount = null;
            if (!ruleDefinition.UsesFinalStageGrouping)
            {
                if (input.Matches.Count <= 20)
                {
                    Console.WriteLine($"{TournamentRuleSetRule.GetLabel(ruleDefinition.TournamentRuleSetMode)} の品質評価用厳密計算を行います。\n");
                }
                else
                {
                    const int defaultSimulationCount = 200_000;
                    simulationCount = ConsolePromptReaders.ReadIntWithDefault(
                        $"局数が多いため {TournamentRuleSetRule.GetLabel(ruleDefinition.TournamentRuleSetMode)} の品質評価用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                        defaultSimulationCount,
                        min: 1);

                    Console.WriteLine();
                }
            }
            else if (input.Matches.Count <= 20)
            {
                Console.WriteLine("品質評価用の厳密計算を行います。\n");
            }
            else
            {
                const int defaultSimulationCount = 200_000;
                simulationCount = ConsolePromptReaders.ReadIntWithDefault(
                    $"局数が多いため品質評価用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                    defaultSimulationCount,
                    min: 1);

                Console.WriteLine();
            }

            return new TournamentQualityEvaluationExecutionOptions(simulationCount, sweepOptions, firstPlayerWinRatePercent);
        }

        return new TournamentQualityEvaluationExecutionOptions(null, sweepOptions, null);
    }

    static TournamentQualitySweepOptions ReadTournamentQualitySweepOptions()
    {
        Console.WriteLine("品質評価の実行方法を選んでください。");
        Console.WriteLine("単発評価は現在の条件だけを評価し、n% スイープ実験は先手勝率を範囲で振って比較します。");
        Console.WriteLine("1. 単発評価");
        Console.WriteLine("2. n% スイープ実験\n");

        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write("実行方法を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return new TournamentQualitySweepOptions(false, 0.0, 0.0, 0.0);
            }

            if (input == "2")
            {
                Console.WriteLine();
                var sweepAttempt = 0;
                while (true)
                {
                    sweepAttempt++;
                    Console.WriteLine("補足: 例として 50 → 55 を 1 刻みで指定すると、50, 51, 52, 53, 54, 55 を順に評価します。\n");
                    var startPercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("開始する先手勝率(%)を入力してください [50]: ", 50.0, 0.0, 100.0);
                    var endPercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("終了する先手勝率(%)を入力してください [55]: ", 55.0, 0.0, 100.0);
                    var stepPercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("刻み幅(%)を入力してください [1]: ", 1.0, 0.000001, 100.0);
                    Console.WriteLine();

                    if (endPercent < startPercent)
                    {
                        if (sweepAttempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("n% スイープ範囲", "終了する先手勝率が開始する先手勝率未満です");

                        Console.WriteLine("終了する先手勝率は開始する先手勝率以上で入力してください。\n");
                        continue;
                    }

                    return new TournamentQualitySweepOptions(true, startPercent, endPercent, stepPercent);
                }
            }

            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("品質評価の実行方法", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }
}

