/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCreate;

internal class RequestInputSession
{
    internal RequestInputSession(
        string? requestFileInputText,
        RequestFileCreateCompletionTarget? completionTarget)
    {
        RequestFileInputText = requestFileInputText;
        CompletionTarget = completionTarget;
    }

    internal string? RequestFileInputText { get; }

    internal RequestFileCreateCompletionTarget? CompletionTarget { get; }
}
