/*
 * ［アプリケーション　＞　入力　＞　コマンドライン引数］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class RequestFileArgumentReader
{
    internal static RequestFileArgumentReadResult Read(IReadOnlyList<string> args)
    {
        try
        {
            return ReadCore(args);
        }
        catch (OperationCanceledException ex)
        {
            return RequestFileArgumentReadResult.FromError(ex.Message);
        }
    }

    static RequestFileArgumentReadResult ReadCore(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            if (arg.Equals("--input-file", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Count) throw new OperationCanceledException("--input-file の後ろにファイルパスを指定してください。");

                return RequestFileArgumentReadResult.FromInputFile(args[index + 1]);
            }

            const string inputFilePrefix = "--input-file=";
            if (arg.StartsWith(inputFilePrefix, StringComparison.OrdinalIgnoreCase)) return RequestFileArgumentReadResult.FromInputFile(arg[inputFilePrefix.Length..]);
        }

        return RequestFileArgumentReadResult.WithoutInputFile();
    }
}