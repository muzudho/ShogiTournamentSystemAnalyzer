/*
 * ［アプリケーション　＞　実行　＞　要求ファイルチェック前　＞　入力元設定］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.ManualInput;

using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCheck;

internal static class InputSourceConfiguration
{
    /// <summary>
    /// 入力元の種類を設定する
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    /// <returns>設定された入力セッション</returns>
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