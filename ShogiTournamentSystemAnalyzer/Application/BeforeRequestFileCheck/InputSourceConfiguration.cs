/*
 * ［アプリケーション　＞　要求ファイルチェック前　＞　入力セッション設定］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.ManualInput;

using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCheck;

internal static class InputSourceConfiguration
{
    /// <summary>
    /// コマンドライン引数に応じて、要求ファイル入力または手動入力のセッションを準備する
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    /// <returns>準備された入力セッション</returns>
    internal static RequestInputSession ConfigureInputSource(IReadOnlyList<string> args)
    {
        var argumentResult = RequestFileArgumentReader.Read(args);
        if (argumentResult.HasError)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {argumentResult.ErrorMessage}");
            return ManualInput.Start();
        }

        if (argumentResult.HasInputFile)
        {
            return TryStartFromRequestFile(argumentResult.InputFilePath!);
        }

        return ManualInput.Start();
    }

    static RequestInputSession TryStartFromRequestFile(string inputFilePath)
    {
        if (RequestFileCheck.TryRead(inputFilePath, RequestInputFileReader.Read, out var checkedInputFile))
        {
            RequestInputApplier.Apply(checkedInputFile);
            return RequestInputSession.WithoutCompletion();
        }

        Console.WriteLine("入力ファイルにエラーがあったため、手動入力へ切り替えます。\n");
        return ManualInput.Start();
    }
}