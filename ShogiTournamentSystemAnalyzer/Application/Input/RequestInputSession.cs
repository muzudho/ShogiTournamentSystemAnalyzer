/*
 * ［アプリケーション　＞　入力　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal sealed class RequestInputSession : IDisposable
{
    readonly TextReader? originalInput;
    readonly ManualInputRecordingTextReader? recordingInput;
    readonly string? requestFileCreatePath;
    bool completed;
    bool disposed;

    RequestInputSession(
        TextReader? originalInput,
        ManualInputRecordingTextReader? recordingInput,
        string? requestFileCreatePath)
    {
        this.originalInput = originalInput;
        this.recordingInput = recordingInput;
        this.requestFileCreatePath = requestFileCreatePath;
    }

    internal static RequestInputSession FromRequestFile()
    {
        return new RequestInputSession(null, null, null);
    }

    internal static RequestInputSession FromManualInputWithoutRequestFileCreate()
    {
        return new RequestInputSession(null, null, null);
    }

    internal static RequestInputSession FromManualInputWithRequestFileCreate(
        TextReader originalInput,
        ManualInputRecordingTextReader recordingInput,
        string requestFileCreatePath)
    {
        return new RequestInputSession(originalInput, recordingInput, requestFileCreatePath);
    }

    internal void CompleteRequestFileCreate()
    {
        if (completed || recordingInput is null || string.IsNullOrWhiteSpace(requestFileCreatePath)) return;

        completed = true;
        RequestFileCreate.Write(requestFileCreatePath, recordingInput.RecordedLines);
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