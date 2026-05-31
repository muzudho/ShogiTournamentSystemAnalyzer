/*
 * ［アプリケーション　＞　入力　＞　要求ファイル読み取り］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

internal static class RequestInputFileReader
{
    internal static RequestFileCheckResult Read(string inputFilePath)
    {
        var fullPath = Path.GetFullPath(inputFilePath);
        if (!File.Exists(fullPath)) throw new OperationCanceledException($"入力ファイルが見つかりません: {fullPath}");

        var rawLines = File.ReadAllLines(fullPath);
        var filteredInput = RequestInputFormatDetector.IsStsaInput3(rawLines)
            ? StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(rawLines, fullPath)
            : RequestInputFormatDetector.IsStsaInput2(rawLines)
                ? StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(rawLines, fullPath)
                : LegacyInputFileFilter.ConvertToFilteredInput(rawLines);

        return new RequestFileCheckResult(fullPath, filteredInput);
    }
}