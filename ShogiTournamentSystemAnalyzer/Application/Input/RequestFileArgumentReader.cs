/*
 * ［アプリケーション　＞　入力　＞　コマンドライン引数］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class RequestFileArgumentReader
{
    internal static string? TryGetInputFilePath(IReadOnlyList<string> args)
    {
        try
        {
            return TryGetInputFilePathCore(args);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {ex.Message}");
            return null;
        }
    }

    static string? TryGetInputFilePathCore(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            if (arg.Equals("--input-file", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Count) throw new OperationCanceledException("--input-file の後ろにファイルパスを指定してください。");

                return args[index + 1];
            }

            const string inputFilePrefix = "--input-file=";
            if (arg.StartsWith(inputFilePrefix, StringComparison.OrdinalIgnoreCase)) return arg[inputFilePrefix.Length..];
        }

        return null;
    }
}