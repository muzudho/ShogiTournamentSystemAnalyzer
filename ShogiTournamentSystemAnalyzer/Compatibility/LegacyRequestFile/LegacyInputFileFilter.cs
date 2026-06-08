/*
 * ［互換性　＞　レガシー要求ファイル　＞　レガシー入力ファイルフィルター］
 */
namespace ShogiTournamentSystemAnalyzer.Compatibility.LegacyRequestFile;

internal static class LegacyInputFileFilter
{
    internal static string ConvertToFilteredInput(IEnumerable<string> rawLines)
    {
        var filteredLines = rawLines
            .Select(line => line.Trim().Equals("#[Enter]", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : line)
            .Where(line => !line.TrimStart().StartsWith('#'));

        return string.Join(Environment.NewLine, filteredLines);
    }
}
