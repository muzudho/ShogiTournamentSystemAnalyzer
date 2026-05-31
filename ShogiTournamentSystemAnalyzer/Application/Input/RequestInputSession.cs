/*
 * ［アプリケーション　＞　入力　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

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

    internal static RequestInputSession FromRequestFile()
    {
        return new RequestInputSession(null, null);
    }

    internal static RequestInputSession FromManualInputWithoutRequestFileCreate()
    {
        return new RequestInputSession(null, null);
    }

    internal static RequestInputSession FromManualInputWithRequestFileCreate(
        TextReader originalInput,
        ManualInputRecordingTextReader recordingInput,
        string requestFileCreatePath)
    {
        var completionTarget = new RequestFileCreateCompletionTarget(requestFileCreatePath, recordingInput);
        return new RequestInputSession(originalInput, completionTarget);
    }

    internal void CompleteRequestFileCreate()
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