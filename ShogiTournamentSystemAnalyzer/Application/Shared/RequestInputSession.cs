/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCreate;

internal sealed class RequestInputSession : IDisposable
{
    readonly TextReader? originalInput;
    readonly RequestFileCreateCompletionTarget? completionTarget;
    bool completed;
    bool disposed;

    RequestInputSession(
        TextReader? originalInput,
        RequestFileCreateCompletionTarget? completionTarget)
    {
        this.originalInput = originalInput;
        this.completionTarget = completionTarget;
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

    internal void Complete()
    {
        if (completed || completionTarget is null) return;

        completed = true;
        RequestFileCreateCompletion.Complete(completionTarget);
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