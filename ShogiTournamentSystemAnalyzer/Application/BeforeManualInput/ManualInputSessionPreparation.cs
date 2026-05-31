/*
 * ［アプリケーション　＞　手動入力前　＞　手動入力セッション準備］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeManualInput;

using ShogiTournamentSystemAnalyzer.Application.ManualInput;
using ShogiTournamentSystemAnalyzer.Application.Shared;

internal static class ManualInputSessionPreparation
{
    internal static RequestInputSession StartForArgumentError(string errorMessage)
    {
        Console.WriteLine($"要求ファイルチェック: エラー有り: {errorMessage}");
        return Start();
    }

    internal static RequestInputSession StartWithoutRequestFile()
    {
        return Start();
    }

    internal static RequestInputSession StartAfterRequestFileCheckError()
    {
        Console.WriteLine("入力ファイルにエラーがあったため、手動入力へ切り替えます。\n");
        return Start();
    }

    static RequestInputSession Start()
    {
        Console.WriteLine("■［手動入力］");
        return ManualInputSessionStarter.Start();
    }
}