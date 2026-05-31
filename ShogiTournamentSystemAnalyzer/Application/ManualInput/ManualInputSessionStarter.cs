/*
 * ［アプリケーション　＞　手動入力　＞　手動入力セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.ManualInput;

using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCreate;
using ShogiTournamentSystemAnalyzer.Application.Shared;

internal static class ManualInputSessionStarter
{
    internal static RequestInputSession Start()
    {
        var requestFileCreatePath = RequestFileCreatePrompt.ReadOutputPath();
        return requestFileCreatePath is null
            ? StartWithoutRecording()
            : ManualInputRecordingSessionStarter.Start(requestFileCreatePath);
    }

    static RequestInputSession StartWithoutRecording()
    {
        Console.WriteLine();
        return RequestInputSession.WithoutCompletion();
    }
}