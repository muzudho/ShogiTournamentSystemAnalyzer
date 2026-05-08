using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("総当たり戦の順位分布を計算します。");
Console.WriteLine("前提: 各対局は黒番・白番を持ち、勝率は Elo レーティング差と黒番有利率から計算します。\n");

PrintInputSample();
var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の黒番勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);

Console.WriteLine();
var players = ReadPlayersFromCsv();
var matches = ReadMatchesFromCsv(players);

Console.WriteLine($"\n総対局数: {matches.Count}");

CalculationResult result;
if (matches.Count <= 20)
{
    Console.WriteLine("厳密計算を行います。\n");
    result = CalculateExactly(players, matches, blackAdvantageRating);
}
else
{
    const int defaultSimulationCount = 200_000;
    var simulationCount = ReadIntWithDefault(
        $"局数が多いためシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
        defaultSimulationCount,
        min: 1);

    Console.WriteLine();
    result = CalculateBySimulation(players, matches, blackAdvantageRating, simulationCount);
}

PrintResult(players, matches, result, blackAdvantagePercent);

static CalculationResult CalculateExactly(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, double blackAdvantageRating)
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
        var blackWinsProbability = GetWinProbability(players[match.Black], players[match.White], blackAdvantageRating);

        wins[match.Black]++;
        Explore(matchIndex + 1, scenarioProbability * blackWinsProbability);
        wins[match.Black]--;

        wins[match.White]++;
        Explore(matchIndex + 1, scenarioProbability * (1.0 - blackWinsProbability));
        wins[match.White]--;
    }

    Explore(0, 1.0);
    return new CalculationResult(placeProbabilities, "厳密計算");
}

