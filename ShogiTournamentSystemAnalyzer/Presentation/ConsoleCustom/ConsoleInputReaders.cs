/*
 * ［プレゼンテーション　＞　コンソール改］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;
using System;

internal static class ConsoleInputReaders
{
    internal static List<Match> ReadOptionalMatchesFromCsv(IReadOnlyList<Player> players, string prompt)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("参考対局入力");
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

            if (InputParsers.TryParseMatches(lines, players, out var matches, out var err)) return matches;

            Console.WriteLine($"参考対局入力の読み取りに失敗しました: {err.Value}");
            if (attempt >= ConsolePromptReaders.InputRetryLimit)
            {
                ConsolePromptReaders.ThrowInputRetryLimitExceeded("参考対局入力", err.Value);
            }
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    internal static TournamentQualityEvaluationReportGroupingOptions ReadTournamentQualityEvaluationReportGroupingOptions()
    {
        if (!SimulationTimeBudget.HasApplicationTimeRemaining())
        {
            Console.WriteLine("実験レポートの Good / Bad 分離を使いますか？");
            Console.WriteLine("時間切れのため既定値 Off を採用します。\n");
            return new TournamentQualityEvaluationReportGroupingOptions(false, null, string.Empty);
        }

        Console.WriteLine("実験レポートの Good / Bad 分離を使いますか？");
        Console.WriteLine("1. Off: 分離しない");
        Console.WriteLine("2. On: Good / Bad フォルダーに分離する\n");

        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("実験レポートの Good / Bad 分離");
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

            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("実験レポートの Good / Bad 分離", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    static string ReadOptionalEvaluationMemo()
    {
        SimulationTimeBudget.ThrowIfApplicationTimeExpired("評価メモ入力");
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
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("実験レポート評価");
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

            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("実験レポート評価", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    internal static Dictionary<string, FinalStageGroup> ReadFinalStageGroupMap()
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("グループ対応CSV入力");
            attempt++;
            Console.WriteLine("グループ対応CSVを貼り付けてください。入力終了は空行です。\n");
            ConsoleSamplePrinter.PrintFinalStageGroupCsvExample();

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("グループ対応CSV", "CSVが入力されていません");

                Console.WriteLine("CSVが入力されていません。再入力してください。\n");
                continue;
            }

            if (InputParsers.TryParseFinalStageGroups(lines, out var groupMap, out var err)) return groupMap;

            Console.WriteLine($"CSVの読み取りに失敗しました: {err.Value}");
            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("グループ対応CSV", err.Value);
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    internal static List<Player> ReadOptionalPlayersFromCsv(string prompt)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("選手一覧CSV入力");
            attempt++;
            Console.WriteLine($"{prompt} 入力終了は空行です。空のまま Enter で省略できます。\n");
            ConsoleSamplePrinter.PrintOptionalPlayersCsvExample();

            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                lines.Add(line);
            }

            if (lines.Count == 0) return new List<Player>();

            if (InputParsers.TryParsePlayers(lines, out var players, out var err)) return players;

            Console.WriteLine($"CSVの読み取りに失敗しました: {err.Value}");
            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("選手一覧CSV", err.Value);
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    internal static List<Player> ReadPlayersFromCsv()
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("選手一覧CSV入力");
            attempt++;
            Console.WriteLine("選手一覧CSVを貼り付けてください。入力終了は空行です。\n");
            ConsoleSamplePrinter.PrintPlayersCsvExample();

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
                if (attempt >= ConsolePromptReaders.InputRetryLimit)
                {
                    ConsolePromptReaders.ThrowInputRetryLimitExceeded("選手一覧CSV", "CSVが入力されていません");
                }

                Console.WriteLine("CSVが入力されていません。再入力してください。\n");
                continue;
            }

            if (InputParsers.TryParsePlayers(lines, out var players, out var err)) return players;

            Console.WriteLine($"CSVの読み取りに失敗しました: {err.Value}");
            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("選手一覧CSV", err.Value);
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    internal static List<Match> ReadMatchesFromCsv(IReadOnlyList<Player> players)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("対局入力");
            attempt++;
            Console.WriteLine("\n対局CSVまたは Round/FirstPlayer-SecondPlayer/対局記号表を貼り付けてください。入力終了は END 行です。\n");
            ConsoleSamplePrinter.PrintMatchesCsvExample();
            ConsoleSamplePrinter.PrintRoundMatrixExample();

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
                if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("対局入力", "対局入力が入力されていません");

                Console.WriteLine("対局入力が入力されていません。再入力してください。\n");
                continue;
            }

            if (InputParsers.TryParseMatches(lines, players, out var matches, out var err)) return matches;

            Console.WriteLine($"対局入力の読み取りに失敗しました: {err.Value}");
            if (attempt >= ConsolePromptReaders.InputRetryLimit)
            {
                ConsolePromptReaders.ThrowInputRetryLimitExceeded("対局入力", err.Value);
            }
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    internal static string ReadRequiredFilePath(string prompt)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("ファイルパス入力");
            attempt++;
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine();
                return input;
            }

            if (attempt >= ConsolePromptReaders.InputRetryLimit)
            {
                ConsolePromptReaders.ThrowInputRetryLimitExceeded("ファイルパス入力", "空欄のためファイルパスとして扱えません");
            }

            Console.WriteLine("ファイルパスを入力してください。\n");
        }
    }

    internal static string? ReadOptionalFilePath(string prompt)
    {
        SimulationTimeBudget.ThrowIfApplicationTimeExpired("ファイルパス入力");
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();
        Console.WriteLine();
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    internal static int? ReadOptionalInt(string prompt)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("整数入力");
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

            if (attempt >= ConsolePromptReaders.InputRetryLimit)
            {
                ConsolePromptReaders.ThrowInputRetryLimitExceeded("整数入力", $"'{input}' は整数ではありません");
            }

            Console.WriteLine("整数を入力してください。\n");
        }
    }
}

