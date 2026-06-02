/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCreate;

internal sealed class RequestInputSession : IDisposable
{
    readonly TextReader? originalInput;
    internal readonly RequestFileCreateCompletionTarget? CompletionTarget;
    bool completed;
    bool disposed;

    RequestInputSession(
        TextReader? originalInput,
        RequestFileCreateCompletionTarget? completionTarget)
    {
        this.originalInput = originalInput;
        this.CompletionTarget = completionTarget;
    }

    internal static RequestInputSession WithoutCompletion()
    {
        return new RequestInputSession(null, null);
    }

    internal static RequestInputSession WithRequestFileCreateCompletion(
        TextReader originalInput,
        RequestFileCreateCompletionTarget completionTarget)
    {
        return new RequestInputSession(originalInput, completionTarget);
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