static CalculationResult CalculateBySimulation(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, double blackAdvantageRating, int simulationCount)
{
    var placeProbabilities = new double[players.Count, players.Count];
    var wins = new int[players.Count];
    var scenarioWeight = 1.0 / simulationCount;

    for (var simulation = 0; simulation < simulationCount; simulation++)
    {
        Array.Clear(wins);

        foreach (var match in matches)
        {
            var blackWinsProbability = GetWinProbability(players[match.Black], players[match.White], blackAdvantageRating);
            if (Random.Shared.NextDouble() < blackWinsProbability)
            {
                wins[match.Black]++;
            }
            else
            {
                wins[match.White]++;
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

static double GetWinProbability(Player black, Player white, double blackAdvantageRating)
{
    return 1.0 / (1.0 + Math.Pow(10.0, (white.Rating - (black.Rating + blackAdvantageRating)) / 400.0));
}

static void PrintResult(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, CalculationResult result, double blackAdvantagePercent)
{
    Console.WriteLine($"計算方法: {result.Mode}\n");
    Console.WriteLine($"同Elo対局時の黒番勝率: {blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%\n");

    var blackCounts = new int[players.Count];
    var whiteCounts = new int[players.Count];
    foreach (var match in matches)
    {
        blackCounts[match.Black]++;
        whiteCounts[match.White]++;
    }

    var nameWidth = Math.Max(6, players.Max(x => x.Name.Length) + 2);
    var header = "対局者".PadRight(nameWidth)
        + "黒番".PadLeft(8)
        + "白番".PadLeft(8)
        + "優勝確率".PadLeft(12)
        + "平均順位".PadLeft(12);
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
            + blackCounts[playerIndex].ToString(CultureInfo.InvariantCulture).PadLeft(8)
            + whiteCounts[playerIndex].ToString(CultureInfo.InvariantCulture).PadLeft(8)
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
        Console.WriteLine("プレイヤーCSVを貼り付けてください。入力終了は空行です。\n");

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

static List<Match> ReadMatchesFromCsv(IReadOnlyList<Player> players)
{
    while (true)
    {
        Console.WriteLine("\n対局CSVを貼り付けてください。入力終了は空行です。\n");

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
            Console.WriteLine("対局CSVが入力されていません。再入力してください。\n");
            continue;
        }

        if (TryParseMatches(lines, players, out var matches, out var errorMessage))
        {
            return matches;
        }

        Console.WriteLine($"対局CSVの読み取りに失敗しました: {errorMessage}");
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

static double ReadDoubleWithDefaultInRange(string prompt, double defaultValue, double minInclusive, double maxInclusive)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return defaultValue;
        }

        if (TryParseDouble(input, out var value) && value >= minInclusive && value <= maxInclusive)
        {
            return value;
        }

        Console.WriteLine($"{minInclusive} 以上 {maxInclusive} 以下の数値を入力してください。");
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

        if (players.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = $"{i + 1} 行目の名前 '{name}' は重複しています。";
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

static bool TryParseMatches(IReadOnlyList<string> lines, IReadOnlyList<Player> players, out List<Match> matches, out string errorMessage)
{
    matches = new List<Match>();
    errorMessage = string.Empty;

    var playerIndexes = players
        .Select((player, index) => new { player.Name, Index = index })
        .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

    var startIndex = 0;
    var firstColumns = SplitCsvLine(lines[0]);
    if (IsMatchHeaderRow(firstColumns))
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

        var blackName = columns[0].Trim();
        var whiteName = columns[1].Trim();

        if (string.IsNullOrWhiteSpace(blackName) || string.IsNullOrWhiteSpace(whiteName))
        {
            errorMessage = $"{i + 1} 行目の黒番名または白番名が空です。";
            return false;
        }

        if (!playerIndexes.TryGetValue(blackName, out var blackIndex))
        {
            errorMessage = $"{i + 1} 行目の黒番 '{blackName}' はプレイヤーCSVに存在しません。";
            return false;
        }

        if (!playerIndexes.TryGetValue(whiteName, out var whiteIndex))
        {
            errorMessage = $"{i + 1} 行目の白番 '{whiteName}' はプレイヤーCSVに存在しません。";
            return false;
        }

        if (blackIndex == whiteIndex)
        {
            errorMessage = $"{i + 1} 行目で同じプレイヤーが黒番と白番の両方に指定されています。";
            return false;
        }

        matches.Add(new Match(blackIndex, whiteIndex));
    }

    if (matches.Count == 0)
    {
        errorMessage = "対局は 1 局以上必要です。";
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

static bool IsMatchHeaderRow(IReadOnlyList<string> columns)
{
    if (columns.Count < 2)
    {
        return false;
    }

    var first = columns[0].Trim();
    var second = columns[1].Trim();

    return first.Equals("black", StringComparison.OrdinalIgnoreCase)
        || first.Equals("黒番", StringComparison.OrdinalIgnoreCase)
        || second.Equals("white", StringComparison.OrdinalIgnoreCase)
        || second.Equals("白番", StringComparison.OrdinalIgnoreCase);
}

static void PrintInputSample()
{
    Console.WriteLine("入力形式:");
    Console.WriteLine("1. 黒番有利率 (%)");
    Console.WriteLine("2. プレイヤーCSV (1列目=名前, 2列目=Elo レーティング)");
    Console.WriteLine("3. 対局CSV (1列目=黒番, 2列目=白番)");
    Console.WriteLine("どちらのCSVも 1 行目のヘッダーは省略可能です。\n");
    Console.WriteLine("入力サンプル:");
    Console.WriteLine("黒番有利率(%): 51\n");
    Console.WriteLine("プレイヤーCSV:");
    Console.WriteLine("name,elo");
    Console.WriteLine("Alice,1500");
    Console.WriteLine("Bob,1650");
    Console.WriteLine("Carol,1420");
    Console.WriteLine("Dave,1800\n");
    Console.WriteLine("対局CSV:");
    Console.WriteLine("black,white");
    Console.WriteLine("Alice,Bob");
    Console.WriteLine("Carol,Alice");
    Console.WriteLine("Dave,Alice");
    Console.WriteLine("Bob,Carol");
    Console.WriteLine("Bob,Dave");
    Console.WriteLine("Dave,Carol\n");
}

static double ConvertBlackAdvantagePercentToRating(double blackAdvantagePercent)
{
    const double epsilon = 1e-9;
    var probability = Math.Clamp(blackAdvantagePercent / 100.0, epsilon, 1.0 - epsilon);
    return 400.0 * Math.Log10(probability / (1.0 - probability));
}

static string FormatPercent(double value)
{
    return (value * 100).ToString("F2", CultureInfo.InvariantCulture) + "%";
}

readonly record struct Player(string Name, double Rating);
readonly record struct Match(int Black, int White);
readonly record struct PlayerScore(int PlayerIndex, int Wins);
readonly record struct CalculationResult(double[,] PlaceProbabilities, string Mode);
