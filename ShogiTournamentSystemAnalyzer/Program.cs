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
            // このプログラムの説明を最初にするぜ（＾▽＾）！
            Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！\n");

            // 入力方法を選ばせる（＾▽＾）！
            ConfigureInputSource(args);

            // 大きくモードが分かれるぜ（＾▽＾）！
            //
            // 📍 TODO: ［ルール選択］→［パラメーター設定］→［試行］→［品質評価・レポート作成］の４ステップのシーケンスにした方がいいのでは（＾～＾）？
            //
            switch (ReadMode())
            {
                // ［通常ルール］モード
                case 1:
                    RunStandardMode();
                    break;

                // ［本戦ルール］モード
                case 2:
                    RunFinalStageMode();
                    break;

                // ［品質評価］モード
                case 3:
                    RunQualityEvaluationMode();
                    break;

                default:
                    throw new InvalidOperationException("未対応のモードです。");
            }
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }


    // ========================================
    // 詳細
    // ========================================


    /// <summary>
    /// シミュレーションは最大ｎ分までにするぜ（＾▽＾）！　あまり長くなりすぎると、結果が出る前に心が折れちゃうからな（＾～＾）！
    /// </summary>
    private static readonly TimeSpan SimulationTimeLimit = TimeSpan.FromMinutes(3);
    private static DateTime? _simulationDeadlineUtc;

}

