/*
 * ［アプリケーション　＞　要求ファイル書出　＞　round-trip smoke］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal static class RequestWriterRoundTripSmoke
{
    const string Option = "--smoke-request-writer-roundtrip";
    const string OutputDirectoryOption = "--output-dir";

    internal static bool TryRun(IReadOnlyList<string> args)
    {
        var optionIndex = IndexOf(args, Option);
        if (optionIndex < 0) return false;

        var outputDirectory = "Output/SmokeGenerated";
        var requestFilePaths = new List<string>();

        for (var index = optionIndex + 1; index < args.Count; index++)
        {
            var arg = args[index];
            if (arg.Equals(OutputDirectoryOption, StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Count) throw new OperationCanceledException($"{OutputDirectoryOption} の後ろに出力フォルダーを指定してください。");

                outputDirectory = args[++index];
                continue;
            }

            requestFilePaths.Add(arg);
        }

        if (requestFilePaths.Count == 0) throw new OperationCanceledException($"{Option} の後ろに要求ファイルパスを1つ以上指定してください。");

        Directory.CreateDirectory(outputDirectory);
        foreach (var requestFilePath in requestFilePaths)
        {
            RunOne(requestFilePath, outputDirectory);
        }

        return true;
    }

    static void RunOne(string requestFilePath, string outputDirectory)
    {
        var fullPath = Path.GetFullPath(requestFilePath);
        var rawLines = File.ReadAllLines(fullPath);
        var requestText = new RequestText("STSAInput/4", rawLines, fullPath);
        if (!StsaInput4RequestParser.TryParse(requestText, out var parsedRequest) || parsedRequest is null)
        {
            throw new OperationCanceledException($"STSAInput/4 として解析できません: {fullPath}");
        }

        var generatedLines = StsaInput4RequestWriter.BuildLines(parsedRequest);
        var generatedPath = Path.Combine(outputDirectory, Path.GetFileName(requestFilePath));
        File.WriteAllLines(generatedPath, generatedLines);

        var generatedFullPath = Path.GetFullPath(generatedPath);
        var generatedRequestText = new RequestText("STSAInput/4", File.ReadAllLines(generatedFullPath), generatedFullPath);
        if (!StsaInput4RequestParser.TryParse(generatedRequestText, out var roundTrippedRequest) || roundTrippedRequest is null)
        {
            throw new OperationCanceledException($"Writer 出力を STSAInput/4 として再解析できません: {generatedFullPath}");
        }

        Console.WriteLine($"Writer round-trip smoke OK: {fullPath} -> {generatedFullPath} ({generatedLines.Count} lines)");
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
