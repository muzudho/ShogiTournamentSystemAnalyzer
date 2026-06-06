namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;

internal class RequestFileCheckWorkflow
{
    public static RequestFileCheckResult Run(RequestFileArgumentReadResult argumentResult)
    {
        // パースする。
        Console.WriteLine("■［要求ファイルチェック］");

        string requestFilePath = argumentResult.RequestFilePath!;
        bool isParseError = false;
        string? parseErrorMessage = null;

        string fullPath;
        RequestText? requestText = null;

        try
        {
            fullPath = Path.GetFullPath(requestFilePath);
            if (IsLegacyInputPath(fullPath))
            {
                throw new OperationCanceledException($"Legacy 入力は実行対象外です。STSAInput/2、STSAInput/3、STSAInput/4、STSAInput/5 へ更新してください: {fullPath}");
            }

            var rawLines = StsaFileIOHelper.ReadAllLines("要求ファイル", requestFilePath);
            var formatName = RequestInputFormatDetector.IsStsaInput5(rawLines)
                ? "STSAInput/5"
                : RequestInputFormatDetector.IsStsaInput4(rawLines)
                    ? "STSAInput/4"
                    : RequestInputFormatDetector.IsStsaInput3(rawLines)
                        ? "STSAInput/3"
                        : RequestInputFormatDetector.IsStsaInput2(rawLines)
                            ? "STSAInput/2"
                            : "Legacy";
            requestText = new RequestText(formatName, rawLines, fullPath);


            Console.WriteLine("要求ファイルチェック: エラー無し\n");
            Console.WriteLine($"要求ファイルを使います: {fullPath}\n");
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
            return new RequestFileCheckResult(false, requestText);
        }

        return new RequestFileCheckResult(true, requestText);
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
