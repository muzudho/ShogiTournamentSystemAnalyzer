/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;

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

        PrintTournamentQualitySweepReportRows(tournamentQualitySweepReportData);
        if (tournamentQualitySweepReportData.StoppedByTimeout)
        {
            Console.WriteLine($"シミュレーションは時間上限 {Program.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切ったため、n% スイープは途中で終了しました。\n");
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
        using var simulationBudget = executionOptions.SimulationCount.HasValue ? BeginSimulationBudget() : default;
        var stoppedByTimeout = false;
        for (var firstPlayerWinRatePercent = executionOptions.SweepOptions.StartPercent; firstPlayerWinRatePercent <= executionOptions.SweepOptions.EndPercent + 1e-9; firstPlayerWinRatePercent += executionOptions.SweepOptions.StepPercent)
        {
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

        return BuildTournamentQualitySweepReportBoundaryData(sweepRows, stoppedByTimeout);
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
        var firstPlayerWinRateRating = ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        using var simulationBudget = executionOptions.SimulationCount.HasValue ? BeginSimulationBudget() : default;
        var result = ruleDefinition.GroupingMode == FinalStageGroupingMode.On
            ? executionOptions.SimulationCount.HasValue
                ? CalculateFinalStageBySimulation(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.PromotedInnovCount)
                : CalculateFinalStageExactly(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, ruleDefinition.PromotedInnovCount)
            : executionOptions.SimulationCount.HasValue
                ? CalculateBySimulation(input.Participants, input.Matches, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.TournamentRuleSetMode)
                : CalculateExactly(input.Participants, input.Matches, firstPlayerWinRateRating, ruleDefinition.TournamentRuleSetMode);

        var resultRows = BuildResultRows(input.Participants, input.Matches, result, firstPlayerWinRatePercent);
        var qualityPlayerRows = BuildTournamentQualityReportPlayerRows(
            resultRows,
            ruleDefinition.GroupMap,
            ruleDefinition.AdditionalApexParticipants,
            ruleDefinition.AdditionalApexPlacementMode,
            input.InnovExpectedRankOffsetMode,
            input.InnovExpectedRankOffsetCount);
        var qualitySummary = BuildTournamentQualityReportSummary(qualityPlayerRows);
        return new TournamentQualityReportRun(qualityPlayerRows, qualitySummary, result.Mode);
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
        return BuildTournamentQualityReportBoundaryData(qualityEvaluationRun);
    }

    static TournamentQualityEvaluationExecutionOptions ReadTournamentQualityEvaluationExecutionOptions(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var sweepOptions = ReadTournamentQualitySweepOptions();

        if (!sweepOptions.IsEnabled)
        {
            var firstPlayerWinRatePercent = ReadDoubleWithDefaultInRange(
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
                    simulationCount = ReadIntWithDefault(
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
                simulationCount = ReadIntWithDefault(
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
                    var startPercent = ReadDoubleWithDefaultInRange("開始する先手勝率(%)を入力してください [50]: ", 50.0, 0.0, 100.0);
                    var endPercent = ReadDoubleWithDefaultInRange("終了する先手勝率(%)を入力してください [55]: ", 55.0, 0.0, 100.0);
                    var stepPercent = ReadDoubleWithDefaultInRange("刻み幅(%)を入力してください [1]: ", 1.0, 0.000001, 100.0);
                    Console.WriteLine();

                    if (endPercent < startPercent)
                    {
                        if (sweepAttempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("n% スイープ範囲", "終了する先手勝率が開始する先手勝率未満です");

                        Console.WriteLine("終了する先手勝率は開始する先手勝率以上で入力してください。\n");
                        continue;
                    }

                    return new TournamentQualitySweepOptions(true, startPercent, endPercent, stepPercent);
                }
            }

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("品質評価の実行方法", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }
}

