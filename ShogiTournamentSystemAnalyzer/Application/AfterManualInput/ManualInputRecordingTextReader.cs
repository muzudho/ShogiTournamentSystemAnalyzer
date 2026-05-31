/*
 * ［アプリケーション　＞　実行　＞　手動入力後　＞　手動入力記録］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

internal sealed class ManualInputRecordingTextReader : TextReader
{
    readonly TextReader inner;
    readonly List<string> recordedLines = [];

    internal ManualInputRecordingTextReader(TextReader inner)
    {
        this.inner = inner;
    }

    internal IReadOnlyList<string> RecordedLines => recordedLines;

    public override string? ReadLine()
    {
        var line = inner.ReadLine();
        if (line is not null)
        {
            recordedLines.Add(line);
        }

        return line;
    }
}