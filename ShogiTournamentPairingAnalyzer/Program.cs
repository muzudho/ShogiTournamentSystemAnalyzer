using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("将棋大会の順位分布を計算します。\n");

switch (ReadMode())
{
    case 1:
        RunStandardMode();
        break;
    case 2:
        RunFinalStageMode();
        break;
    default:
        throw new InvalidOperationException("未対応のモードです。");
}

static void RunStandardMode()
{
    Console.WriteLine("通常モード: 総当たり戦の順位分布を計算します。");
    Console.WriteLine("前提: 各対局は黒番・白番を持ち、勝率は Elo レーティング差と黒番有利率から計算します。\n");

    PrintInputSample();
    var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の黒番勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
    var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);

    Console.WriteLine();
    var allParticipants = ReadParticipantsFromCsv();
    var allMatches = ReadMatchesFromCsv(allParticipants);
    var (participants, matches) = FilterToScheduledParticipants(allParticipants, allMatches);

    if (participants.Count != allParticipants.Count)
    {
        Console.WriteLine($"未対局の選手 {allParticipants.Count - participants.Count} 人を結果から除外します。\n");
    }

    PrintMatchesCsv(participants, matches);

    Console.WriteLine($"\n総対局数: {matches.Count}");

    CalculationResult result;
    if (matches.Count <= 20)
    {
        Console.WriteLine("厳密計算を行います。\n");
        result = CalculateExactly(participants, matches, blackAdvantageRating);
    }
    else
    {
        const int defaultSimulationCount = 200_000;
        var simulationCount = ReadIntWithDefault(
            $"局数が多いためシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
            defaultSimulationCount,
            min: 1);

        Console.WriteLine();
        result = CalculateBySimulation(participants, matches, blackAdvantageRating, simulationCount);
    }

    var resultRows = BuildResultRows(participants, matches, result, blackAdvantagePercent);
    PrintResult(participants.Count, result, blackAdvantagePercent, resultRows);

    var defaultOutputCsvPath = Path.GetFullPath($"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
        $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
        defaultOutputCsvPath));
    WriteResultCsv(outputCsvPath, result.Mode, blackAdvantagePercent, resultRows);
    Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
}

static void RunFinalStageMode()
{
    Console.WriteLine("本戦専用モードはこれから実装します。\n");
}

static int ReadMode()
{
    Console.WriteLine("モードを選んでください。");
    Console.WriteLine("1. 通常モード（総当たり戦分析）");
    Console.WriteLine("2. 本戦専用モード（Apex / Innov 定先戦分析）\n");

    while (true)
    {
        Console.Write("モード番号を入力してください [1]: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input) || input == "1")
        {
            Console.WriteLine();
            return 1;
        }

        if (input == "2")
        {
            Console.WriteLine();
            return 2;
        }

        Console.WriteLine("1 か 2 を入力してください。\n");
    }
}

static CalculationResult CalculateExactly(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, double blackAdvantageRating)
{
    var placeProbabilities = new double[participants.Count, participants.Count];
    var wins = new int[participants.Count];

    void Explore(int matchIndex, double scenarioProbability)
    {
        if (matchIndex == matches.Count)
        {
            AccumulatePlaceProbabilities(wins, scenarioProbability, placeProbabilities);
            return;
        }

        var match = matches[matchIndex];
        var blackWinsProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);

        wins[match.Black]++;
        Explore(matchIndex + 1, scenarioProbability * blackWinsProbability);
        wins[match.Black]--;

        wins[match.White]++;
        Explore(matchIndex + 1, scenarioProbability * (1.0 - blackWinsProbability));
        wins[match.White]--;
    }

    Explore(0, 1.0);
    return new CalculationResult(placeProbabilities, "厳密計算", null);
}

static (List<Participant> Participants, List<Match> Matches) FilterToScheduledParticipants(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches)
{
    var activeIndexes = matches
        .SelectMany(match => new[] { match.Black, match.White })
        .Distinct()
        .OrderBy(index => index)
        .ToList();

    var indexMap = activeIndexes
        .Select((oldIndex, newIndex) => new { oldIndex, newIndex })
        .ToDictionary(x => x.oldIndex, x => x.newIndex);

    var filteredParticipants = activeIndexes
        .Select(index => participants[index])
        .ToList();

    var filteredMatches = matches
        .Select(match => new Match(indexMap[match.Black], indexMap[match.White]))
        .ToList();

    return (filteredParticipants, filteredMatches);
}

