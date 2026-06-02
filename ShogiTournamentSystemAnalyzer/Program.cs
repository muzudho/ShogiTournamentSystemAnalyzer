/*
 * ［プログラム　＞　エントリーポイント］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;
using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCreate;
using ShogiTournamentSystemAnalyzer.Application.ManualInput;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;
using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Domain.Request;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;
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
        try
        {
            //          ●開始
            //          │
            //          ↓

            // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
            Console.OutputEncoding = Encoding.UTF8;

            // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
            SimulationTimeBudget.BeginApplicationBudget();

            // このプログラムの説明を最初に表示するぜ（＾▽＾）！
            ProgramConsoleGuide.PrintProgramIntroduction();

            // ［依頼という境界］
            RequestBoundary requestBoundary = new();

            RequestInputSession? inputSession;

            //          ◆"コマンドライン引数で入力ファイルを指定したか？"
            var argumentResult = RequestFileArgumentReader.Read(args);
            //          │
            //          ↓

            // ［要求ファイル］確認中の異常の場合。
            if (argumentResult.HasError)
            {
                Console.WriteLine($"●異常終了：　［要求ファイル］確認中。 {argumentResult.ErrorMessage!}");
                return;
            }

            //          │
            //          ├─────────────────────────┐
            //          │                                                  ・
            //          │ "はい"                                           ・
            // ［要求ファイル］が指定されていた場合。
            if (argumentResult.HasInputFile)
            {
                //      │                                                  ・
                //      │                                                  ・
                //      ■［要求ファイルチェック］(`RequestFileCheck`)      ・
                var requestFileCheckResultVer2 = RequestFileCheckWorkflow.Run(argumentResult);
                //      │                                                  ・
                //      │                                                  ・
                //      ◆"エラーが有ったか？"                              ・
                //      │                                                  ・
                //      │                                                  ・
                //      ├──────────┐                            ・
                //      ・                    │                            ・
                //      ・                    │ "エラー有り"               ・
                if (requestFileCheckResultVer2.HasError)
                {
                    //  ・                    │                            ・
                    //  ・                    ↓                            ・
                    //  ・                    ●終了                        ・
                    return;
                }
                //      │                                                  ・
                //      │                                                  ・
                //      │  "エラー無し"                                    ・
                inputSession = requestFileCheckResultVer2.InputSession;
            }
            // ［要求ファイル］が指定されていない場合。
            else
            {
                Console.WriteLine("■［手動入力］");

                var requestFileCreatePath = RequestFileCreatePrompt.ReadOutputPath();
                if (requestFileCreatePath is null)
                {
                    Console.WriteLine();
                    inputSession = new RequestInputSession(null, null);
                }
                else
                {
                    inputSession = ManualInputRecordingSessionStarter.Start(requestFileCreatePath);
                }
            }

            if (inputSession.CompletionTarget != null)
            {
                // ［要求ファイル］を書き出します。
                StsaFileIOHelper.Write(
                    label: "要求ファイル",
                    outputPath: inputSession.CompletionTarget.RequestFileCreatePath,
                    lines: inputSession.CompletionTarget.RecordingInput.RecordedLines);
            }







            if (args.Length > 0)
            {
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

            Console.WriteLine("■［分析の前に］");

            // 前提入力は TournamentRule / PlayerList / RankingSettings の３境界だぜ（＾▽＾）！
            // 主線は TournamentFinalState → FinalRanking → TournamentQualityReport に寄せていくぜ（＾▽＾）！
            requestBoundary.AnalysisFlowMode = ConsolePromptReaders.ReadAnalysisFlowMode();

            // 対象［大会ルール］を選ばせるぜ（＾▽＾）！
            requestBoundary.RuleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(requestBoundary.AnalysisFlowMode);

            // メインライン選択のガイドを表示するぜ（＾▽＾）！
            ProgramConsoleGuide.PrintSelectedMainline(requestBoundary.AnalysisFlowMode, requestBoundary.RuleProfileMode);

            //      │
            //      ↓
            //      ■［分析］(`Analysis`)
            Console.WriteLine("■［分析］");
            AnalysisWorkflow.Run(requestBoundary);
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
