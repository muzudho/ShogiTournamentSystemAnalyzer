/*
 * ［アプリケーション　＞　要求ファイル記入中　＞　要求テキスト入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileWriting;

internal sealed class RequestTextInput : TextReader
{
    readonly TextReader _textReader;
    readonly List<string> _recordedLines = [];

    internal RequestTextInput(TextReader textReader)
    {
        this._textReader = textReader;
    }

    internal IReadOnlyList<string> RecordedLines => _recordedLines;

    public override string? ReadLine()
    {
        var line = _textReader.ReadLine();
        if (line is not null)
        {
            _recordedLines.Add(line);
        }

        return line;
    }
}
