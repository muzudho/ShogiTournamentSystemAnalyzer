/*
 * ［アプリケーション　＞　要求ファイル書出　＞　parse failure smoke］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal static class RequestParseFailureSmoke
{
    const string Option = "--smoke-request-parse-fails";
    const string ContainsOption = "--contains";

    internal static bool TryRun(IReadOnlyList<string> args)
    {
        var optionIndex = IndexOf(args, Option);
        if (optionIndex < 0) return false;

        string? expectedMessagePart = null;
        var requestFilePaths = new List<string>();

        for (var index = optionIndex + 1; index < args.Count; index++)
        {
            var arg = args[index];
            if (arg.Equals(ContainsOption, StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Count) throw new OperationCanceledException($"{ContainsOption} の後ろに期待するエラーメッセージ片を指定してください。");

                expectedMessagePart = args[++index];
                continue;
            }

            requestFilePaths.Add(arg);
        }

        if (requestFilePaths.Count == 0) throw new OperationCanceledException($"{Option} の後ろに要求ファイルパスを1つ以上指定してください。");

        foreach (var requestFilePath in requestFilePaths)
        {
            RunOne(requestFilePath, expectedMessagePart);
        }

        return true;
    }

    static void RunOne(string requestFilePath, string? expectedMessagePart)
    {
        var fullPath = Path.GetFullPath(requestFilePath);
        var rawLines = File.ReadAllLines(fullPath);
        var sourceFormatName = rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/5", StringComparison.OrdinalIgnoreCase))
            ? "STSAInput/5"
            : "STSAInput/4";
        var requestText = new RequestText(sourceFormatName, rawLines, fullPath);

        try
        {
            if (!StsaInputRequestParser.TryParse(requestText, out var parsedRequest) || parsedRequest is null)
            {
                throw new OperationCanceledException($"{sourceFormatName} として解析できません: {fullPath}");
            }
        }
        catch (OperationCanceledException ex)
        {
            if (!string.IsNullOrWhiteSpace(expectedMessagePart)
                && !ex.Message.Contains(expectedMessagePart, StringComparison.Ordinal))
            {
                throw new OperationCanceledException($"期待したエラーメッセージ片が見つかりません。期待: {expectedMessagePart} / 実際: {ex.Message}");
            }

            Console.WriteLine($"Request parse failure smoke OK: {fullPath} ({ex.Message})");
            return;
        }

        throw new OperationCanceledException($"要求ファイルの解析が失敗する想定でしたが成功しました: {fullPath}");
    }

    static int IndexOf(IReadOnlyList<string> args, string option)
    {
        for (var index = 0; index < args.Count; index++)
        {
            if (args[index].Equals(option, StringComparison.OrdinalIgnoreCase)) return index;
        }

        return -1;
    }
}