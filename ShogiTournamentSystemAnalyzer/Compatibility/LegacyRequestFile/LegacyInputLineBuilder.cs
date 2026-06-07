/*
 * ［互換性　＞　レガシー要求ファイル　＞　レガシー入力列ビルダー］
 */
namespace ShogiTournamentSystemAnalyzer.Compatibility.LegacyRequestFile;

internal static class LegacyInputLineBuilder
{
    internal static void AppendDelimitedSection(List<string> destination, IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            destination.Add(line);
        }

        destination.Add(string.Empty);
    }

    internal static void AppendEndTerminatedSection(List<string> destination, IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            destination.Add(line);
        }

        destination.Add("END");
    }

}