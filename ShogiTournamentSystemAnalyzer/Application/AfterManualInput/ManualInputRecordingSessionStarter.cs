/*
 * ［アプリケーション　＞　手動入力後　＞　手動入力記録セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ManualInputRecordingSessionStarter
{
    internal static RequestInputSession Start(string recordingLogPath)
    {
        var recordedLines = ConsoleInput.StartRecording();
        var recordingCompletionTarget = new ManualInputRecordingCompletionTarget(recordingLogPath, recordedLines);

        Console.WriteLine($"分析中の入力を記録し、分析後に録音ログを作成します: {recordingLogPath}\n");
        return new RequestInputSession(requestFileInputText: null, recordingCompletionTarget);
    }
}
