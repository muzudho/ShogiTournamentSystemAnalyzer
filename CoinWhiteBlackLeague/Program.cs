using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("総当たり戦の順位分布を計算します。");
Console.WriteLine("前提: 各組み合わせは1局、勝率は Elo レーティング差から計算します。\n");

PrintCsvSample();
var players = ReadPlayersFromCsv();
var playerCount = players.Count;

var matches = BuildMatches(playerCount);
Console.WriteLine($"\n総対局数: {matches.Count}");

CalculationResult result;
if (matches.Count <= 20)
{
    Console.WriteLine("厳密計算を行います。\n");
    result = CalculateExactly(players, matches);
}
else
{
    const int defaultSimulationCount = 200_000;
    var simulationCount = ReadIntWithDefault(
        $"局数が多いためシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
        defaultSimulationCount,
        min: 1);

    Console.WriteLine();
    result = CalculateBySimulation(players, matches, simulationCount);
}

PrintResult(players, result);

static List<Match> BuildMatches(int playerCount)
{
    var matches = new List<Match>();

    for (var i = 0; i < playerCount; i++)
    {
        for (var j = i + 1; j < playerCount; j++)
        {
            matches.Add(new Match(i, j));
        }
    }

    return matches;
}

static CalculationResult CalculateExactly(IReadOnlyList<Player> players, IReadOnlyList<Match> matches)
{
    var placeProbabilities = new double[players.Count, players.Count];
    var wins = new int[players.Count];

    void Explore(int matchIndex, double scenarioProbability)
    {
        if (matchIndex == matches.Count)
        {
            AccumulatePlaceProbabilities(wins, scenarioProbability, placeProbabilities);
            return;
        }

        var match = matches[matchIndex];
        var firstWinsProbability = GetWinProbability(players[match.First], players[match.Second]);

        wins[match.First]++;
        Explore(matchIndex + 1, scenarioProbability * firstWinsProbability);
        wins[match.First]--;

        wins[match.Second]++;
        Explore(matchIndex + 1, scenarioProbability * (1.0 - firstWinsProbability));
        wins[match.Second]--;
    }

    Explore(0, 1.0);
    return new CalculationResult(placeProbabilities, "厳密計算");
}

static CalculationResult CalculateBySimulation(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, int simulationCount)
{
    var placeProbabilities = new double[players.Count, players.Count];
    var wins = new int[players.Count];
    var scenarioWeight = 1.0 / simulationCount;

    for (var simulation = 0; simulation < simulationCount; simulation++)
    {
        Array.Clear(wins);

        foreach (var match in matches)
        {
            var firstWinsProbability = GetWinProbability(players[match.First], players[match.Second]);
            if (Random.Shared.NextDouble() < firstWinsProbability)
            {
                wins[match.First]++;
            }
            else
            {
                wins[match.Second]++;
            }
        }

        AccumulatePlaceProbabilities(wins, scenarioWeight, placeProbabilities);
    }

    return new CalculationResult(placeProbabilities, $"シミュレーション ({simulationCount:N0}回)");
}

static void AccumulatePlaceProbabilities(int[] wins, double scenarioProbability, double[,] placeProbabilities)
{
    var ranking = wins
        .Select((winCount, index) => new PlayerScore(index, winCount))
        .OrderByDescending(x => x.Wins)
        .ThenBy(x => x.PlayerIndex)
        .ToArray();

    var currentPlace = 0;
    while (currentPlace < ranking.Length)
    {
        var groupEnd = currentPlace + 1;
        while (groupEnd < ranking.Length && ranking[groupEnd].Wins == ranking[currentPlace].Wins)
        {
            groupEnd++;
        }

        var groupSize = groupEnd - currentPlace;
        var splitProbability = scenarioProbability / groupSize;

        for (var i = currentPlace; i < groupEnd; i++)
        {
            var playerIndex = ranking[i].PlayerIndex;
            for (var place = currentPlace; place < groupEnd; place++)
            {
                placeProbabilities[playerIndex, place] += splitProbability;
            }
        }

        currentPlace = groupEnd;
    }
}

static double GetWinProbability(Player first, Player second)
{
    return 1.0 / (1.0 + Math.Pow(10.0, (second.Rating - first.Rating) / 400.0));
}

static void PrintResult(IReadOnlyList<Player> players, CalculationResult result)
{
    Console.WriteLine($"計算方法: {result.Mode}\n");

    var nameWidth = Math.Max(6, players.Max(x => x.Name.Length) + 2);
    var header = "対局者".PadRight(nameWidth) + "優勝確率".PadLeft(12) + "平均順位".PadLeft(12);
    for (var place = 0; place < players.Count; place++)
    {
        header += $"{(place + 1).ToString(CultureInfo.InvariantCulture) + "位",10}";
    }

    Console.WriteLine(header);
    Console.WriteLine(new string('-', header.Length));

    for (var playerIndex = 0; playerIndex < players.Count; playerIndex++)
    {
        var expectedPlace = Enumerable.Range(0, players.Count)
            .Sum(place => (place + 1) * result.PlaceProbabilities[playerIndex, place]);

        var line = players[playerIndex].Name.PadRight(nameWidth)
            + FormatPercent(result.PlaceProbabilities[playerIndex, 0]).PadLeft(12)
            + expectedPlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12);

        for (var place = 0; place < players.Count; place++)
        {
            line += FormatPercent(result.PlaceProbabilities[playerIndex, place]).PadLeft(10);
        }

        Console.WriteLine(line);
    }
}

