/*
 * ［アプリケーション　＞　共有　＞　入力セッション］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Shared;

internal class RequestInputSession
{
    internal RequestInputSession(
        string? requestFileInputText,
        // 要求ファイル作成パス
        string requestFilePath,
        // 記録した手動入力行
        IReadOnlyList<string> recordedLines)
    {
        RequestFileInputText = requestFileInputText;
        RequestFilePath = requestFilePath;
        RecordedLines = recordedLines;
    }

    internal string? RequestFileInputText { get; }

    // 要求ファイル作成パス
    internal string RequestFilePath { get; init; }

    // 記録した手動入力行
    internal IReadOnlyList<string> RecordedLines { get; init; }

}
