/*
 * ［アプリケーション　＞　要求ファイルチェック前　＞　コマンドライン引数］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

internal static class RequestFileArgumentReader
{
    private const string RequestFileOption = "--request-file";
    private const string InputFileOption = "--input-file";

    internal static RequestFileArgumentReadResult Read(IReadOnlyList<string> args)
    {
        try
        {
            for (var index = 0; index < args.Count; index++)
            {
                var arg = args[index];
                if (arg.Equals(RequestFileOption, StringComparison.OrdinalIgnoreCase)
                    || arg.Equals(InputFileOption, StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Count) throw new OperationCanceledException($"{RequestFileOption} の後ろにファイルパスを指定してください。");

                    return RequestFileArgumentReadResult.FromRequestFile(args[index + 1]);
                }

                const string requestFilePrefix = RequestFileOption + "=";
                if (arg.StartsWith(requestFilePrefix, StringComparison.OrdinalIgnoreCase)) return RequestFileArgumentReadResult.FromRequestFile(arg[requestFilePrefix.Length..]);

                const string inputFilePrefix = InputFileOption + "=";
                if (arg.StartsWith(inputFilePrefix, StringComparison.OrdinalIgnoreCase)) return RequestFileArgumentReadResult.FromRequestFile(arg[inputFilePrefix.Length..]);
            }

            return RequestFileArgumentReadResult.WithoutRequestFile();
        }
        catch (OperationCanceledException ex)
        {
            return RequestFileArgumentReadResult.FromError(ex.Message);
        }
    }
}
