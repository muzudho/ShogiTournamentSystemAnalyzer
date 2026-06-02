/*
 * ［アプリケーション　＞　入力　＞　要求ファイル読み取り］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;

internal static class RequestInputFileReader
{
    internal static RequestFileCheckResult Read(string inputFilePath)
    {
        var fullPath = Path.GetFullPath(inputFilePath);

        var rawLines = StsaFileIOHelper.ReadAllLines("入力ファイル", inputFilePath);

        var filteredInput = RequestInputFormatDetector.IsStsaInput3(rawLines)
            ? StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(rawLines, fullPath)
            : RequestInputFormatDetector.IsStsaInput2(rawLines)
                ? StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(rawLines, fullPath)
                : LegacyInputFileFilter.ConvertToFilteredInput(rawLines);

        return new RequestFileCheckResult(fullPath, filteredInput);
    }
}
