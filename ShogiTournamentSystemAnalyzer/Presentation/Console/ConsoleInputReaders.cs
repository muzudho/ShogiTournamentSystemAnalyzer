internal static partial class Program
{
    static List<Match> ReadOptionalMatchesFromCsv(IReadOnlyList<Player> players, string prompt)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.WriteLine($"\n{prompt} 入力終了は END 行です。空のまま END で省略できます。\n");

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (line is null) throw new OperationCanceledException("参考対局入力中に入力ストリームが終了しました。");

                if (line.Trim().Equals("END", StringComparison.OrdinalIgnoreCase)) break;

                lines.Add(line);
            }

            if (lines.All(string.IsNullOrWhiteSpace)) return new List<Match>();

            if (TryParseMatches(lines, players, out var matches, out var err)) return matches;

            Console.WriteLine($"参考対局入力の読み取りに失敗しました: {err.Value}");
            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("参考対局入力", err.Value);
            }
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static TournamentQualityEvaluationReportGroupingOptions ReadTournamentQualityEvaluationReportGroupingOptions()
    {
        Console.WriteLine("実験レポートの Good / Bad 分離を使いますか？");
        Console.WriteLine("1. Off: 分離しない");
        Console.WriteLine("2. On: Good / Bad フォルダーに分離する\n");

        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write("モード番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return new TournamentQualityEvaluationReportGroupingOptions(false, null, string.Empty);
            }

            if (input == "2")
            {
                Console.WriteLine();
                var outcome = ReadTournamentQualityEvaluationReportOutcome();
                var evaluationMemo = ReadOptionalEvaluationMemo();
                return new TournamentQualityEvaluationReportGroupingOptions(true, outcome, evaluationMemo);
            }

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("実験レポートの Good / Bad 分離", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    static string ReadOptionalEvaluationMemo()
    {
        Console.Write("評価メモを1行で入力してください（省略可）: ");
        var input = Console.ReadLine();
        if (input is null) throw new OperationCanceledException("評価メモ入力中に入力ストリームが終了しました。");

        Console.WriteLine();
        return input.Trim();
    }

    static TournamentQualityEvaluationReportOutcome ReadTournamentQualityEvaluationReportOutcome()
    {
        Console.WriteLine("今回の案の評価を選んでください。");
        Console.WriteLine("1. Good");
        Console.WriteLine("2. Bad\n");

        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write("評価番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return TournamentQualityEvaluationReportOutcome.Good;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return TournamentQualityEvaluationReportOutcome.Bad;
            }

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("実験レポート評価", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    static Dictionary<string, FinalStageGroup> ReadFinalStageGroupMap()
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.WriteLine("グループ対応CSVを貼り付けてください。入力終了は空行です。\n");
            PrintFinalStageGroupCsvExample();

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("グループ対応CSV", "CSVが入力されていません");

                Console.WriteLine("CSVが入力されていません。再入力してください。\n");
                continue;
            }

            if (TryParseFinalStageGroups(lines, out var groupMap, out var err)) return groupMap;

            Console.WriteLine($"CSVの読み取りに失敗しました: {err.Value}");
            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("グループ対応CSV", err.Value);
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static List<Player> ReadOptionalPlayersFromCsv(string prompt)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.WriteLine($"{prompt} 入力終了は空行です。空のまま Enter で省略できます。\n");
            PrintOptionalPlayersCsvExample();

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                lines.Add(line);
            }

            if (lines.Count == 0) return new List<Player>();

            if (TryParsePlayers(lines, out var players, out var err)) return players;

            Console.WriteLine($"CSVの読み取りに失敗しました: {err.Value}");
            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("選手一覧CSV", err.Value);
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static List<Player> ReadPlayersFromCsv()
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.WriteLine("選手一覧CSVを貼り付けてください。入力終了は空行です。\n");
            PrintPlayersCsvExample();

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (line is null) throw new OperationCanceledException("選手一覧CSVの入力中に入力ストリームが終了しました。");

                if (string.IsNullOrWhiteSpace(line)) break;

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                if (attempt >= InputRetryLimit)
                {
                    ThrowInputRetryLimitExceeded("選手一覧CSV", "CSVが入力されていません");
                }

                Console.WriteLine("CSVが入力されていません。再入力してください。\n");
                continue;
            }

            if (TryParsePlayers(lines, out var players, out var err)) return players;

            Console.WriteLine($"CSVの読み取りに失敗しました: {err.Value}");
            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("選手一覧CSV", err.Value);
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static List<Match> ReadMatchesFromCsv(IReadOnlyList<Player> players)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.WriteLine("\n対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。入力終了は END 行です。\n");
            PrintMatchesCsvExample();
            PrintRoundMatrixExample();

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (line is null) throw new OperationCanceledException("対局入力中に入力ストリームが終了しました。");

                if (line.Trim().Equals("END", StringComparison.OrdinalIgnoreCase)) break;

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("対局入力", "対局入力が入力されていません");

                Console.WriteLine("対局入力が入力されていません。再入力してください。\n");
                continue;
            }

            if (TryParseMatches(lines, players, out var matches, out var err)) return matches;

            Console.WriteLine($"対局入力の読み取りに失敗しました: {err.Value}");
            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("対局入力", err.Value);
            }
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static string ReadRequiredFilePath(string prompt)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine();
                return input;
            }

            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("ファイルパス入力", "空欄のためファイルパスとして扱えません");
            }

            Console.WriteLine("ファイルパスを入力してください。\n");
        }
    }

    static string? ReadOptionalFilePath(string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();
        Console.WriteLine();
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    static int? ReadOptionalInt(string prompt)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine();
                return null;
            }

            if (int.TryParse(input, out var value))
            {
                Console.WriteLine();
                return value;
            }

            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("整数入力", $"'{input}' は整数ではありません");
            }

            Console.WriteLine("整数を入力してください。\n");
        }
    }
}

