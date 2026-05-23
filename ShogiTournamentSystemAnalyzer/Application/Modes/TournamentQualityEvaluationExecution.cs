/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Helpers;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;
using System.Globalization;

internal static partial class Program
{
    /// <summary>
    /// ［大会品質評価フロー］の実行。単発評価か n% スイープ実験のどちらかを選択して実行します。
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ruleDefinition"></param>
    /// <param name="executionOptions"></param>
    static void RunTournamentQualitySweepExperiment(
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

        var outputOptions = ReadTournamentQualitySweepReportOutputOptions(ruleDefinition);
        WriteTournamentQualitySweepReportOutputs(tournamentQualitySweepReportData, outputOptions);
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

        var suggestion = BuildNextCycleSuggestionForSweep(executionOptions, sweepRows.Count, stoppedByTimeout);
        return BoundaryDataBuilders.BuildTournamentQualitySweepReportBoundaryData(sweepRows, stoppedByTimeout, suggestion);
    }

    /// <summary>
    /// ［大会品質評価フロー］を実行して［大会品質レポート］を返す。
    /// </summary>
    /// <param name="input">The quality evaluation input containing participants and matches to evaluate.</param>
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
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        using var simulationBudget = executionOptions.SimulationCount.HasValue ? SimulationTimeBudget.BeginSimulationBudget() : default;
        var result = ruleDefinition.GroupingMode == FinalStageGroupingMode.On
            ? executionOptions.SimulationCount.HasValue
                ? CalculateFinalStageBySimulation(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.PromotedInnovCount)
                : CalculateFinalStageExactly(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, ruleDefinition.PromotedInnovCount)
            : executionOptions.SimulationCount.HasValue
                ? CalculateBySimulation(input.Participants, input.Matches, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.TournamentRuleSetMode)
                : CalculateExactly(input.Participants, input.Matches, firstPlayerWinRateRating, ruleDefinition.TournamentRuleSetMode);

        var resultRows = RankingResultRowBuilder.BuildResultRows(input.Participants, input.Matches, result, firstPlayerWinRatePercent);
        var qualityPlayerRows = TournamentQualityEvaluationReportBuilder.BuildTournamentQualityReportPlayerRows(
            resultRows,
            ruleDefinition.GroupMap,
            ruleDefinition.AdditionalApexParticipants,
            ruleDefinition.AdditionalApexPlacementMode,
            input.InnovExpectedRankOffsetMode,
            input.InnovExpectedRankOffsetCount);
        var qualitySummary = TournamentQualityEvaluationReportBuilder.BuildTournamentQualityReportSummary(qualityPlayerRows);
        var calculationMode = qualityPlayerRows.Count == 0 && !result.Mode.Contains("時間切れ", StringComparison.Ordinal)
            ? result.Mode + " (0回)"
            : result.Mode;
        var suggestion = BuildNextCycleSuggestionForSingleRun(executionOptions, result.Mode.Contains("時間切れ", StringComparison.Ordinal), qualityPlayerRows.Count);
        return new TournamentQualityReportRun(qualityPlayerRows, qualitySummary, calculationMode, suggestion);
    }

    static TournamentQualityNextCycleSuggestion BuildNextCycleSuggestionForSweep(
        TournamentQualityEvaluationExecutionOptions executionOptions,
        int completedPointCount,
        bool stoppedByTimeout)
    {
        var start = executionOptions.SweepOptions.StartPercent;
        var end = executionOptions.SweepOptions.EndPercent;
        var step = executionOptions.SweepOptions.StepPercent;
        var currentPointCount = Math.Max(1, (int)Math.Floor((end - start) / step) + 1);
        var completionRate = Math.Clamp((double)completedPointCount / currentPointCount, 0.0, 1.0);
        var width = Math.Max(step, end - start);
        var widthScale = stoppedByTimeout
            ? completionRate switch
            {
                <= 0.20 => 0.25,
                <= 0.40 => 0.40,
                <= 0.70 => 0.60,
                _ => 0.80,
            }
            : 1.10;
        var stepScale = stoppedByTimeout
            ? completionRate switch
            {
                <= 0.20 => 4.0,
                <= 0.40 => 3.0,
                <= 0.70 => 2.0,
                _ => 1.5,
            }
            : 1.0;
        var suggestedEnd = Math.Min(100.0, start + Math.Max(step, width * widthScale));
        var suggestedStep = Math.Max(step, Math.Ceiling(step * stepScale));
        var suggestedSettings = new List<string>
        {
            $"開始する先手勝率(%) = {start.ToString("F2", CultureInfo.InvariantCulture)}",
            $"終了する先手勝率(%) = {suggestedEnd.ToString("F2", CultureInfo.InvariantCulture)}",
            $"刻み幅(%) = {suggestedStep.ToString("F2", CultureInfo.InvariantCulture)}"
        };

        if (stoppedByTimeout)
        {
            var targetPointCount = Math.Max(1, (int)Math.Ceiling(currentPointCount * Math.Max(0.25, completionRate * 1.5)));
            suggestedSettings.Add($"目安の評価点数 = {targetPointCount}");
        }

        var reason = stoppedByTimeout
            ? $"今回の計算点数は {completedPointCount}/{currentPointCount} 件（完了率 {(completionRate * 100.0).ToString("F0", CultureInfo.InvariantCulture)}%）だったので、完了率に応じて範囲と刻み幅を自動調整した案です。"
            : "今回の範囲で最後まで回せたので、同条件を基準に少しずつ広げられます。";

        return new TournamentQualityNextCycleSuggestion("次回の n%スイープ提案", suggestedSettings, reason);
    }

    static TournamentQualityNextCycleSuggestion BuildNextCycleSuggestionForSingleRun(
        TournamentQualityEvaluationExecutionOptions executionOptions,
        bool stoppedByTimeout,
        int completedPlayerRowCount)
    {
        var suggestedSettings = new List<string>();
        if (executionOptions.FirstPlayerWinRatePercent.HasValue)
        {
            suggestedSettings.Add($"同Elo対局時の先手勝率(%) = {executionOptions.FirstPlayerWinRatePercent.Value.ToString("F2", CultureInfo.InvariantCulture)}");
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
            if (stoppedByTimeout)
            {
                var quickTrialSimulationCount = Math.Max(1_000, suggestedSimulationCount / 2);
                suggestedSettings.Add($"さらに様子見するなら試行回数 = {quickTrialSimulationCount:N0}");
            }
        }

        if (suggestedSettings.Count == 0)
        {
            suggestedSettings.Add("対局数または対象条件を少し絞って再試行する");
        }

        var reason = stoppedByTimeout
            ? $"今回の品質評価は時間切れになったので、現在の試行回数帯に合わせて一段階ずつ下げる案です。取得できた選手行数は {completedPlayerRowCount} 件です。"
            : "今回の条件で回せたので、同条件を基準に比較を続けられます。";

        return new TournamentQualityNextCycleSuggestion("次回の品質評価提案", suggestedSettings, reason);
    }

    /// <summary>
    /// ［大会品質評価フロー］を実行して［大会品質レポート］を返す。
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ruleDefinition"></param>
    /// <param name="executionOptions"></param>
    /// <returns></returns>
    static TournamentQualityReportData ExecuteTournamentQualityReport(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions)
    {
        var qualityEvaluationRun = ExecuteTournamentQualityEvaluationRun(input, ruleDefinition, executionOptions);
        return BoundaryDataBuilders.BuildTournamentQualityReportBoundaryData(qualityEvaluationRun);
    }

    static TournamentQualityEvaluationExecutionOptions ReadTournamentQualityEvaluationExecutionOptions(
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

