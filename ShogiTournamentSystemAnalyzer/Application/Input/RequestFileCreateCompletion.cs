/*
 * ［アプリケーション　＞　入力　＞　要求ファイル作成完了］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class RequestFileCreateCompletion
{
    internal static void Complete(string requestFileCreatePath, ManualInputRecordingTextReader recordingInput)
    {
        RequestFileCreate.Write(requestFileCreatePath, recordingInput.RecordedLines);
    }
}