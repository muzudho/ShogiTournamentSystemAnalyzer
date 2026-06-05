/*
 * ［アプリケーション　＞　手動入力後　＞　手入力ログ記録］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

internal sealed class ManualInputLogTextReader : TextReader
{
    readonly TextReader inner;
    readonly List<string> recordedLines = [];

    internal ManualInputLogTextReader(TextReader inner)
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