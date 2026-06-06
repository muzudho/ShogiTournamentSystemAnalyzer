namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ApplicationTournamentUser
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns>成功か</returns>
    /// <exception cref="OperationCanceledException"></exception>
    internal static bool TryRunTournamentUserDomain(
        string[] args,
        out TournamentUserDomainResult result)
    {        //　　｜
        //　　｜　［大会ルールという境界］        `TournamentRule`
        //　　｜　［プレイヤー一覧という境界］    `PlayerList`
        //　　｜　［順位付けの設定という境界］    `RankingSettings`
        //　　↓

        result = null!;
        var analysisFlowSelection = AnalysisFlowSelection.FromSingle(AnalysisFlowMode.Simulation);
        var ruleProfileAttributes = RuleProfileAttributes.CreateStandardScheduled();
        AnalysisRequest? analysisRequest = null;    // TODO: これは現在の本命フローに合わせている（＾～＾）この変数を育てていき、将来的には旧フロー用の選択値は解消したい（＾～＾）？

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
        if (argumentResult.HasRequestFile)
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
                && StsaInputRequestParser.TryParse(requestFileCheckResult.RequestText, out var parsedAnalysisRequest)
                && parsedAnalysisRequest is not null)
            {
                requestText = null;
                analysisRequest = parsedAnalysisRequest;
                analysisFlowSelection = parsedAnalysisRequest.FlowSelection;
                ruleProfileAttributes = parsedAnalysisRequest.Steps[0].GetRuleProfileAttributes();
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

            // TODO: これも入力に含めたいぜ（＾～＾）
            analysisFlowSelection = ConsolePromptReaders.ReadAnalysisFlowSelection();

            ruleProfileAttributes = ConsolePromptReaders.ReadRuleProfileAttributes(analysisFlowSelection);

            ManualAnalysisRequestReader.TryRead(analysisFlowSelection, ruleProfileAttributes, out analysisRequest);

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
            var requestFilePath = InputRequestFilePath();
            WriteRequestFile(analysisRequest, requestFilePath);
        }

        if (requestText is not null)
        {
            InputFromSomewhere.UseText(requestText);
            analysisFlowSelection = ConsolePromptReaders.ReadAnalysisFlowSelection();
            ruleProfileAttributes = ConsolePromptReaders.ReadRuleProfileAttributes(analysisFlowSelection);
        }

        result = new TournamentUserDomainResult(
            analysisFlowSelection,
            ruleProfileAttributes,
            analysisRequest);
        return true;
    }

    internal sealed record TournamentUserDomainResult(
        AnalysisFlowSelection AnalysisFlowSelection,
        RuleProfileAttributes RuleProfileAttributes,
        AnalysisRequest? AnalysisRequest);

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
                "STSAInput/5" => throw new OperationCanceledException("STSAInput/5 は直通 parser 専用です。RuleProfileAttributes と実行ステップの組み合わせを確認してください。"),
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
        AnalysisRequest? analysisRequest,
        string? requestFilePath)
    {
        if (string.IsNullOrWhiteSpace(requestFilePath)) return;

        if (analysisRequest is null)
        {
            Console.WriteLine("要求ファイル書出は、STSAInput/4 へ変換済みの手入力だけ対応しています。raw 手入力ログは保存しません。\n");
            return;
        }

        Console.WriteLine($"要求ファイルを書き出します: {requestFilePath}\n");
        StsaFileIOHelper.Write(
            label: "要求ファイル",
            outputPath: requestFilePath,
            lines: StsaInputRequestWriter.BuildAttributeLines(analysisRequest));
    }
}
