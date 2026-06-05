/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

internal class RequestInputSession
{
    internal RequestInputSession(
        string? requestFileInputText,
        // 記録した手動入力行
        IReadOnlyList<string> recordedLines)
    {
        RequestFileInputText = requestFileInputText;
        RecordedLines = recordedLines;
    }

    // ファイル入力テキスト
    internal string? RequestFileInputText { get; }

    // 記録した手動入力行
    internal IReadOnlyList<string> RecordedLines { get; init; }

}
