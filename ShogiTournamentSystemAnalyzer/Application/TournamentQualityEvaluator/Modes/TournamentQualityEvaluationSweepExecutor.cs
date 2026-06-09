/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.TournamentQualityEvaluator.Modes;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class TournamentQualityEvaluationSweepExecutor
{
    internal static void Run(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityScoreRule scoreRule)
    {
        var tournamentQualitySweepReportData = ExecuteSweepReport(input, ruleDefinition, executionOptions, scoreRule);

        ConsoleResultPrinter.PrintTournamentQualitySweepReportRows(tournamentQualitySweepReportData);
        if (tournamentQualitySweepReportData.StoppedByTimeout)
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切ったため、n% スイープは途中で終了しました。\n");
        }

        var outputOptions = TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualitySweepReportOutputOptions(ruleDefinition);
        TournamentQualityEvaluationOutputCoordinator.WriteTournamentQualitySweepReportOutputs(tournamentQualitySweepReportData, outputOptions);
    }

    internal static TournamentQualitySweepReportData ExecuteSweepReport(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityScoreRule scoreRule)
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

            var qualityEvaluationRun = TournamentQualityEvaluationSingleRunExecutor.ExecuteRun(
                input,
                ruleDefinition,
                executionOptions with { FirstPlayerWinRatePercent = firstPlayerWinRatePercent },
                scoreRule);

            sweepRows.Add(new TournamentQualitySweepReportRow(
                firstPlayerWinRatePercent,
                qualityEvaluationRun.Summary.SpearmanCorrelation,
                qualityEvaluationRun.Summary.MeanAbsoluteRankError,
                qualityEvaluationRun.Summary.AverageTop8Retention,
                qualityEvaluationRun.Summary.EloTop1OverallTop1Probability,
                qualityEvaluationRun.Summary.MostPenalizedPlayerName,
                qualityEvaluationRun.Summary.MostPenalizedDelta,
                qualityEvaluationRun.Summary.MostAdvantagedPlayerName,
                qualityEvaluationRun.Summary.MostAdvantagedDelta,
                qualityEvaluationRun.Summary.ScoreBreakdown.TotalScore,
                qualityEvaluationRun.Summary.ScoreBreakdown.Reliability.SimulationCount,
                qualityEvaluationRun.Summary.ScoreBreakdown.Reliability.Label,
                qualityEvaluationRun.Summary.ScoreBreakdown.Reliability.IsReferenceRecord,
                qualityEvaluationRun.Summary.ScoreBreakdown.Reliability.IsOfficialEvaluation));

            if (qualityEvaluationRun.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
            {
                stoppedByTimeout = true;
                break;
            }
        }

        var suggestion = TournamentQualityNextCycleSuggestionBuilder.BuildForSweep(input, executionOptions, sweepRows, stoppedByTimeout);
        return BoundaryDataBuilders.BuildTournamentQualitySweepReportBoundaryData(sweepRows, stoppedByTimeout, suggestion);
    }
}
