/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class TournamentQualityEvaluationMainline
{
    internal static void Run(RuleProfileMode ruleProfileMode)
    {
        // ［選手一覧］を読み取る
        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        // 大会品質評価ルール定義を読み取る
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationRuleDefinition(players, ruleProfileMode, out var ruleDefinition)) return;

        // 大会品質評価の入力を読み取る
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationInput(players, ruleDefinition, out var input)) return;

        // 実行オプションを読み取る
        var executionOptions = TournamentQualityEvaluationMode.ReadTournamentQualityEvaluationExecutionOptions(input, ruleDefinition);

        // 大会品質評価の実行コンテキストを出力
        TournamentQualityEvaluationOutputCoordinator.PrintTournamentQualityEvaluationContext(input, ruleDefinition);

        if (executionOptions.IsSweep)
        {
            TournamentQualityEvaluationMode.RunTournamentQualitySweepExperiment(
                input,
                ruleDefinition,
                executionOptions);
            return;
        }

        // 大会品質評価レポートを実行
        var tournamentQualityReportData = TournamentQualityEvaluationMode.ExecuteTournamentQualityReport(
            input,
            ruleDefinition,
            executionOptions);

        // 品質評価レポートの要約を出力
        ConsoleResultPrinter.PrintTournamentQualityReportSummary(tournamentQualityReportData);

        // プレイヤーハイライトを出力
        ConsoleResultPrinter.PrintTournamentQualityReportPlayerHighlights(tournamentQualityReportData);

        if (tournamentQualityReportData.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        // 出力オプションを読み取る
        var outputOptions = TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualityReportOutputOptions(ruleDefinition);

        // 出力オプションの指定があればCSV出力する
        TournamentQualityEvaluationOutputCoordinator.WriteTournamentQualityReportOutputs(tournamentQualityReportData, outputOptions);
    }
}
