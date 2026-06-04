/*
 * ［プログラム　＞　エントリーポイント］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;
using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCreate;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
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
            // ［●開始］

            #region ［■辺１］

            // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
            Console.OutputEncoding = Encoding.UTF8;
            ConsoleInput.UseConsole();

            // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
            SimulationTimeBudget.BeginApplicationBudget();

            // このプログラムの説明を最初に表示するぜ（＾▽＾）！
            ProgramConsoleGuide.PrintProgramIntroduction();

            // ［依頼という境界］
            RequestBoundary requestBoundary = new();

            RequestInputSession? inputSession;

            #endregion

            #region ［◆節１：コマンドライン引数で入力ファイルを指定したか？

            var argumentResult = RequestFileArgumentReader.Read(args);

            // ［要求ファイル］確認中の異常の場合。
            if (argumentResult.HasError)
            {
                Console.WriteLine($"●異常終了：　［要求ファイル］確認中。 {argumentResult.ErrorMessage!}");
                return;
            }

            #endregion

            // ［■辺２：はい、入力ファイル指定有り］
            if (argumentResult.HasInputFile)
            {
                // ［□要求ファイルチェック(`RequestFileCheck`)］
                var requestFileCheckResultVer2 = RequestFileCheckWorkflow.Run(argumentResult);


                // ［◆節２：エラーが有ったか？］
                if (requestFileCheckResultVer2.HasError)
                {
                    // ［■辺３：はい、エラー有り］
                    // ［●終了１］
                    return;
                }

                // ［■辺４：いいえ、エラー無し］
                inputSession = requestFileCheckResultVer2.InputSession;
            }
            //  ［■辺５：いいえ、入力ファイル指定無し］
            else
            {
                Console.WriteLine("■［手動入力］");

                // ［□手動入力（`ManualInput`）］
                var requestModelProducer = new RequestModelFromManualProducer();

                // ［◆節３：エラーが有ったか？］

                // ［■辺６：はい、エラー有り］
                if (requestModelProducer.HasError)
                {
                    // ［●終了２］
                    return;
                }

                // ［■辺７：いいえ、エラー無し］
                requestModelProducer.Produce(requestBoundary);

                // 👇［節４］～［辺９］
                var requestFileCreatePath = RequestFileCreatePrompt.InputRequestFilePath();

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

            if (inputSession is null)
            {
                Console.WriteLine("●異常終了：　入力セッションを開始できませんでした。");
                return;
            }

            if (inputSession.RequestFileInputText is not null)
            {
                ConsoleInput.UseText(inputSession.RequestFileInputText);
            }

            #region ［■辺１０：分析の前に］

            Console.WriteLine("■［分析の前に］");

            // 前提入力は TournamentRule / PlayerList / RankingSettings の３境界だぜ（＾▽＾）！
            // 主線は TournamentFinalState → FinalRanking → TournamentQualityReport に寄せていくぜ（＾▽＾）！
            requestBoundary.AnalysisFlowMode = ConsolePromptReaders.ReadAnalysisFlowMode();

            // 対象［大会ルール］を選ばせるぜ（＾▽＾）！
            requestBoundary.RuleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(requestBoundary.AnalysisFlowMode);

            // メインライン選択のガイドを表示するぜ（＾▽＾）！
            ProgramConsoleGuide.PrintSelectedMainline(requestBoundary.AnalysisFlowMode, requestBoundary.RuleProfileMode);

            #endregion

            //      │
            //      ↓
            //      ［□分析(`Analysis`)］
            Console.WriteLine("■［分析］");
            AnalysisWorkflow.Run(requestBoundary);

            if (inputSession.CompletionTarget != null)
            {
                // ［要求ファイル］を書き出します。
                StsaFileIOHelper.Write(
                    label: "要求ファイル",
                    outputPath: inputSession.CompletionTarget.RequestFileCreatePath,
                    lines: inputSession.CompletionTarget.RecordedLines);
                ConsoleInput.StopRecording();
            }

            //      │
            //      ↓
            // ［●終了］
            return;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }

}
