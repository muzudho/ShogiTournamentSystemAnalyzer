namespace ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestFileCheckResultVer2
{
    public RequestFileCheckResultVer2(
        bool hasError,
        IReadOnlyList<string> recordedLines)
    {
        HasError = hasError;
        RecordedLines = recordedLines;
    }

    public bool HasError { get; set; }

    // 記録した手動入力行
    internal IReadOnlyList<string> RecordedLines { get; init; }
}
