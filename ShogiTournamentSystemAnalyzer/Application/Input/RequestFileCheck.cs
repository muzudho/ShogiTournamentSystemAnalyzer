/*
 * ［アプリケーション　＞　入力　＞　要求ファイルチェック］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class RequestFileCheck
{
    internal static bool TryApply(string inputFilePath, Action<string> applyInputFile)
    {
        Console.WriteLine("■［要求ファイルチェック］");

        try
        {
            applyInputFile(inputFilePath);
            Console.WriteLine("要求ファイルチェック: エラー無し\n");
            return true;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            return false;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            return false;
        }
    }
}