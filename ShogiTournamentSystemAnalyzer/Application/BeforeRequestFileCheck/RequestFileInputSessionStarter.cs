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

        string fullPath;
        string filteredInput;

        try
        {
            fullPath = Path.GetFullPath(inputFilePath);

            var rawLines = StsaFileIOHelper.ReadAllLines("入力ファイル", inputFilePath);

            filteredInput = RequestInputFormatDetector.IsStsaInput3(rawLines)
                ? StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(rawLines, fullPath)
                : RequestInputFormatDetector.IsStsaInput2(rawLines)
                    ? StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(rawLines, fullPath)
                    : LegacyInputFileFilter.ConvertToFilteredInput(rawLines);

            Console.WriteLine("要求ファイルチェック: エラー無し\n");
            Console.SetIn(new StringReader(filteredInput));
            Console.WriteLine($"入力ファイルを使います: {fullPath}\n");

            inputSession = new RequestInputSession(null, null);
            return true;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            inputSession = null;
            return false;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            inputSession = null;
            return false;
        }
    }
}
