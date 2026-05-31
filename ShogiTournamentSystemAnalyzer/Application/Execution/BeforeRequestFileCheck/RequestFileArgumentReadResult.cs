/*
 * ［アプリケーション　＞　実行　＞　要求ファイルチェック前　＞　要求ファイル引数読み取り結果］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal sealed record RequestFileArgumentReadResult(string? InputFilePath, string? ErrorMessage)
{
    internal bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    internal bool HasInputFile => !string.IsNullOrWhiteSpace(InputFilePath);

    internal static RequestFileArgumentReadResult FromInputFile(string inputFilePath)
    {
        return new RequestFileArgumentReadResult(inputFilePath, null);
    }

    internal static RequestFileArgumentReadResult WithoutInputFile()
    {
        return new RequestFileArgumentReadResult(null, null);
    }

    internal static RequestFileArgumentReadResult FromError(string errorMessage)
    {
        return new RequestFileArgumentReadResult(null, errorMessage);
    }
}