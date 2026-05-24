/*
 * ［プログラム　＞　オーケストレーション］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
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
            ProgramConsoleGuide.PrintProgramIntroduction();

            // 入力方法を選ばせる（＾▽＾）！
            InputSourceConfiguration.ConfigureInputSource(args);

            // 前提入力は TournamentRule / PlayerList / RankingSettings の３境界だぜ（＾▽＾）！
            // 主線は TournamentFinalState → FinalRanking → TournamentQualityReport に寄せていくぜ（＾▽＾）！
            var flowMode = ConsolePromptReaders.ReadAnalysisFlowMode();
            var ruleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(flowMode);
            ProgramConsoleGuide.PrintSelectedMainline(flowMode, ruleProfileMode);

            // 選択フロー
            AnalysisFlowDispatcher.Execute(flowMode, ruleProfileMode);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }

}

