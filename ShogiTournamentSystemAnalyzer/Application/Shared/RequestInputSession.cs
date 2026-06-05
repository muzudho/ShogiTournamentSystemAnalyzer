/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

internal class RequestInputSession
{
    internal RequestInputSession(
        string? requestFileInputText,
        RequestFileCompletionTarget? completionTarget)
    {
        RequestFileInputText = requestFileInputText;
        RequestFileCompletionTarget = completionTarget;
    }

    internal string? RequestFileInputText { get; }

    internal RequestFileCompletionTarget? RequestFileCompletionTarget { get; }
}
