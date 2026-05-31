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
            // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
            SimulationTimeBudget.BeginApplicationBudget();

            // このプログラムの説明を最初に表示するぜ（＾▽＾）！
            ProgramConsoleGuide.PrintProgramIntroduction();

            // ［依頼］が［要求ファイル］からか、［対話か］に応じて、入力導線を準備するぜ（＾▽＾）！
            using var inputSource = InputSourceConfiguration.ConfigureInputSource(args);

            // ［依頼］を受け取って分析を始めるぜ（＾▽＾）！
            Analysis.Run();

            // 入力セッションの後片付けや完了処理を行うぜ（＾▽＾）！
            inputSource.Complete();
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }

}

