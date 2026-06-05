namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Request;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;
using System.Text;

internal static class ApplicationWorkflow
{
    public static void Run(string[] args)
    {
        // ●開始

        //┌───┴──┐
        //│オープニング│
        //└───┬──┘
        Opening();

        // ［依頼という境界］
        RequestBoundary requestBoundary = new();    // TODO: RequestBoundary と AnalysisRequest はどっちも RunAnalysisDomain メソッドに渡されるけど、１つにまとまらないのかだぜ（＾～＾）？
        IReadOnlyList<string> recordedLines = Array.Empty<string>();
        string? requestFilePath = null;
        AnalysisRequest? analysisRequest = null;    // TODO: RequestBoundary と AnalysisRequest はどっちも RunAnalysisDomain メソッドに渡されるけど、１つにまとまらないのかだぜ（＾～＾）？

        //┌───┴──┐
        //│大会利用者域│
        //└───┬──┘
        bool isSuccessful = RunTournamentUserDomain(
            args,
            requestBoundary,
            ref recordedLines,
            ref requestFilePath,
            ref analysisRequest);
        if (!isSuccessful) return;  // エラー終了

        //┌───┴──┐
        //│分析　　　　│
        //└───┬──┘
        RunAnalysisDomain(requestBoundary, analysisRequest);

        InputFromSomewhere.PauseRecording();
        WriteRequestFile(recordedLines, requestFilePath);   // TODO: ［要求ファイル］の書き込みは、［大会利用者域］の仕事だぜ（＾～＾）
        InputFromSomewhere.StopRecording();

        // ●終了
        return;
    }

