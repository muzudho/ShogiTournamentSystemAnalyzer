/*
 * ［アプリケーション　＞　入力　＞　要求ファイル読み取り］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;


internal static class RequestInputFileReader
{
    internal static RequestFileCheckResult Read(string inputFilePath)
    {
        var fullPath = Path.GetFullPath(inputFilePath);
        if (!File.Exists(fullPath)) throw new OperationCanceledException($"入力ファイルが見つかりません: {fullPath}");

        var rawLines = File.ReadAllLines(fullPath);
        var filteredInput = IsStsaInput3(rawLines)
            ? StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(rawLines, fullPath)
            : IsStsaInput2(rawLines)
                ? StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(rawLines, fullPath)
                : ConvertLegacyInputFileToFilteredInput(rawLines);

        return new RequestFileCheckResult(fullPath, filteredInput);
    }

    static bool IsStsaInput2(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/2", StringComparison.OrdinalIgnoreCase));
    }

    static bool IsStsaInput3(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/3", StringComparison.OrdinalIgnoreCase));
    }

    static string ConvertLegacyInputFileToFilteredInput(IEnumerable<string> rawLines)
    {
        var filteredLines = rawLines
            .Select(line => line.Trim().Equals("#[Enter]", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : line)
            .Where(line => !line.TrimStart().StartsWith('#'));

        return string.Join(Environment.NewLine, filteredLines);
    }

}