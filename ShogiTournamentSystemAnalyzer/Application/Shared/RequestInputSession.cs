/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

internal class RequestInputSession
{
    internal RequestInputSession(
        string? requestFileInputText,
        ManualInputLogCompletionTarget? completionTarget)
    {
        RequestFileInputText = requestFileInputText;
        ManualInputLogCompletionTarget = completionTarget;
    }

    internal string? RequestFileInputText { get; }

    internal ManualInputLogCompletionTarget? ManualInputLogCompletionTarget { get; }
}