    /// <summary>
    /// ［開始］
    /// </summary>
    private static void Opening()
    {
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.OutputEncoding = Encoding.UTF8;
        InputFromSomewhere.UseConsole();

        // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
        SimulationTimeBudget.BeginApplicationBudget();

        // このプログラムの説明を最初に表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintProgramIntroduction();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <param name="requestBoundary"></param>
    /// <returns>成功か</returns>
    /// <exception cref="OperationCanceledException"></exception>
    private static bool RunTournamentUserDomain(
        string[] args,
        RequestBoundary requestBoundary,
        ref IReadOnlyList<string> recordedLines,
        ref string? requestFilePath,
        ref AnalysisRequest? analysisRequest)
    {        //　　｜
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
            return false;
        }

        #endregion

        // 要求テキスト
        string? requestText = null;

        // ［■辺２：はい、要求ファイル指定有り］
        if (argumentResult.HasInputFile)
        {
            // ［□要求ファイルチェック(`RequestFileCheck`)］
            var requestFileCheckResult = RequestFileCheckWorkflow.Run(argumentResult);

            // ［◆節２：エラーが有ったか？］
            if (!requestFileCheckResult.IsSuccessful)
            {
                // ［■辺３：はい、エラー有り］
                // ［●終了１］
                return false;
            }

            if (requestFileCheckResult.RequestText is not null
                && StsaInput4RequestParser.TryParse(requestFileCheckResult.RequestText, out var parsedAnalysisRequest)
                && parsedAnalysisRequest is not null)
            {
                requestText = null;
                analysisRequest = parsedAnalysisRequest;
                requestBoundary.AnalysisFlowSelection = parsedAnalysisRequest.FlowSelection;
                requestBoundary.RuleProfileMode = parsedAnalysisRequest.RuleProfileMode;
            }
            else
            {
                if (!TryConvertToLegacyInputText(requestFileCheckResult.RequestText, out requestText)) return false;
            }

            // ［■辺４：いいえ、エラー無し］
        }
        //  ［■辺５：いいえ、入力ファイル指定無し］
        else
        {
            Console.WriteLine("■［手動入力］");

            // ［□手動入力（`ManualInput`）］
            //
            //  📍 TODO: ここで、大会ルールを入力するプログラムを作りたい。今は空っぽ。
            //

            // 記録した手動入力行
            recordedLines = InputFromSomewhere.StartRecording();

            // TODO: これも入力に含めたいぜ（＾～＾）
            requestBoundary.AnalysisFlowSelection = ConsolePromptReaders.ReadAnalysisFlowSelection();

            // TODO: これも入力に含めたいぜ（＾～＾）
            requestBoundary.RuleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(requestBoundary.AnalysisFlowSelection);

            // ［◆節３：エラーが有ったか？］

            //// ［■辺６：はい、エラー有り］
            //if (false)
            //{
            //    // ［●終了２］
            //    return false;
            //}

            // ［■辺７：いいえ、エラー無し］

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
                    var input = InputFromSomewhere.ReadLine()?.Trim();
                    if (input is null) throw new OperationCanceledException("要求ファイル書出中に入力ストリームが終了しました。");

                    // ［■辺８：はい、書き出します］
                    if (input == "2")
                    {
                        // ［□要求ファイル書出］
                        Console.WriteLine("■［要求ファイル書出］");
                        var defaultPath = RequestFilePath.BuildDefaultPath();
                        var outputPath = ConsolePromptReaders.ReadTextWithDefault(
                            $"要求ファイルの出力先パスまたはフォルダーパスを入力してください [{defaultPath}]: ",
                            defaultPath);

                        return RequestFilePath.ResolveOutputPath(outputPath);
                    }

                    // ［■辺９：いいえ、書き出しません］
                    if (string.IsNullOrEmpty(input) || input == "1") return null;

                    if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("要求ファイル書出選択", "1 または 2 以外が入力されました");

                    Console.WriteLine("1 か 2 を入力してください。\n");
                }
            }
            InputFromSomewhere.PauseRecording();
            requestFilePath = InputRequestFilePath();
            recordedLines = InputFromSomewhere.ResumeRecording(recordedLines);
        }

        if (requestText is not null)
        {
            InputFromSomewhere.UseText(requestText);
            requestBoundary.AnalysisFlowSelection = ConsolePromptReaders.ReadAnalysisFlowSelection();
            requestBoundary.RuleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(requestBoundary.AnalysisFlowSelection);
        }

        return true;
    }

    static bool TryConvertToLegacyInputText(RequestText? checkedRequestText, out string? requestText)
    {
        requestText = null;
        if (checkedRequestText is null)
        {
            Console.WriteLine("●エラー終了：　［要求ファイル］パース中エラー。 要求テキストがありません。");
            return false;
        }

        try
        {
            requestText = checkedRequestText.FormatName switch
            {
                "STSAInput/4" => StsaInputLegacyConverter.ConvertStsaInput4ToLegacyInput(checkedRequestText.Lines, checkedRequestText.SourcePath ?? "(要求テキスト)"),
                "STSAInput/3" => StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(checkedRequestText.Lines, checkedRequestText.SourcePath ?? "(要求テキスト)"),
                "STSAInput/2" => StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(checkedRequestText.Lines, checkedRequestText.SourcePath ?? "(要求テキスト)"),
                _ => LegacyInputFileFilter.ConvertToFilteredInput(checkedRequestText.Lines),
            };
            return true;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"●エラー終了：　［要求ファイル］パース中エラー。 {ex.Message}");
            return false;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"●エラー終了：　［要求ファイル］パース中エラー。 {ex.Message}");
            return false;
        }
    }
    /// <summary>
    /// ［要求ファイル］を書き出します。
    /// </summary>
    private static void WriteRequestFile(
        IReadOnlyList<string> recordedLines,
        string? requestFilePath)
    {
        if (!string.IsNullOrWhiteSpace(requestFilePath) && recordedLines.Count > 0)
        {
            Console.WriteLine($"要求ファイルを書き出します: {requestFilePath}\n");
            StsaFileIOHelper.Write(
                label: "要求ファイル",
                outputPath: requestFilePath,
                lines: recordedLines);
        }
    }
    /// <summary>
    /// ［分析］
    /// </summary>
    private static void RunAnalysisDomain(
        RequestBoundary requestBoundary,
        AnalysisRequest? analysisRequest)
    {
        // メインライン選択のガイドを表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintSelectedMainline(requestBoundary.AnalysisFlowSelection, requestBoundary.RuleProfileMode);

        //      │
        //      ↓
        //      
        Console.WriteLine("■［分析］");
        //TournamentFinalStateBoundary tournamentFinalStateBoundary = new();
        //［シミュレーション域］
        //　　｜
        //　　｜　［大会最終状態という境界］      `TournamentFinalState`
        //　　↓
        //［順位付け域］
        //FinalRankingBoundary finalRankingBoundary = new();
        //　　｜
        //　　｜　［最終順位という境界］          `FinalRanking`
        //　　↓
        //［大会品質評価フロー域］                `TournamentQualityEvaluator`
        //　　｜
        //　　｜　［大会品質レポートという境界］  `TournamentQualityReport`
        //　　↓


        // 本処理（選択フロー）
        if (analysisRequest is not null)
        {
            AnalysisRequestDispatcher.Execute(analysisRequest);
            return;
        }

        AnalysisFlowDispatcher.Execute(requestBoundary.AnalysisFlowSelection, requestBoundary.RuleProfileMode);

    }
}
