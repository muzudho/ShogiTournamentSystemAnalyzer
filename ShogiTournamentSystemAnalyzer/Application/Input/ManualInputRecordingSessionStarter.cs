/*
 * ［アプリケーション　＞　入力　＞　手動入力記録セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class ManualInputRecordingSessionStarter
{
    internal static RequestInputSession Start(string requestFileCreatePath)
    {
        var originalInput = Console.In;
        var recordingInput = new ManualInputRecordingTextReader(originalInput);
        var completionTarget = new RequestFileCreateCompletionTarget(requestFileCreatePath, recordingInput);

        Console.SetIn(recordingInput);
        Console.WriteLine($"分析中の入力を記録し、分析後に要求ファイルを作成します: {requestFileCreatePath}\n");
        return RequestInputSession.WithRequestFileCreateCompletion(originalInput, completionTarget);
    }
}