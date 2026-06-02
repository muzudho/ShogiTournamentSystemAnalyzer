/*
 * ［アプリケーション　＞　要求ファイルチェック前　＞　要求ファイル入力セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

using System.Diagnostics.CodeAnalysis;
using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.Shared;

internal static class RequestFileInputSessionStarter
{
    internal static bool TryStart(string inputFilePath, [NotNullWhen(true)] out RequestInputSession? inputSession)
    {
        Console.WriteLine("■［要求ファイルチェック］");

        RequestFileCheckResult? result;
        bool isSuccess;

        try
        {
            result = RequestInputFileReader.Read(inputFilePath);
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
            RequestInputApplier.Apply(result);
            inputSession = new RequestInputSession(null, null);
            return true;
        }

        inputSession = null;
        return false;
    }
}
