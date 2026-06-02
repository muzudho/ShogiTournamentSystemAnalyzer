/*
 * ［アプリケーション　＞　手動入力後　＞　手動入力記録セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCreate;
using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ManualInputRecordingSessionStarter
{
    internal static RequestInputSession Start(string requestFileCreatePath)
    {
        var recordedLines = ConsoleInput.StartRecording();
        var completionTarget = new RequestFileCreateCompletionTarget(requestFileCreatePath, recordedLines);

        Console.WriteLine($"分析中の入力を記録し、分析後に要求ファイルを作成します: {requestFileCreatePath}\n");
        return new RequestInputSession(requestFileInputText: null, completionTarget);
    }
}
