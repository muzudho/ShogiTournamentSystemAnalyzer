/*
 * ［アプリケーション　＞　要求ファイルチェック　＞　要求入力形式判定］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

internal static class RequestInputFormatDetector
{
    internal static bool IsStsaInput2(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/2", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsStsaInput4(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/4", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsStsaInput5(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/5", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsStsaInput3(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/3", StringComparison.OrdinalIgnoreCase));
    }
}
