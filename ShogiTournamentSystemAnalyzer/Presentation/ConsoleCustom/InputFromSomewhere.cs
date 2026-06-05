/*
 * ［プレゼンテーション　＞　コンソール改　＞　入力］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// 入力元を、［コンソール］か［テキスト］か、切り替えるためのクラス。
/// </summary>
internal static class InputFromSomewhere
{
    static TextReader? textReader;
    static List<string>? recordedLines;

    internal static void UseConsole()
    {
        textReader = null;
    }

    internal static void UseText(string inputText)
    {
        textReader = new StringReader(inputText);
    }

    internal static IReadOnlyList<string> StartRecording()
    {
        recordedLines = [];
        return recordedLines;
    }

    internal static void StopRecording()
    {
        recordedLines = null;
    }

    internal static void PauseRecording()
    {
        recordedLines = null;
    }

    internal static IReadOnlyList<string> ResumeRecording(IReadOnlyList<string> lines)
    {
        recordedLines = lines as List<string> ?? [.. lines];
        return recordedLines;
    }

    internal static string? ReadLine()
    {
        var line = textReader is null
            ? Console.ReadLine()
            : textReader.ReadLine();

        if (line is not null)
        {
            recordedLines?.Add(line);
        }

        return line;
    }
}
