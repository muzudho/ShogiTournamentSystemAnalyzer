/*
 * ［アプリケーション　＞　要求ファイルチェック前　＞　要求ファイル入力セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;
using System.Diagnostics.CodeAnalysis;

internal static class RequestFileInputSessionStarter
{
    internal static bool TryStart(string inputFilePath, [NotNullWhen(true)] out RequestInputSession? inputSession)
    {
        Console.WriteLine("■［要求ファイルチェック］");

        RequestFileCheckResult? result;
        bool isSuccess;

        try
        {
            var fullPath = Path.GetFullPath(inputFilePath);

            var rawLines = StsaFileIOHelper.ReadAllLines("入力ファイル", inputFilePath);

            var filteredInput = RequestInputFormatDetector.IsStsaInput3(rawLines)
                ? StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(rawLines, fullPath)
                : RequestInputFormatDetector.IsStsaInput2(rawLines)
                    ? StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(rawLines, fullPath)
                    : LegacyInputFileFilter.ConvertToFilteredInput(rawLines);

            result = new RequestFileCheckResult(fullPath, filteredInput);

            Console.WriteLine("要求ファイルチェック: エラー無し\n");
            isSuccess = true;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            result = null;
            isSuccess = false;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            result = null;
            isSuccess = false;
        }

        if (isSuccess)
        {
            Console.SetIn(new StringReader(result.FilteredInput));
            Console.WriteLine($"入力ファイルを使います: {result.FullPath}\n");

            inputSession = new RequestInputSession(null, null);
            return true;
        }

        inputSession = null;
        return false;
    }
}
