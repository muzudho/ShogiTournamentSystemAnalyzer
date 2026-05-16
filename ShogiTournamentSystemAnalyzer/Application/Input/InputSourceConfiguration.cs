internal static partial class Program
{
    static void ConfigureInputSource(IReadOnlyList<string> args)
    {
        var inputFilePath = TryGetInputFilePathFromArgs(args);
        if (!string.IsNullOrWhiteSpace(inputFilePath))
        {
            ApplyInputFile(inputFilePath);
            return;
        }

        Console.WriteLine("入力方法を選んでください。");
        Console.WriteLine("1. そのまま入力する");
        Console.WriteLine("2. 入力ファイルを使う\n");

        while (true)
        {
            Console.Write("入力方法を選んでください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (input is null)
            {
                throw new OperationCanceledException("入力方法の選択中に入力ストリームが終了しました。");
            }

            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return;
            }

            if (input == "2")
            {
                var path = ReadInputFilePath();
                ApplyInputFile(path);
                return;
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    static string ReadInputFilePath()
    {
        while (true)
        {
            Console.Write("入力ファイルのパスを入力してください: ");
            var input = Console.ReadLine()?.Trim();
            if (input is null)
            {
                throw new OperationCanceledException("入力ファイルパスの入力中に入力ストリームが終了しました。");
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("ファイルパスを入力してください。\n");
                continue;
            }

            return input;
        }
    }

    static string? TryGetInputFilePathFromArgs(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            if (arg.Equals("--input-file", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Count)
                {
                    throw new OperationCanceledException("--input-file の後ろにファイルパスを指定してください。");
                }

                return args[index + 1];
            }

            const string inputFilePrefix = "--input-file=";
            if (arg.StartsWith(inputFilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return arg[inputFilePrefix.Length..];
            }
        }

        return null;
    }

    static void ApplyInputFile(string inputFilePath)
    {
        var fullPath = Path.GetFullPath(inputFilePath);
        if (!File.Exists(fullPath))
        {
            throw new OperationCanceledException($"入力ファイルが見つかりません: {fullPath}");
        }

        var filteredLines = File.ReadLines(fullPath)
            .Select(line => line.Trim().Equals("#[Enter]", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : line)
            .Where(line => !line.TrimStart().StartsWith('#'));

        var filteredInput = string.Join(Environment.NewLine, filteredLines);

        Console.SetIn(new StringReader(filteredInput));
        Console.WriteLine($"入力ファイルを使います: {fullPath}\n");
    }
}

