internal static partial class Program
{
    static List<Match> ReadOptionalMatchesFromCsv(IReadOnlyList<Player> participants, string prompt)
    {
        while (true)
        {
            Console.WriteLine($"\n{prompt} 入力終了は END 行です。空のまま END で省略できます。\n");

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (line is null)
                {
                    throw new OperationCanceledException("参考対局入力中に入力ストリームが終了しました。");
                }

                if (line.Trim().Equals("END", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                lines.Add(line);
            }

            if (lines.All(string.IsNullOrWhiteSpace))
            {
                return new List<Match>();
            }

            if (TryParseMatches(lines, participants, out var matches, out var errorMessage))
            {
                return matches;
            }

            Console.WriteLine($"参考対局入力の読み取りに失敗しました: {errorMessage}");
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static ExperimentalReportGroupingOptions ReadExperimentalReportGroupingOptions()
    {
        Console.WriteLine("実験レポートの Good / Bad 分離を使いますか？");
        Console.WriteLine("1. Off: 分離しない");
        Console.WriteLine("2. On: Good / Bad フォルダーに分離する\n");

        while (true)
        {
            Console.Write("モード番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return new ExperimentalReportGroupingOptions(false, null, string.Empty);
            }

            if (input == "2")
            {
                Console.WriteLine();
                var outcome = ReadExperimentalReportOutcome();
                var evaluationMemo = ReadOptionalEvaluationMemo();
                return new ExperimentalReportGroupingOptions(true, outcome, evaluationMemo);
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    static string ReadOptionalEvaluationMemo()
    {
        Console.Write("評価メモを1行で入力してください（省略可）: ");
        var input = Console.ReadLine();
        if (input is null)
        {
            throw new OperationCanceledException("評価メモ入力中に入力ストリームが終了しました。");
        }

        Console.WriteLine();
        return input.Trim();
    }

    static ExperimentalReportOutcome ReadExperimentalReportOutcome()
    {
        Console.WriteLine("今回の案の評価を選んでください。");
        Console.WriteLine("1. Good");
        Console.WriteLine("2. Bad\n");

        while (true)
        {
            Console.Write("評価番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return ExperimentalReportOutcome.Good;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return ExperimentalReportOutcome.Bad;
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    static Dictionary<string, FinalStageGroup> ReadFinalStageGroupMap()
    {
        while (true)
        {
            Console.WriteLine("グループ対応CSVを貼り付けてください。入力終了は空行です。\n");

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                Console.WriteLine("CSVが入力されていません。再入力してください。\n");
                continue;
            }

            if (TryParseFinalStageGroups(lines, out var groupMap, out var errorMessage))
            {
                return groupMap;
            }

            Console.WriteLine($"CSVの読み取りに失敗しました: {errorMessage}");
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static List<Player> ReadOptionalPlayersFromCsv(string prompt)
    {
        while (true)
        {
            Console.WriteLine($"{prompt} 入力終了は空行です。空のまま Enter で省略できます。\n");

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                return new List<Player>();
            }

            if (TryParsePlayers(lines, out var participants, out var errorMessage))
            {
                return participants;
            }

            Console.WriteLine($"CSVの読み取りに失敗しました: {errorMessage}");
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static List<Player> ReadPlayersFromCsv()
    {
        while (true)
        {
            Console.WriteLine("選手一覧CSVを貼り付けてください。入力終了は空行です。\n");

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (line is null)
                {
                    throw new OperationCanceledException("選手一覧CSVの入力中に入力ストリームが終了しました。");
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                Console.WriteLine("CSVが入力されていません。再入力してください。\n");
                continue;
            }

            if (TryParsePlayers(lines, out var participants, out var errorMessage))
            {
                return participants;
            }

            Console.WriteLine($"CSVの読み取りに失敗しました: {errorMessage}");
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static List<Match> ReadMatchesFromCsv(IReadOnlyList<Player> participants)
    {
        while (true)
        {
            Console.WriteLine("\n対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。入力終了は END 行です。\n");

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (line is null)
                {
                    throw new OperationCanceledException("対局入力中に入力ストリームが終了しました。");
                }

                if (line.Trim().Equals("END", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                Console.WriteLine("対局入力が入力されていません。再入力してください。\n");
                continue;
            }

            if (TryParseMatches(lines, participants, out var matches, out var errorMessage))
            {
                return matches;
            }

            Console.WriteLine($"対局入力の読み取りに失敗しました: {errorMessage}");
            Console.WriteLine("もう一度入力してください。\n");
        }
    }
}

