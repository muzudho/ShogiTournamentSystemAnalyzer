/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［大会品質評価］モードのメインライン。スイープ実行と結果出力を行う。
/// </summary>
internal static class TournamentQualityEvaluationMainline
{
    internal static void Run(RuleProfileMode ruleProfileMode)
    {
        // ［選手一覧］を読み取る
        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        // ［大会品質評価ルール定義］を読み取る
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationRuleDefinition(players, ruleProfileMode, out var ruleDefinition)) return;

        // ［大会品質評価入力］を読み取る
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationInput(players, ruleDefinition, out var input)) return;

        // ［大会品質評価実行オプション］を読み取る
        var executionOptions = TournamentQualityEvaluationExecutor.ReadTournamentQualityEvaluationExecutionOptions(input, ruleDefinition);

        // ［大会品質評価実行コンテキスト］を出力
        TournamentQualityEvaluationOutputCoordinator.PrintTournamentQualityEvaluationContext(input, ruleDefinition);

        // スイープ実行
        if (executionOptions.IsSweep)
        {
            TournamentQualityEvaluationSweepExecutor.Run(
                input,
                ruleDefinition,
                executionOptions);
            return;
        }

        // 大会品質評価レポートを実行
        var tournamentQualityReportData = TournamentQualityEvaluationSingleRunExecutor.ExecuteReport(
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