static List<Player> ReadPlayersFromCsv()
{
    while (true)
    {
        Console.WriteLine("CSVを貼り付けてください。入力終了は空行です。\n");

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

        if (TryParsePlayers(lines, out var players, out var errorMessage))
        {
            return players;
        }

        Console.WriteLine($"CSVの読み取りに失敗しました: {errorMessage}");
        Console.WriteLine("もう一度入力してください。\n");
    }
}

static int ReadInt(string prompt, int min)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (int.TryParse(input, out var value) && value >= min)
        {
            return value;
        }

        Console.WriteLine($"{min} 以上の整数を入力してください。");
    }
}

static int ReadIntWithDefault(string prompt, int defaultValue, int min)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return defaultValue;
        }

        if (int.TryParse(input, out var value) && value >= min)
        {
            return value;
        }

        Console.WriteLine($"{min} 以上の整数を入力してください。");
    }
}

static bool TryParsePlayers(IReadOnlyList<string> lines, out List<Player> players, out string errorMessage)
{
    players = new List<Player>();
    errorMessage = string.Empty;

    var startIndex = 0;
    var firstColumns = SplitCsvLine(lines[0]);
    if (IsHeaderRow(firstColumns))
    {
        startIndex = 1;
    }

    for (var i = startIndex; i < lines.Count; i++)
    {
        var columns = SplitCsvLine(lines[i]);
        if (columns.Count < 2)
        {
            errorMessage = $"{i + 1} 行目は 2 列以上必要です。";
            return false;
        }

        var name = columns[0].Trim();
        var ratingText = columns[1].Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            errorMessage = $"{i + 1} 行目の名前が空です。";
            return false;
        }

        if (!TryParseDouble(ratingText, out var rating))
        {
            errorMessage = $"{i + 1} 行目の Elo レーティングは数値で入力してください。";
            return false;
        }

        players.Add(new Player(name, rating));
    }

    if (players.Count < 2)
    {
        errorMessage = "対局者は 2 人以上必要です。";
        return false;
    }

    return true;
}

static bool TryParseDouble(string? input, out double value)
{
    return double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value)
        || double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
}

static List<string> SplitCsvLine(string line)
{
    var columns = new List<string>();
    var field = new StringBuilder();
    var inQuotes = false;

    for (var i = 0; i < line.Length; i++)
    {
        var ch = line[i];

        if (ch == '"')
        {
            if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
            {
                field.Append('"');
                i++;
            }
            else
            {
                inQuotes = !inQuotes;
            }

            continue;
        }

        if (ch == ',' && !inQuotes)
        {
            columns.Add(field.ToString());
            field.Clear();
            continue;
        }

        field.Append(ch);
    }

    columns.Add(field.ToString());
    return columns;
}

static bool IsHeaderRow(IReadOnlyList<string> columns)
{
    if (columns.Count < 2)
    {
        return false;
    }

    var first = columns[0].Trim();
    var second = columns[1].Trim();

    return first.Equals("name", StringComparison.OrdinalIgnoreCase)
        || first.Equals("名前", StringComparison.OrdinalIgnoreCase)
        || second.Equals("elo", StringComparison.OrdinalIgnoreCase)
        || second.Equals("rating", StringComparison.OrdinalIgnoreCase)
        || second.Equals("eloRating", StringComparison.OrdinalIgnoreCase)
        || second.Equals("eloレーティング", StringComparison.OrdinalIgnoreCase)
        || second.Equals("レーティング", StringComparison.OrdinalIgnoreCase);
}

static void PrintCsvSample()
{
    Console.WriteLine("入力形式: CSV");
    Console.WriteLine("1列目=名前, 2列目=Elo レーティング");
    Console.WriteLine("1行目のヘッダーは省略可能です。\n");
    Console.WriteLine("入力サンプル:");
    Console.WriteLine("name,elo");
    Console.WriteLine("Alice,1500");
    Console.WriteLine("Bob,1650");
    Console.WriteLine("Carol,1420");
    Console.WriteLine("Dave,1800\n");
}

static string FormatPercent(double value)
{
    return (value * 100).ToString("F2", CultureInfo.InvariantCulture) + "%";
}

readonly record struct Player(string Name, double Rating);
readonly record struct Match(int First, int Second);
readonly record struct PlayerScore(int PlayerIndex, int Wins);
readonly record struct CalculationResult(double[,] PlaceProbabilities, string Mode);
