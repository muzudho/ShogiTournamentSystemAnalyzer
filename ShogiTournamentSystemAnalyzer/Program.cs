/*
 * ［プログラム　＞　エントリーポイント］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Ranking;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;
using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Request;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;
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

            // ［■辺１］
            Opening();


            // ========================================
            // ［大会利用者域］（`TournamentUser`）
            // ========================================


            // ［依頼という境界］
            RequestBoundary requestBoundary = new();

            RequestInputSession? requestInputSession = null;

            RunTournamentUserDomain(args, requestBoundary, ref requestInputSession);


            // ========================================
            // ［□分析(`Analysis`)］
            // ========================================


            // メインライン選択のガイドを表示するぜ（＾▽＾）！
            ProgramConsoleGuide.PrintSelectedMainline(requestBoundary.AnalysisFlowSelection, requestBoundary.RuleProfileMode);

            //      │
            //      ↓
            //      
            Console.WriteLine("■［分析］");
            TournamentFinalStateBoundary tournamentFinalStateBoundary = new();
            //［シミュレーション域］
            SimulationWorkflow.Run(requestBoundary, tournamentFinalStateBoundary);
            //　　｜
            //　　｜　［大会最終状態という境界］      `TournamentFinalState`
            //　　↓
            //［順位付け域］
            FinalRankingBoundary finalRankingBoundary = new();
            RankingWorkflow.Run(tournamentFinalStateBoundary, finalRankingBoundary);
            //　　｜
            //　　｜　［最終順位という境界］          `FinalRanking`
            //　　↓
            //［大会品質評価フロー域］                `TournamentQualityEvaluator`
            TournamentQualityEvaluatorWorkflow.Run(finalRankingBoundary);
            //　　｜
            //　　｜　［大会品質レポートという境界］  `TournamentQualityReport`
            //　　↓


            // 本処理（選択フロー）
            AnalysisFlowDispatcher.Execute(requestBoundary.AnalysisFlowSelection, requestBoundary.RuleProfileMode);

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

    /// <summary>
    /// ［開始］
    /// </summary>
    private static void Opening()
    {
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.OutputEncoding = Encoding.UTF8;
        ConsoleInput.UseConsole();

        // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
        SimulationTimeBudget.BeginApplicationBudget();

        // このプログラムの説明を最初に表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintProgramIntroduction();
    }

    private static void RunTournamentUserDomain(
        string[] args,
        RequestBoundary requestBoundary,
        ref RequestInputSession? requestInputSession)
    {
        //　　｜
        //　　｜　［大会ルールという境界］        `TournamentRule`
        //　　｜　［プレイヤー一覧という境界］    `PlayerList`
        //　　｜　［順位付けの設定という境界］    `RankingSettings`
        //　　↓


        #region ［◆節１：コマンドライン引数で要求ファイルを指定したか？

        var argumentResult = RequestFileArgumentReader.Read(args);

        // ［要求ファイル］確認中の異常の場合。
        if (argumentResult.HasError)
        {
            Console.WriteLine($"●異常終了：　［要求ファイル］確認中。 {argumentResult.ErrorMessage!}");
            return;
        }

        #endregion

        // ［■辺２：はい、要求ファイル指定有り］
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
            requestInputSession = requestFileCheckResultVer2.InputSession;
        }
        //  ［■辺５：いいえ、入力ファイル指定無し］
        else
        {
            Console.WriteLine("■［手動入力］");

            // ［□手動入力（`ManualInput`）］
            //
            //  📍 TODO: ここで、大会ルールを入力するプログラムを作りたい。今は空っぽ。
            //
            var requestModelProducer = new RequestModelFromManualProducer();

            // 前提入力は TournamentRule / PlayerList / RankingSettings の３境界だぜ（＾▽＾）！
            // 主線は TournamentFinalState → FinalRanking → TournamentQualityReport に寄せていくぜ（＾▽＾）！
            requestBoundary.AnalysisFlowSelection = ConsolePromptReaders.ReadAnalysisFlowSelection();

            // 対象［大会ルール］を選ばせるぜ（＾▽＾）！
            requestBoundary.RuleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(requestBoundary.AnalysisFlowSelection);

            // ［◆節３：エラーが有ったか？］

            // ［■辺６：はい、エラー有り］
            if (requestModelProducer.HasError)
            {
                // ［●終了２］
                return;
            }

            // ［■辺７：いいえ、エラー無し］
            requestModelProducer.Produce(requestBoundary);

            //  ［要求ファイル］の保存先パスを尋ねるだけ（＾～＾） まだ保存はしない。
            static string? InputRequestFilePath()
            {
                // ［◆節４：今回の手入力を要求ファイルとして書き出しておきますか？］
                Console.WriteLine("今回の手入力を要求ファイルとして書き出しておきますか？");
                Console.WriteLine("1. いいえ");
                Console.WriteLine("2. はい\n");

                var attempt = 0;
                while (true)
                {
                    attempt++;
                    Console.Write("番号を入力してください [1]: ");
                    var input = ConsoleInput.ReadLine()?.Trim();
                    if (input is null) throw new OperationCanceledException("要求ファイル書出中に入力ストリームが終了しました。");

                    // ［■辺８：はい、書き出します］
                    if (input == "2")
                    {
                        // ［□要求ファイル書出］
                        Console.WriteLine("■［要求ファイル書出］");
                        var defaultPath = RequestFile.BuildDefaultPath();
                        var outputPath = ConsolePromptReaders.ReadTextWithDefault(
                            $"要求ファイルの出力先パスまたはフォルダーパスを入力してください [{defaultPath}]: ",
                            defaultPath);

                        return RequestFile.ResolveOutputPath(outputPath);
                    }

                    // ［■辺９：いいえ、書き出しません］
                    if (string.IsNullOrEmpty(input) || input == "1") return null;

                    if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("要求ファイル書出選択", "1 または 2 以外が入力されました");

                    Console.WriteLine("1 か 2 を入力してください。\n");
                }
            }
            var requestFilePath = InputRequestFilePath();

            // ［要求ファイル］を書き出します。
            void WriteRequestFile(
                ref RequestInputSession? requestInputSession,
                string? requestFilePath)
            {

                if (requestFilePath is null)
                {
                    Console.WriteLine();
                    requestInputSession = new RequestInputSession(null, null);
                }
                else
                {
                    requestInputSession = RequestFileCreationSessionStarter.Start(requestFilePath);
                }

                if (requestInputSession is null)
                {
                    Console.WriteLine("●異常終了：　入力セッションを開始できませんでした。");
                    return;
                }

                if (requestInputSession.RequestFileInputText is not null)
                {
                    ConsoleInput.UseText(requestInputSession.RequestFileInputText);
                }

                // ［要求ファイル］は、分析中の入力記録が揃ってから書き出す。
                if (requestInputSession.RequestFileCompletionTarget != null)
                {
                    // ［要求ファイル］を書き出します。
                    StsaFileIOHelper.Write(
                        label: "要求ファイル",
                        outputPath: requestInputSession.RequestFileCompletionTarget.ManualInputLogPath,
                        lines: requestInputSession.RequestFileCompletionTarget.RecordedLines);
                    ConsoleInput.StopRecording();
                }
            }
            WriteRequestFile(ref requestInputSession, requestFilePath);
        }
    }
}
