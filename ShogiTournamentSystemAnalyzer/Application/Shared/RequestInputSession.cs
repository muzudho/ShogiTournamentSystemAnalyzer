/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

internal class RequestInputSession
{
    internal RequestInputSession(
        string? requestFileInputText,
        ManualInputRecordingCompletionTarget? completionTarget)
    {
        RequestFileInputText = requestFileInputText;
        RecordingCompletionTarget = completionTarget;
    }

    internal string? RequestFileInputText { get; }

    internal ManualInputRecordingCompletionTarget? RecordingCompletionTarget { get; }
}
