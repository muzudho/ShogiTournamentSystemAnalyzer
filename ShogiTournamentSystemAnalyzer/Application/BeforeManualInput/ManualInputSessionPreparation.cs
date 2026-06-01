/*
 * ［アプリケーション　＞　手動入力前　＞　手動入力セッション準備］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeManualInput;

using ShogiTournamentSystemAnalyzer.Application.ManualInput;
using ShogiTournamentSystemAnalyzer.Application.Shared;

internal static class ManualInputSessionPreparation
{
    internal static RequestInputSession StartWithoutRequestFile()
    {
        return Start();
    }

    static RequestInputSession Start()
    {
        Console.WriteLine("■［手動入力］");
        return ManualInputSessionStarter.Start();
    }
}
