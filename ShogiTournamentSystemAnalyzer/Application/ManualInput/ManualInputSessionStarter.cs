/*
 * ［アプリケーション　＞　手動入力　＞　手動入力セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.ManualInput;

using ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCreate;

using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

internal static class ManualInputSessionStarter
{
    internal static RequestInputSession Start()
    {
        Console.WriteLine("■［手動入力］");
        var requestFileCreatePath = RequestFileCreatePrompt.ReadOutputPath();
        if (requestFileCreatePath is null)
        {
            Console.WriteLine();
            return RequestInputSession.WithoutCompletion();
        }

        return ManualInputRecordingSessionStarter.Start(requestFileCreatePath);
    }
}