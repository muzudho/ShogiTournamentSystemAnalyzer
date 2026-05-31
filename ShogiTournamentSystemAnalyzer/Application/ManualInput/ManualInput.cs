/*
 * ［アプリケーション　＞　実行　＞　手動入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.ManualInput;

using ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCreate;

using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

internal static class ManualInput
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