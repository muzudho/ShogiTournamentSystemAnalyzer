/*
 * ［プレゼンテーション　＞　コンソール改　＞　入力］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ConsoleInput
{
    static TextReader? inputReader;
    static List<string>? recordedLines;

    internal static void UseConsole()
    {
        inputReader = null;
    }

    internal static void UseText(string inputText)
    {
        inputReader = new StringReader(inputText);
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
        var line = inputReader is null
            ? Console.ReadLine()
            : inputReader.ReadLine();

        if (line is not null)
        {
            recordedLines?.Add(line);
        }

        return line;
    }
}
