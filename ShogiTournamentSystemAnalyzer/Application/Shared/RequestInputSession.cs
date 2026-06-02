/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCreate;

internal class RequestInputSession : IDisposable
{
    readonly TextReader? originalInput;
    internal readonly RequestFileCreateCompletionTarget? CompletionTarget;
    bool completed;
    bool disposed;

    internal RequestInputSession(
        TextReader? originalInput,
        RequestFileCreateCompletionTarget? completionTarget)
    {
        this.originalInput = originalInput;
        this.CompletionTarget = completionTarget;
    }

    public void Dispose()
    {
        if (disposed) return;

        disposed = true;
        if (originalInput is not null)
        {
            Console.SetIn(originalInput);
        }
    }
}
