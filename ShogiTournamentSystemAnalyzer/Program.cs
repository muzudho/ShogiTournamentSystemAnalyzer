/*
 * ［プログラム　＞　エントリーポイント］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application;

/// <summary>
/// ここがプログラムの入り口（エントリーポイント）だぜ（＾▽＾）！
/// </summary>
internal static partial class Program
{


    // ========================================
    // 概要
    // ========================================


    /// <summary>
    /// ここからプログラムが始まるぜ（＾▽＾）！
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    private static void Main(string[] args)
    {
        try
        {
            ApplicationWorkflow.Run(args);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }
}
