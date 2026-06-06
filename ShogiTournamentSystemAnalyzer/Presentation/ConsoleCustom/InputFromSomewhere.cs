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

    internal static void UseConsole()
    {
        textReader = null;
    }

    internal static void UseText(string inputText)
    {
        textReader = new StringReader(inputText);
    }

    internal static string? ReadLine()
    {
        return textReader is null
            ? Console.ReadLine()
            : textReader.ReadLine();
    }
}
