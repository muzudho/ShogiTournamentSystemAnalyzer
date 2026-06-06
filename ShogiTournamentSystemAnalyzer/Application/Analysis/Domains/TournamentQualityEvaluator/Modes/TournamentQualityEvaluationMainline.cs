/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［大会品質評価］モードのメインライン。スイープ実行と結果出力を行う。
/// </summary>
internal static class TournamentQualityEvaluationMainline
{
    internal static void Run(RuleProfileAttributes ruleProfileAttributes)
    {
        // ［選手一覧］を読み取る
        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        // ［大会品質評価ルール定義］を読み取る
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationRuleDefinition(players, ruleProfileAttributes, out var ruleDefinition)) return;

        // ［大会品質評価入力］を読み取る
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationInput(players, ruleDefinition, out var input)) return;

        // ［大会品質評価実行オプション］を読み取る
        var executionOptions = TournamentQualityEvaluationExecutor.ReadTournamentQualityEvaluationExecutionOptions(input, ruleDefinition);

        RunAfterInput(input, ruleDefinition, executionOptions, outputOptions: null);
    }

    internal static void Run(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        RunAfterInput(input, ruleDefinition, executionOptions, outputOptions);
    }

    static void RunAfterInput(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions? outputOptions)
    {
        // ［大会品質評価実行コンテキスト］を出力
        TournamentQualityEvaluationOutputCoordinator.PrintTournamentQualityEvaluationContext(input, ruleDefinition);

        // スイープ実行
        if (executionOptions.IsSweep)
        {
            var tournamentQualitySweepReportData = TournamentQualityEvaluationSweepExecutor.ExecuteSweepReport(
                input,
                ruleDefinition,
                executionOptions);

            ConsoleResultPrinter.PrintTournamentQualitySweepReportRows(tournamentQualitySweepReportData);
            if (tournamentQualitySweepReportData.StoppedByTimeout)
            {
                Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切ったため、n% スイープは途中で終了しました。\n");
            }

            var resolvedOutputOptions = outputOptions ?? TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualitySweepReportOutputOptions(ruleDefinition);
            TournamentQualityEvaluationOutputCoordinator.WriteTournamentQualitySweepReportOutputs(tournamentQualitySweepReportData, resolvedOutputOptions);
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
        var singleRunOutputOptions = outputOptions ?? TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualityReportOutputOptions(ruleDefinition);

        // 出力オプションの指定があればCSV出力する
        TournamentQualityEvaluationOutputCoordinator.WriteTournamentQualityReportOutputs(tournamentQualityReportData, singleRunOutputOptions);
    }
}
