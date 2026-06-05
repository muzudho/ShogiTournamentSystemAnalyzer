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
        string? parseErrorMessage = null;

        string fullPath;
        string filteredInput = string.Empty;

        try
        {
            fullPath = Path.GetFullPath(inputFilePath);
            if (IsLegacyInputPath(fullPath))
            {
                throw new OperationCanceledException($"Legacy 入力は実行対象外です。STSAInput/2、STSAInput/3、STSAInput/4 へ更新してください: {fullPath}");
            }

            var rawLines = StsaFileIOHelper.ReadAllLines("入力ファイル", inputFilePath);

            // ファイルの形式を判定して、必要に応じてレガシー形式に変換する。
            filteredInput = RequestInputFormatDetector.IsStsaInput4(rawLines)
                ? StsaInputLegacyConverter.ConvertStsaInput4ToLegacyInput(rawLines, fullPath)
                : RequestInputFormatDetector.IsStsaInput3(rawLines)
                    ? StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(rawLines, fullPath)
                    : RequestInputFormatDetector.IsStsaInput2(rawLines)
                        ? StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(rawLines, fullPath)
                        : LegacyInputFileFilter.ConvertToFilteredInput(rawLines);

            Console.WriteLine("要求ファイルチェック: エラー無し\n");
            Console.WriteLine($"入力ファイルを使います: {fullPath}\n");
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            parseErrorMessage = ex.Message;
            isParseError = true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            parseErrorMessage = ex.Message;
            isParseError = true;
        }

        if (isParseError)
        {
            // ファイルは有ったが、内容のパースエラー等の場合。
            Console.WriteLine($"●エラー終了：　［要求ファイル］パース中エラー。 {parseErrorMessage}");
            return new RequestFileCheckResultVer2(hasError: true, inputSession: null);
        }

        return new RequestFileCheckResultVer2(
            hasError: false,
            inputSession: new RequestInputSession(filteredInput, string.Empty, Array.Empty<string>()));
    }

    static bool IsLegacyInputPath(string fullPath)
    {
        var legacyRoot = Path.GetFullPath(Path.Combine("Inputs", "Legacy"));
        var relativePath = Path.GetRelativePath(legacyRoot, fullPath);
        return relativePath != "."
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }
}
