/*
 * ［アプリケーション　＞　入力　＞　要求入力形式判定］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class RequestInputFormatDetector
{
    internal static bool IsStsaInput2(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/2", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsStsaInput3(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/3", StringComparison.OrdinalIgnoreCase));
    }
}