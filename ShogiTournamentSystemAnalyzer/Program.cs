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

