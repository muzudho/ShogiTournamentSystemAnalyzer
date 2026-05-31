/*
 * ［アプリケーション　＞　要求ファイルチェック前　＞　入力セッション設定］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.ManualInput;
using ShogiTournamentSystemAnalyzer.Application.Shared;

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
            return ManualInputSessionStarter.Start();
        }

        if (!argumentResult.HasInputFile) return ManualInputSessionStarter.Start();

        if (RequestFileInputSessionStarter.TryStart(argumentResult.InputFilePath!, out var inputSession))
        {
            return inputSession;
        }

        Console.WriteLine("入力ファイルにエラーがあったため、手動入力へ切り替えます。\n");
        return ManualInputSessionStarter.Start();
    }
}