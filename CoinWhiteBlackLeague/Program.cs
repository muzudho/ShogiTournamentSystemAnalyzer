using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("総当たり戦の順位分布を計算します。");
Console.WriteLine("前提: 各組み合わせは1局、勝率は strengthA / (strengthA + strengthB) で計算します。\n");

var playerCount = ReadInt("対局者数を入力してください: ", min: 2);
var players = new List<Player>();

for (var i = 0; i < playerCount; i++)
{
    Console.WriteLine($"\n対局者 {i + 1}");
    var name = ReadRequiredText("名前: ");
    var strength = ReadDouble("強さ (> 0): ", minExclusive: 0);
    players.Add(new Player(name, strength));
}

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
    return first.Strength / (first.Strength + second.Strength);
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

static string ReadRequiredText(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        Console.WriteLine("空では入力できません。");
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

static double ReadDouble(string prompt, double minExclusive)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();

        if (TryParseDouble(input, out var value) && value > minExclusive)
        {
            return value;
        }

        Console.WriteLine($"{minExclusive} より大きい数値を入力してください。");
    }
}

static bool TryParseDouble(string? input, out double value)
{
    return double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value)
        || double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
}

static string FormatPercent(double value)
{
    return (value * 100).ToString("F2", CultureInfo.InvariantCulture) + "%";
}

readonly record struct Player(string Name, double Strength);
readonly record struct Match(int First, int Second);
readonly record struct PlayerScore(int PlayerIndex, int Wins);
readonly record struct CalculationResult(double[,] PlaceProbabilities, string Mode);
