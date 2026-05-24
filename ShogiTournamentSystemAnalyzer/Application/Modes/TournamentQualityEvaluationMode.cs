/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class TournamentQualityEvaluationMode
{
    internal static void Run(RuleProfileMode ruleProfileMode)
    {
        if (ruleProfileMode == RuleProfileMode.Standard)
        {
            Console.WriteLine("品質評価 / 通常ルール: 総当たり戦向けルールの実力反映性を評価します。\n");
            ConsoleSamplePrinter.PrintQualityEvaluationStandardOverview();
        }
        else
        {
            Console.WriteLine("品質評価 / 本戦ルール: 本戦ルールの実力反映性を評価します。\n");
            ConsoleSamplePrinter.PrintQualityEvaluationFinalStageOverview();
        }

        RunMainlineToTournamentQualityReport(ruleProfileMode);
    }

    static void RunMainlineToTournamentQualityReport(RuleProfileMode ruleProfileMode)
    {
        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationRuleDefinition(players, ruleProfileMode, out var ruleDefinition)) return;
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationInput(players, ruleDefinition, out var input)) return;

        var executionOptions = ReadTournamentQualityEvaluationExecutionOptions(input, ruleDefinition);
        TournamentQualityEvaluationOutputCoordinator.PrintTournamentQualityEvaluationContext(input, ruleDefinition);

        if (executionOptions.IsSweep)
        {
            RunTournamentQualitySweepExperiment(
                input,
                ruleDefinition,
                executionOptions);
            return;
        }

        var tournamentQualityReportData = ExecuteTournamentQualityReport(
            input,
            ruleDefinition,
            executionOptions);

        ConsoleResultPrinter.PrintTournamentQualityReportSummary(tournamentQualityReportData);
        ConsoleResultPrinter.PrintTournamentQualityReportPlayerHighlights(tournamentQualityReportData);
        if (tournamentQualityReportData.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var outputOptions = TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualityReportOutputOptions(ruleDefinition);
        TournamentQualityEvaluationOutputCoordinator.WriteTournamentQualityReportOutputs(tournamentQualityReportData, outputOptions);
    }

}

