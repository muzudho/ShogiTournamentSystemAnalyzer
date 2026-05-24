/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using System.Globalization;
using System.Text;

/// <summary>
/// ここがプログラムだぜ（＾▽＾）！
/// </summary>
internal static partial class Program
{


    // ========================================
    // 概要
    // ========================================


    /// <summary>
    /// ここからプログラムが始まるぜ（＾▽＾）！
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            SimulationTimeBudget.BeginApplicationBudget();

            // このプログラムの説明を最初にするぜ（＾▽＾）！
            PrintProgramIntroduction();

            // 入力方法を選ばせる（＾▽＾）！
            InputSourceConfiguration.ConfigureInputSource(args);

            // 前提入力は TournamentRule / PlayerList / RankingSettings の３境界だぜ（＾▽＾）！
            // 主線は TournamentFinalState → FinalRanking → TournamentQualityReport に寄せていくぜ（＾▽＾）！
            var flowMode = ConsolePromptReaders.ReadAnalysisFlowMode();
            var ruleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(flowMode);
            PrintSelectedMainline(flowMode, ruleProfileMode);

            // 選択フロー
            ExecuteSelectedFlow(flowMode, ruleProfileMode);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }

    static void PrintProgramIntroduction()
    {
        Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！");
        Console.WriteLine();
        Console.WriteLine("前提入力: TournamentRule / PlayerList / RankingSettings");
        Console.WriteLine("主線: TournamentFinalState -> FinalRanking -> TournamentQualityReport\n");
    }

    static void PrintSelectedMainline(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        var profileLabel = ruleProfileMode switch
        {
            RuleProfileMode.Standard => "Standard",
            RuleProfileMode.FinalStage => "FinalStage",
            RuleProfileMode.TournamentFramework => "TournamentFramework",
            RuleProfileMode.Empty => "Empty",
            _ => ruleProfileMode.ToString(),
        };

        var mainlineLabel = flowMode switch
        {
            AnalysisFlowMode.Simulation => "TournamentFinalState -> FinalRanking",
            AnalysisFlowMode.QualityEvaluation => "TournamentFinalState -> FinalRanking -> TournamentQualityReport",
            _ => "TournamentFinalState -> FinalRanking"
        };

        Console.WriteLine($"選択された主線: {profileLabel} / {mainlineLabel}\n");
    }

    /// <summary>
    /// 選択フロー
    /// </summary>
    /// <param name="flowMode"></param>
    /// <param name="ruleProfileMode"></param>
    /// <exception cref="InvalidOperationException"></exception>
    static void ExecuteSelectedFlow(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        switch ((flowMode, ruleProfileMode))
        {
            case (AnalysisFlowMode.Simulation, RuleProfileMode.Standard):
                SimulationScenarioRunner.Run(SimulationScenarioFactory.Create(ruleProfileMode));
                break;

            case (AnalysisFlowMode.Simulation, RuleProfileMode.FinalStage):
                SimulationScenarioRunner.Run(SimulationScenarioFactory.Create(ruleProfileMode));
                break;

            case (AnalysisFlowMode.Simulation, RuleProfileMode.TournamentFramework):
                RunTournamentFrameworkMode();
                break;

            case (AnalysisFlowMode.Simulation, RuleProfileMode.Empty):
                RunEmptyMode();
                break;

            // ［大会品質評価フロー］
            case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.Standard):
                RunMainlineToTournamentQualityReport(RuleProfileMode.Standard);
                break;

            case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.FinalStage):
                RunMainlineToTournamentQualityReport(RuleProfileMode.FinalStage);
                break;

            default:
                throw new InvalidOperationException("未対応のモードです。");
        }
    }
}

