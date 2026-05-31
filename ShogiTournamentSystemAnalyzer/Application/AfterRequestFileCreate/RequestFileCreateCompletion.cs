/*
 * ［アプリケーション　＞　実行　＞　要求ファイル作成後　＞　要求ファイル作成完了］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class RequestFileCreateCompletion
{
    internal static void Complete(RequestFileCreateCompletionTarget target)
    {
        RequestFileCreate.Write(target.RequestFileCreatePath, target.RecordingInput.RecordedLines);
    }
}