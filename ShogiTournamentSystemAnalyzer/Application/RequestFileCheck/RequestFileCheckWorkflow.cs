namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Domain.Request;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;

internal class RequestFileCheckWorkflow
{
    public static RequestFileCheckResultVer2 Run(RequestFileArgumentReadResult argumentResult)
    {
        // パースする。
        Console.WriteLine("■［要求ファイルチェック］");

        string inputFilePath = argumentResult.InputFilePath!;
        bool isParseError = false;

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
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            isParseError = true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            isParseError = true;
        }

        if (isParseError)
        {
            // ファイルは有ったが、内容のパースエラー等の場合。
            Console.WriteLine($"●エラー終了：　［要求ファイル］パース中エラー。 {argumentResult.ErrorMessage!}");
            return new RequestFileCheckResultVer2(hasError: true, inputSession: null);
        }

        return new RequestFileCheckResultVer2(hasError: false, inputSession: new RequestInputSession(null, null));
    }
}
