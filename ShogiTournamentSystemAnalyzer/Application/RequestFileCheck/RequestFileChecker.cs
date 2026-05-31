/*
 * ［アプリケーション　＞　要求ファイルチェック　＞　要求ファイル検査］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using System.Diagnostics.CodeAnalysis;

internal static class RequestFileChecker
{
    internal static bool TryRead(
        string inputFilePath,
        Func<string, RequestFileCheckResult> readInputFile,
        [NotNullWhen(true)] out RequestFileCheckResult? result)
    {
        Console.WriteLine("■［要求ファイルチェック］");

        try
        {
            result = readInputFile(inputFilePath);
            Console.WriteLine("要求ファイルチェック: エラー無し\n");
            return true;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            result = null;
            return false;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            result = null;
            return false;
        }
    }
}