static CalculationResult CalculateBySimulation(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, double blackAdvantageRating, int simulationCount)
{
    var placeProbabilities = new double[participants.Count, participants.Count];
    var wins = new int[participants.Count];
    var scenarioWeight = 1.0 / simulationCount;

    for (var simulation = 0; simulation < simulationCount; simulation++)
    {
        Array.Clear(wins);

        foreach (var match in matches)
        {
            var blackWinsProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);
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

    return new CalculationResult(placeProbabilities, $"シミュレーション ({simulationCount:N0}回)", simulationCount);
}

static void AccumulatePlaceProbabilities(int[] wins, double scenarioProbability, double[,] placeProbabilities)
{
    var ranking = wins
        .Select((winCount, index) => new ParticipantScore(index, winCount))
        .OrderByDescending(x => x.Wins)
        .ThenBy(x => x.ParticipantIndex)
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
            var participantIndex = ranking[i].ParticipantIndex;
            for (var place = currentPlace; place < groupEnd; place++)
            {
                placeProbabilities[participantIndex, place] += splitProbability;
            }
        }

        currentPlace = groupEnd;
    }
}

static double GetWinProbability(Participant black, Participant white, double blackAdvantageRating)
{
    return 1.0 / (1.0 + Math.Pow(10.0, (white.Rating - (black.Rating + blackAdvantageRating)) / 400.0));
}

static List<ResultRow> BuildResultRows(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, CalculationResult result, double blackAdvantagePercent)
{
    var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
    var blackCounts = new int[participants.Count];
    var whiteCounts = new int[participants.Count];
    var blackWinProbabilitySums = new double[participants.Count];
    var whiteWinProbabilitySums = new double[participants.Count];
    var totalWinProbabilitySums = new double[participants.Count];
    var opponentRatings = Enumerable.Range(0, participants.Count)
        .Select(_ => new List<double>())
        .ToArray();

    foreach (var match in matches)
    {
        var blackWinProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);
        blackCounts[match.Black]++;
        whiteCounts[match.White]++;
        blackWinProbabilitySums[match.Black] += blackWinProbability;
        whiteWinProbabilitySums[match.White] += 1.0 - blackWinProbability;
        totalWinProbabilitySums[match.Black] += blackWinProbability;
        totalWinProbabilitySums[match.White] += 1.0 - blackWinProbability;
        opponentRatings[match.Black].Add(participants[match.White].Rating);
        opponentRatings[match.White].Add(participants[match.Black].Rating);
    }

    var rows = new List<ResultRow>(participants.Count);
    for (var participantIndex = 0; participantIndex < participants.Count; participantIndex++)
    {
        var expectedPlace = Enumerable.Range(0, participants.Count)
            .Sum(place => (place + 1) * result.PlaceProbabilities[participantIndex, place]);
        var blackWinRate = blackCounts[participantIndex] == 0
            ? (double?)null
            : blackWinProbabilitySums[participantIndex] / blackCounts[participantIndex];
        var whiteWinRate = whiteCounts[participantIndex] == 0
            ? (double?)null
            : whiteWinProbabilitySums[participantIndex] / whiteCounts[participantIndex];
        var matchCount = blackCounts[participantIndex] + whiteCounts[participantIndex];
        var totalWinRate = matchCount == 0
            ? 0.0
            : totalWinProbabilitySums[participantIndex] / matchCount;
        var effectiveRating = CalculateEquivalentNeutralRating(opponentRatings[participantIndex], totalWinRate);
        var placeProbabilities = Enumerable.Range(0, participants.Count)
            .Select(place => result.PlaceProbabilities[participantIndex, place])
            .ToArray();
        var placeCounts = result.SimulationCount.HasValue
            ? placeProbabilities.Select(value => value * result.SimulationCount.Value).ToArray()
            : null;

        rows.Add(new ResultRow(
            participants[participantIndex].Name,
            participants[participantIndex].Rating,
            effectiveRating,
            effectiveRating - participants[participantIndex].Rating,
            blackCounts[participantIndex],
            whiteCounts[participantIndex],
            blackWinRate,
            whiteWinRate,
            result.PlaceProbabilities[participantIndex, 0],
            expectedPlace,
            placeProbabilities,
            placeCounts));
    }

    return rows;
}

