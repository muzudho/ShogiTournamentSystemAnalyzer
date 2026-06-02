/*
 * ［プログラム　＞　エントリーポイント］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.ManualInput;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;
using ShogiTournamentSystemAnalyzer.Domain.Request;
using System.Text;

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
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            bool isFutureImplementation = false;

            // 現在の実装
            if (isFutureImplementation)
            {
                ApplicationStartup.Start();
                RequestWorkflow.Run(args);
                return;
            }

            // TODO: ここから下は、将来的な実装
            // ［依頼という境界］
            RequestBoundary requestBoundary = new();

            //          開始
            //          │
            //          ↓
            //          ◆"コマンドライン引数で入力ファイルを指定したか？"
            //          │
            //          ├─────────────────────────┐
            //          │                                                  ・
            //          │ "はい"                                           ・
            if (args.Length > 0)
            {
                //      │                                                  ・
                //      │                                                  ・
                //      ■［要求ファイルチェック］(`RequestFileCheck`)      ・
                var requestModelProducer = RequestFileCheckWorkflow.Run(args);
                //      │                                                  ・
                //      │                                                  ・
                //      ◆"エラーが有ったか？"                              ・
                //      │                                                  ・
                //      │                                                  ・
                //      ├──────────┐                            ・
                //      ・                    │                            ・
                //      ・                    │ "エラー有り"               ・
                if (requestModelProducer.HasError)
                {
                    //  ・                    │                            ・
                    //  ・                    ↓                            ・
                    //  ・                    ●終了                        ・
                    return;
                }
                //      │                                                  ・
                //      │                                                  ・
                //      │  "エラー無し"                                    ・
                requestModelProducer.Produce(requestBoundary);
            }
            //          ・                                                  │
            //          ・                                                  │
            //          ・                                                  │ "いいえ"
            else
            {
                //      ・                                                  │
                //      ・                                                  │
                //      ・                                                  ■［手動入力］（`ManualInput`）
                var requestModelProducer = ManualInputWorkflow.Run();
                //      ・                                                  │
                //      ・                                                  │
                //      ・                                                  ◆"エラーが有ったか？"
                //      ・                                                  │
                //      ・                                                  ├──────────┐
                //      ・                                                  ・                    │
                //      ・                                                  ・                    │ "エラー有り"
                if (requestModelProducer.HasError)
                {
                    //  ・                                                  ・                    │
                    //  ・                                                  ・                    ↓
                    //  ・                                                  ・                    ●終了
                    return;
                }
                //      ・                                                  │
                //      ・                                                  │
                //      ・  "エラー無し"                                    │
                requestModelProducer.Produce(requestBoundary);
                //      ・                                                  │
                //      ・                                                  ↓
                //      ・                                                  ◆"今回の入力を保存しておきますか？"
                //      ・                                                  │
                //      ・                                                  ├───────────────────────┐
                //      ・                                                  │                                              ・
                //      ・                                                  │ "はい"                                       ・
                if (requestModelProducer.ShallSave)
                {
                    //  ・                                                  │                                              ・
                    //  ・                                                  │                                              ・
                    //  ・                                                  ■［要求ファイル作成］(`RequestFileCreate`)     ・
                    RequestFileCreateWorkflow.Run(requestBoundary);
                }
                //      ・                                                  ・                                              │ "いいえ"
                //      ・                                                  ・                                              │
                //      ・                                                  │←──────────────────────┘
                //      ・                                                  │
                //      │←────────────────────────┘
                //      │
            }
            //      │
            //      ↓
            //      ■［分析］(`Analysis`)
            AnalysisWorkflowNewVersion.Run(requestBoundary);
            //      │
            //      ↓
            //      終了
            return;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }

}
