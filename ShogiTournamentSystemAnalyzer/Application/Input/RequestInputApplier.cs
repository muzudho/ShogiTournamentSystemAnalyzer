/*
 * ［アプリケーション　＞　入力　＞　要求入力適用］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class RequestInputApplier
{
    internal static void Apply(RequestFileCheckResult checkedInputFile)
    {
        Console.SetIn(new StringReader(checkedInputFile.FilteredInput));
        Console.WriteLine($"入力ファイルを使います: {checkedInputFile.FullPath}\n");
    }
}