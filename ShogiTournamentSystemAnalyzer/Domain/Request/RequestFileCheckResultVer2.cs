namespace ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestFileCheckResultVer2
{
    public RequestFileCheckResultVer2(
        bool hasError,
        string? requestFileInputText,
        IReadOnlyList<string> recordedLines)
    {
        HasError = hasError;
        RequestFileInputText = requestFileInputText;
        RecordedLines = recordedLines;
    }

    public bool HasError { get; set; }

    // ファイル入力テキスト
    internal string? RequestFileInputText { get; init; }

    // 記録した手動入力行
    internal IReadOnlyList<string> RecordedLines { get; init; }
}
