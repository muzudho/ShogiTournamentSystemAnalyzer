/*
 * ［アプリケーション　＞　実行　＞　要求ファイルチェック］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

using System.Diagnostics.CodeAnalysis;

internal static class RequestFileCheck
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