static void PrintResult(int playerCount, CalculationResult result, double blackAdvantagePercent, IReadOnlyList<ResultRow> resultRows)
{
    Console.WriteLine($"計算方法: {result.Mode}\n");
    Console.WriteLine($"同Elo対局時の黒番勝率: {blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%\n");

    var nameWidth = Math.Max(6, resultRows.Max(x => x.Name.Length) + 2);
    var header = "対局者".PadRight(nameWidth)
        + "元Elo".PadLeft(10)
        + "実効Elo".PadLeft(10)
        + "差分".PadLeft(10)
        + "黒番".PadLeft(8)
        + "白番".PadLeft(8)
        + "黒番勝率".PadLeft(12)
        + "白番勝率".PadLeft(12)
        + "優勝確率".PadLeft(12)
        + "平均順位".PadLeft(12);

    Console.WriteLine(header);
    Console.WriteLine(new string('-', header.Length));

    foreach (var row in resultRows)
    {
        var line = row.Name.PadRight(nameWidth)
            + FormatRating(row.OriginalRating).PadLeft(10)
            + FormatRating(row.EffectiveRating).PadLeft(10)
            + FormatSignedRating(row.RatingDelta).PadLeft(10)
            + row.BlackCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
            + row.WhiteCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
            + FormatOptionalPercent(row.BlackWinRate).PadLeft(12)
            + FormatOptionalPercent(row.WhiteWinRate).PadLeft(12)
            + FormatPercent(row.ChampionshipProbability).PadLeft(12)
            + row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12);

        Console.WriteLine(line);
    }
}

static void WriteResultCsv(string outputCsvPath, string mode, double blackAdvantagePercent, IReadOnlyList<ResultRow> resultRows)
{
    var directoryPath = Path.GetDirectoryName(outputCsvPath);
    if (!string.IsNullOrWhiteSpace(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    var lines = new List<string>();
    var headerColumns = new List<string>
    {
        "calculationMode",
        "blackAdvantagePercent",
        "participantName",
        "originalElo",
        "effectiveElo",
        "eloDelta",
        "blackCount",
        "whiteCount",
        "blackWinRatePercent",
        "whiteWinRatePercent",
        "championshipProbabilityPercent",
        "averagePlace"
    };

    if (resultRows.Count > 0)
    {
        for (var place = 0; place < resultRows[0].PlaceProbabilities.Length; place++)
        {
            headerColumns.Add($"place{place + 1}Percent");
            if (resultRows[0].PlaceCounts is not null)
            {
                headerColumns.Add($"place{place + 1}Count");
            }
        }
    }

    lines.Add(string.Join(",", headerColumns.Select(EscapeCsv)));

    foreach (var row in resultRows)
    {
        var columns = new List<string>
        {
            mode,
            blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture),
            row.Name,
            FormatRating(row.OriginalRating),
            FormatRating(row.EffectiveRating),
            FormatSignedRating(row.RatingDelta),
            row.BlackCount.ToString(CultureInfo.InvariantCulture),
            row.WhiteCount.ToString(CultureInfo.InvariantCulture),
            FormatOptionalPercentValue(row.BlackWinRate),
            FormatOptionalPercentValue(row.WhiteWinRate),
            (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture),
            row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)
        };

        for (var place = 0; place < row.PlaceProbabilities.Length; place++)
        {
            columns.Add((row.PlaceProbabilities[place] * 100).ToString("F2", CultureInfo.InvariantCulture));
            if (row.PlaceCounts is not null)
            {
                columns.Add(row.PlaceCounts[place].ToString("F3", CultureInfo.InvariantCulture));
            }
        }

        lines.Add(string.Join(",", columns.Select(EscapeCsv)));
    }

    File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
}

