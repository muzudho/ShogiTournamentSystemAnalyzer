/*
 * ［アプリケーション　＞　要求ファイルチェック前　＞　要求ファイル引数読み取り結果］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

internal sealed record RequestFileArgumentReadResult(string? RequestFilePath, string? ErrorMessage)
{
    internal bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    internal bool HasRequestFile => !string.IsNullOrWhiteSpace(RequestFilePath);

    internal static RequestFileArgumentReadResult FromRequestFile(string requestFilePath)
    {
        return new RequestFileArgumentReadResult(requestFilePath, null);
    }

    internal static RequestFileArgumentReadResult WithoutRequestFile()
    {
        return new RequestFileArgumentReadResult(null, null);
    }

    internal static RequestFileArgumentReadResult FromError(string errorMessage)
    {
        return new RequestFileArgumentReadResult(null, errorMessage);
    }
}