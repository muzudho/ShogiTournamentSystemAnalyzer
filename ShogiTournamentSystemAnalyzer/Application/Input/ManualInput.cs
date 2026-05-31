/*
 * ［アプリケーション　＞　入力　＞　手動入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class ManualInput
{
    internal static RequestInputSession Start()
    {
        Console.WriteLine("■［手動入力］");
        var requestFileCreatePath = RequestFileCreatePrompt.ReadOutputPath();
        if (requestFileCreatePath is null)
        {
            Console.WriteLine();
            return RequestInputSession.FromManualInputWithoutRequestFileCreate();
        }

        return ManualInputRecordingSessionStarter.Start(requestFileCreatePath);
    }
}