static List<Participant> ReadParticipantsFromCsv()
{
    while (true)
    {
        Console.WriteLine("選手一覧CSVを貼り付けてください。入力終了は空行です。\n");

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

        if (TryParseParticipants(lines, out var participants, out var errorMessage))
        {
            return participants;
        }

        Console.WriteLine($"CSVの読み取りに失敗しました: {errorMessage}");
        Console.WriteLine("もう一度入力してください。\n");
    }
}

static string ReadTextWithDefault(string prompt, string defaultValue)
{
    Console.Write(prompt);
    var input = Console.ReadLine()?.Trim();
    return string.IsNullOrEmpty(input) ? defaultValue : input;
}

static string ResolveOutputCsvPath(string inputPath)
{
    var fullPath = Path.GetFullPath(inputPath);
    if (Directory.Exists(fullPath))
    {
        return Path.Combine(fullPath, $"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    if (LooksLikeDirectoryPath(inputPath))
    {
        return Path.Combine(fullPath, $"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    return fullPath;
}

static bool LooksLikeDirectoryPath(string path)
{
    return path.EndsWith(Path.DirectorySeparatorChar)
        || path.EndsWith(Path.AltDirectorySeparatorChar)
        || string.IsNullOrEmpty(Path.GetExtension(path));
}

static List<Match> ReadMatchesFromCsv(IReadOnlyList<Participant> participants)
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
                break;
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

static bool TryParseParticipants(IReadOnlyList<string> lines, out List<Participant> participants, out string errorMessage)
{
    participants = new List<Participant>();
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

        if (participants.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = $"{i + 1} 行目の名前 '{name}' は重複しています。";
            return false;
        }

        if (!TryParseDouble(ratingText, out var rating))
        {
            errorMessage = $"{i + 1} 行目の Elo レーティングは数値で入力してください。";
            return false;
        }

        participants.Add(new Participant(name, rating));
    }

    if (participants.Count < 2)
    {
        errorMessage = "選手は 2 人以上必要です。";
        return false;
    }

    return true;
}

static bool TryParseMatches(IReadOnlyList<string> lines, IReadOnlyList<Participant> participants, out List<Match> matches, out string errorMessage)
{
    if (LooksLikeRoundMatrixInput(lines))
    {
        return TryParseMatchesFromRoundMatrix(lines, participants, out matches, out errorMessage);
    }

    matches = new List<Match>();
    errorMessage = string.Empty;

    var participantIndexes = participants
        .Select((participant, index) => new { participant.Name, Index = index })
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

        if (!participantIndexes.TryGetValue(blackName, out var blackIndex))
        {
            errorMessage = $"{i + 1} 行目の黒番 '{blackName}' は選手一覧CSVに存在しません。";
            return false;
        }

        if (!participantIndexes.TryGetValue(whiteName, out var whiteIndex))
        {
            errorMessage = $"{i + 1} 行目の白番 '{whiteName}' は選手一覧CSVに存在しません。";
            return false;
        }

        if (blackIndex == whiteIndex)
        {
            errorMessage = $"{i + 1} 行目で同じ選手が黒番と白番の両方に指定されています。";
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

static bool TryParseMatchesFromRoundMatrix(IReadOnlyList<string> lines, IReadOnlyList<Participant> participants, out List<Match> matches, out string errorMessage)
{
    matches = new List<Match>();
    errorMessage = string.Empty;

    var nonEmptyLines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
    if (nonEmptyLines.Count == 0 || !nonEmptyLines[0].Trim().Equals("Round", StringComparison.OrdinalIgnoreCase))
    {
        errorMessage = "先頭行に Round が必要です。";
        return false;
    }

    var blackWhiteIndex = nonEmptyLines.FindIndex(1, x => x.Trim().Equals("Black/White", StringComparison.OrdinalIgnoreCase));
    if (blackWhiteIndex < 0)
    {
        errorMessage = "Black/White セクションが見つかりません。";
        return false;
    }

    var playersSectionIndex = nonEmptyLines.FindIndex(1, IsPlayerAliasSectionHeader);
    if (playersSectionIndex >= 0 && playersSectionIndex < blackWhiteIndex)
    {
        errorMessage = "対局記号表セクションは Black/White セクションの後ろに置いてください。";
        return false;
    }

    var roundLines = nonEmptyLines.Skip(1).Take(blackWhiteIndex - 1).ToList();
    var blackWhiteLines = playersSectionIndex >= 0
        ? nonEmptyLines.Skip(blackWhiteIndex + 1).Take(playersSectionIndex - blackWhiteIndex - 1).ToList()
        : nonEmptyLines.Skip(blackWhiteIndex + 1).ToList();

    if (!TryParseSquareMatrix(roundLines, "Round", out var roundNames, out var roundValues, out errorMessage))
    {
        return false;
    }

    if (!TryParseSquareMatrix(blackWhiteLines, "Black/White", out var colorNames, out var colorValues, out errorMessage))
    {
        return false;
    }

    if (roundNames.Count != colorNames.Count || !roundNames.SequenceEqual(colorNames, StringComparer.OrdinalIgnoreCase))
    {
        errorMessage = "Round と Black/White の記号並びが一致していません。";
        return false;
    }

    var resolvedNames = roundNames;
    if (playersSectionIndex >= 0)
    {
    var participantAliasLines = nonEmptyLines.Skip(playersSectionIndex + 1).ToList();
    if (!TryParseParticipantAliases(participantAliasLines, roundNames, out resolvedNames, out errorMessage))
        {
            return false;
        }
    }

    var participantIndexes = participants
        .Select((participant, index) => new { participant.Name, Index = index })
        .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

    var orderedMatches = new List<(int Round, Match Match, int Order)>();
    for (var i = 0; i < resolvedNames.Count; i++)
    {
        if (!participantIndexes.ContainsKey(resolvedNames[i]))
        {
            errorMessage = $"選手 '{resolvedNames[i]}' は選手一覧CSVに存在しません。";
            return false;
        }

        for (var j = i + 1; j < resolvedNames.Count; j++)
        {
            var roundForward = NormalizeMatrixCell(roundValues[i, j]);
            var roundBackward = NormalizeMatrixCell(roundValues[j, i]);

            if (string.IsNullOrEmpty(roundForward) && string.IsNullOrEmpty(roundBackward))
            {
                continue;
            }

            if (!string.Equals(roundForward, roundBackward, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"Round 表の '{roundNames[i]}' と '{roundNames[j]}' の値が一致していません。";
                return false;
            }

            if (!int.TryParse(roundForward, NumberStyles.Integer, CultureInfo.InvariantCulture, out var roundNumber) || roundNumber <= 0)
            {
                errorMessage = $"Round 表の '{roundNames[i]}' と '{roundNames[j]}' の値は 1 以上の整数で入力してください。";
                return false;
            }

            var colorForward = NormalizeMatrixCell(colorValues[i, j]).ToLowerInvariant();
            var colorBackward = NormalizeMatrixCell(colorValues[j, i]).ToLowerInvariant();

            Match match;
            if (colorForward == "b" && colorBackward == "w")
            {
                match = new Match(participantIndexes[resolvedNames[i]], participantIndexes[resolvedNames[j]]);
            }
            else if (colorForward == "w" && colorBackward == "b")
            {
                match = new Match(participantIndexes[resolvedNames[j]], participantIndexes[resolvedNames[i]]);
            }
            else
            {
                errorMessage = $"Black/White 表の '{roundNames[i]}' と '{roundNames[j]}' は b/w の組み合わせで入力してください。";
                return false;
            }

            orderedMatches.Add((roundNumber, match, orderedMatches.Count));
        }
    }

    if (orderedMatches.Count == 0)
    {
        errorMessage = "対局は 1 局以上必要です。";
        return false;
    }

    matches = orderedMatches
        .OrderBy(x => x.Round)
        .ThenBy(x => x.Order)
        .Select(x => x.Match)
        .ToList();

    return true;
}

static bool TryParseParticipantAliases(IReadOnlyList<string> lines, IReadOnlyList<string> aliases, out List<string> resolvedNames, out string errorMessage)
{
    resolvedNames = new List<string>();
    errorMessage = string.Empty;

    if (lines.Count == 0)
    {
        errorMessage = "対局記号表セクションの内容がありません。";
        return false;
    }

    var aliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var line in lines)
    {
        var columns = SplitCsvLine(line);
        if (columns.Count < 2)
        {
            errorMessage = "対局記号表セクションは 2 列以上で入力してください。";
            return false;
        }

        var alias = columns[0].Trim();
        var participantName = columns[1].Trim();

        if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(participantName))
        {
            errorMessage = "対局記号表セクションの記号または選手名が空です。";
            return false;
        }

        if (aliasMap.ContainsKey(alias))
        {
            errorMessage = $"対局記号表セクションの記号 '{alias}' が重複しています。";
            return false;
        }

        aliasMap.Add(alias, participantName);
    }

    foreach (var alias in aliases)
    {
        if (!aliasMap.TryGetValue(alias, out var participantName))
        {
            errorMessage = $"対局記号表セクションに記号 '{alias}' の対応表がありません。";
            return false;
        }

        resolvedNames.Add(participantName);
    }

    return true;
}

static bool TryParseDouble(string? input, out double value)
{
    return double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value)
        || double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
}

static bool TryParseSquareMatrix(IReadOnlyList<string> lines, string sectionName, out List<string> names, out string[,] values, out string errorMessage)
{
    names = new List<string>();
    values = new string[0, 0];
    errorMessage = string.Empty;

    if (lines.Count < 2)
    {
        errorMessage = $"{sectionName} セクションの行数が不足しています。";
        return false;
    }

    var headerColumns = SplitCsvLine(lines[0]).Select(x => x.Trim()).ToList();
    if (headerColumns.Count < 2)
    {
        errorMessage = $"{sectionName} セクションのヘッダーが不正です。";
        return false;
    }

    names = headerColumns.Skip(1).ToList();
    if (names.Any(string.IsNullOrWhiteSpace) || names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count)
    {
        errorMessage = $"{sectionName} セクションの見出しが不正です。";
        return false;
    }

    var nameToRowIndex = names
        .Select((name, index) => new { name, index })
        .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

    values = new string[names.Count, names.Count];
    var seenRows = new bool[names.Count];

    for (var lineIndex = 1; lineIndex < lines.Count; lineIndex++)
    {
        var columns = SplitCsvLine(lines[lineIndex]);
        if (columns.Count < names.Count + 1)
        {
            errorMessage = $"{sectionName} セクションの {lineIndex + 1} 行目の列数が不足しています。";
            return false;
        }

        var rowName = columns[0].Trim();
        if (!nameToRowIndex.TryGetValue(rowName, out var rowIndex))
        {
            errorMessage = $"{sectionName} セクションの {lineIndex + 1} 行目の記号 '{rowName}' がヘッダーにありません。";
            return false;
        }

        if (seenRows[rowIndex])
        {
            errorMessage = $"{sectionName} セクションの行 '{rowName}' が重複しています。";
            return false;
        }

        seenRows[rowIndex] = true;
        for (var columnIndex = 0; columnIndex < names.Count; columnIndex++)
        {
            values[rowIndex, columnIndex] = columns[columnIndex + 1].Trim();
        }
    }

    if (seenRows.Any(x => !x))
    {
        errorMessage = $"{sectionName} セクションに不足している行があります。";
        return false;
    }

    return true;
}

static string NormalizeMatrixCell(string? value)
{
    var normalized = value?.Trim() ?? string.Empty;
    return normalized == "-" ? string.Empty : normalized;
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

static bool LooksLikeRoundMatrixInput(IReadOnlyList<string> lines)
{
    var firstNonEmptyLine = lines.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    return firstNonEmptyLine is not null
        && firstNonEmptyLine.Trim().Equals("Round", StringComparison.OrdinalIgnoreCase);
}

static bool IsPlayerAliasSectionHeader(string line)
{
    var header = line.Trim();
    return header.Equals("Players", StringComparison.OrdinalIgnoreCase)
        || header.Equals("対局記号表", StringComparison.OrdinalIgnoreCase);
}

static void PrintInputSample()
{
    Console.WriteLine("入力形式:");
    Console.WriteLine("1. 黒番有利率 (%)");
    Console.WriteLine("2. 選手一覧CSV (1列目=名前, 2列目=Elo レーティング)");
    Console.WriteLine("3. 対局CSV (1列目=黒番, 2列目=白番) または Round/Black-White/対局記号表");
    Console.WriteLine("選手一覧CSVは空行で入力終了、対局入力は END 行で入力終了です。\n");
    Console.WriteLine("入力サンプル:");
    Console.WriteLine("黒番有利率(%): 51\n");
    Console.WriteLine("選手一覧CSV:");
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
    Console.WriteLine("Dave,Carol");
    Console.WriteLine("END\n");
    Console.WriteLine("Round/Black-White 表の例:");
    Console.WriteLine("Round");
    Console.WriteLine(" , A, B, C, D");
    Console.WriteLine("A, -, 3, 2, 1");
    Console.WriteLine("B, 3, -, 1, 2");
    Console.WriteLine("C, 2, 1, -, 3");
    Console.WriteLine("D, 1, 2, 3, -");
    Console.WriteLine();
    Console.WriteLine("Black/White");
    Console.WriteLine(" , A, B, C, D");
    Console.WriteLine("A, -, b, b, b");
    Console.WriteLine("B, w, -, b, b");
    Console.WriteLine("C, w, w, -, b");
    Console.WriteLine("D, w, w, w, -");
    Console.WriteLine();
    Console.WriteLine("対局記号表");
    Console.WriteLine("A, \"Alice\"");
    Console.WriteLine("B, \"Bob\"");
    Console.WriteLine("C, \"Carol\"");
    Console.WriteLine("D, \"Dave\"");
    Console.WriteLine("END\n");
}

static void PrintMatchesCsv(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches)
{
    Console.WriteLine("生成された対局CSV:");
    Console.WriteLine("black,white");

    foreach (var match in matches)
    {
        Console.WriteLine($"{EscapeCsv(participants[match.Black].Name)},{EscapeCsv(participants[match.White].Name)}");
    }

    Console.WriteLine();
}

static string EscapeCsv(string value)
{
    if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
    {
        return value;
    }

    return $"\"{value.Replace("\"", "\"\"")}\"";
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

static string FormatOptionalPercent(double? value)
{
    return value.HasValue ? FormatPercent(value.Value) : "-";
}

static string FormatOptionalPercentValue(double? value)
{
    return value.HasValue
        ? (value.Value * 100).ToString("F2", CultureInfo.InvariantCulture)
        : string.Empty;
}

static double CalculateEquivalentNeutralRating(IReadOnlyList<double> opponentRatings, double targetAverageScore)
{
    if (opponentRatings.Count == 0)
    {
        return 0.0;
    }

    const double epsilon = 1e-9;
    var clampedScore = Math.Clamp(targetAverageScore, epsilon, 1.0 - epsilon);
    var lowerBound = opponentRatings.Min() - 4000.0;
    var upperBound = opponentRatings.Max() + 4000.0;

    for (var i = 0; i < 80; i++)
    {
        var mid = (lowerBound + upperBound) / 2.0;
        var averageScore = opponentRatings.Average(opponentRating => GetNeutralWinProbability(mid, opponentRating));

        if (averageScore < clampedScore)
        {
            lowerBound = mid;
        }
        else
        {
            upperBound = mid;
        }
    }

    return (lowerBound + upperBound) / 2.0;
}

static double GetNeutralWinProbability(double playerRating, double opponentRating)
{
    return 1.0 / (1.0 + Math.Pow(10.0, (opponentRating - playerRating) / 400.0));
}

static string FormatRating(double value)
{
    return Math.Round(value).ToString("F0", CultureInfo.InvariantCulture);
}

static string FormatSignedRating(double value)
{
    return Math.Round(value).ToString("+0;-0;0", CultureInfo.InvariantCulture);
}

readonly record struct Participant(string Name, double Rating);
readonly record struct Match(int Black, int White);
readonly record struct ParticipantScore(int ParticipantIndex, int Wins);
readonly record struct CalculationResult(double[,] PlaceProbabilities, string Mode, int? SimulationCount);
readonly record struct ResultRow(
    string Name,
    double OriginalRating,
    double EffectiveRating,
    double RatingDelta,
    int BlackCount,
    int WhiteCount,
    double? BlackWinRate,
    double? WhiteWinRate,
    double ChampionshipProbability,
    double AveragePlace,
    double[] PlaceProbabilities,
    double[]? PlaceCounts);
