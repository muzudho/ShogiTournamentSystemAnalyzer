/*
 * ［プログラム　＞　オーケストレーション］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Input;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;
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

            // 要求ファイル指定の有無に応じて、入力導線を準備するぜ（＾▽＾）！
            using var inputSource = InputSourceConfiguration.ConfigureInputSource(args);

            RunAnalysis();

            // 手動入力を記録していた場合は、分析後に要求ファイルを作成するぜ（＾▽＾）！
            inputSource.CompleteRequestFileCreate();
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }


    static void RunAnalysis()
    {
        Console.WriteLine("■［分析］");

        // 前提入力は TournamentRule / PlayerList / RankingSettings の３境界だぜ（＾▽＾）！
        // 主線は TournamentFinalState → FinalRanking → TournamentQualityReport に寄せていくぜ（＾▽＾）！
        var flowMode = ConsolePromptReaders.ReadAnalysisFlowMode();

        // 対象［大会ルール］を選ばせるぜ（＾▽＾）！
        var ruleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(flowMode);

        // メインライン選択のガイドを表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintSelectedMainline(flowMode, ruleProfileMode);

        // 選択フロー
        AnalysisFlowDispatcher.Execute(flowMode, ruleProfileMode);
    }

}

