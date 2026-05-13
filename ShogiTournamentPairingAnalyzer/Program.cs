using System.Globalization;
using System.Text;

internal static partial class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("将棋大会の順位分布を計算します。\n");

        try
        {
            RunApp();
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }

static List<Match> ReadOptionalMatchesFromCsv(IReadOnlyList<Participant> participants, string prompt)
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

    static void RunApp()
    {
        switch (ReadMode())
        {
            case 1:
                RunStandardMode();
                break;
            case 2:
                RunFinalStageMode();
                break;
            case 3:
                RunQualityEvaluationMode();
                break;
            default:
                throw new InvalidOperationException("未対応のモードです。");
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

static string BuildQualitySummaryDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, ExperimentalReportGroupingOptions options)
{
    var fileName = $"quality_summary_{placementMode}_{boundaryRescueMode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
    if (!options.IsEnabled)
    {
        return Path.GetFullPath(fileName);
    }

    var outcomeFolderName = options.Outcome == ExperimentalReportOutcome.Bad ? "Bad" : "Good";
    var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
    Directory.CreateDirectory(baseDirectory);
    return Path.Combine(baseDirectory, fileName);
}

static int ReadMode()
{
    Console.WriteLine("モードを選んでください。");
    Console.WriteLine("1. 通常モード（総当たり戦分析）");
    Console.WriteLine("2. 本戦専用モード（Apex / Innov 定先戦分析）");
    Console.WriteLine("3. 品質評価モード（本戦ルールの実力反映性評価）\n");

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

        if (input == "3")
        {
            Console.WriteLine();
            return 3;
        }

        Console.WriteLine("1、2、3 のいずれかを入力してください。\n");
    }
}

static void PrintFinalStageInputSample()
{
    Console.WriteLine("本戦専用モードの入力形式:");
    Console.WriteLine("1. 選手一覧CSV");
    Console.WriteLine("2. グループ対応CSV");
    Console.WriteLine("3. 本戦不出場Apex一覧CSV（省略可）");
    Console.WriteLine("4. 対局CSV または Round/Black-White/対局記号表\n");
    Console.WriteLine("選手一覧CSVの例:");
    Console.WriteLine("name,elo");
    Console.WriteLine("Alice,5000");
    Console.WriteLine("Bob,4980");
    Console.WriteLine("Carol,4960");
    Console.WriteLine("Dave,4940\n");
    Console.WriteLine("グループ対応CSVの例:");
    Console.WriteLine("group,name");
    Console.WriteLine("Apex,Alice");
    Console.WriteLine("Apex,Bob");
    Console.WriteLine("Innov,Carol");
    Console.WriteLine("Innov,Dave\n");
    Console.WriteLine("本戦不出場Apex一覧CSVの例（省略可）:");
    Console.WriteLine("name,elo");
    Console.WriteLine("Eve,4920");
    Console.WriteLine("Frank,4900\n");
    Console.WriteLine("対局CSVの例:");
    Console.WriteLine("black,white");
    Console.WriteLine("Carol,Alice");
    Console.WriteLine("Dave,Bob");
    Console.WriteLine("END\n");
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

static CalculationResult CalculateFinalStageExactly(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double blackAdvantageRating, int promotedInnovCount = 0)
{
    var placeProbabilities = new double[participants.Count, participants.Count + additionalApexCount];
    var wins = new int[participants.Count];
    var apexParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Apex);
    var innovParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Innov);

    void Explore(int matchIndex, double scenarioProbability)
    {
        if (matchIndex == matches.Count)
        {
            AccumulateFinalStagePlaceProbabilities(wins, participants, apexParticipantIndexes, innovParticipantIndexes, additionalApexCount, boundaryRescueMode, blackAdvantageRating, scenarioProbability, placeProbabilities, promotedInnovCount);
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
    return new CalculationResult(placeProbabilities, "本戦専用 厳密計算", null);
}

static CalculationResult CalculateFinalStageBySimulation(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double blackAdvantageRating, int simulationCount, int promotedInnovCount = 0)
{
    var placeProbabilities = new double[participants.Count, participants.Count + additionalApexCount];
    var wins = new int[participants.Count];
    var scenarioWeight = 1.0 / simulationCount;
    var apexParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Apex);
    var innovParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Innov);

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

        AccumulateFinalStagePlaceProbabilities(wins, participants, apexParticipantIndexes, innovParticipantIndexes, additionalApexCount, boundaryRescueMode, blackAdvantageRating, scenarioWeight, placeProbabilities, promotedInnovCount);
    }

    return new CalculationResult(placeProbabilities, $"本戦専用 シミュレーション ({simulationCount:N0}回)", simulationCount);
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

static List<Participant> ReadOptionalParticipantsFromCsv(string prompt)
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
            return new List<Participant>();
        }

        if (TryParseParticipants(lines, out var participants, out var errorMessage))
        {
            return participants;
        }

        Console.WriteLine($"CSVの読み取りに失敗しました: {errorMessage}");
        Console.WriteLine("もう一度入力してください。\n");
    }
}

static bool ValidateFinalStageParticipants(IReadOnlyList<Participant> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, out string errorMessage)
{
    errorMessage = string.Empty;

    if (participants.Count != 16)
    {
        errorMessage = $"本戦参加者は 16 名で入力してください。現在は {participants.Count} 名です。";
        return false;
    }

    if (groupMap.Count != participants.Count)
    {
        errorMessage = $"グループ対応CSVの人数が一致していません。選手一覧CSVは {participants.Count} 名、グループ対応CSVは {groupMap.Count} 名です。";
        return false;
    }

    foreach (var participant in participants)
    {
        if (!groupMap.ContainsKey(participant.Name))
        {
            errorMessage = $"選手 '{participant.Name}' のグループが指定されていません。";
            return false;
        }
    }

    var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
    if (apexCount > 8)
    {
        errorMessage = $"Apex は 8 名以下で入力してください。現在は {apexCount} 名です。";
        return false;
    }

    return true;
}

static bool ValidateAdditionalApexParticipants(IReadOnlyList<Participant> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, IReadOnlyList<Participant> additionalApexParticipants, out string errorMessage)
{
    errorMessage = string.Empty;

    var knownNames = new HashSet<string>(participants.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
    foreach (var participant in additionalApexParticipants)
    {
        if (knownNames.Contains(participant.Name))
        {
            errorMessage = $"本戦不出場Apex一覧の選手 '{participant.Name}' は本戦参加者と重複しています。";
            return false;
        }

        if (groupMap.ContainsKey(participant.Name))
        {
            errorMessage = $"本戦不出場Apex一覧の選手 '{participant.Name}' はグループ対応CSVにも含まれています。";
            return false;
        }
    }

    return true;
}

static List<int> GetParticipantIndexesByGroup(IReadOnlyList<Participant> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, FinalStageGroup targetGroup)
{
    return participants
        .Select((participant, index) => new { participant.Name, Index = index })
        .Where(x => groupMap[x.Name] == targetGroup)
        .Select(x => x.Index)
        .ToList();
}

static void AccumulateFinalStagePlaceProbabilities(int[] wins, IReadOnlyList<Participant> participants, IReadOnlyList<int> apexParticipantIndexes, IReadOnlyList<int> innovParticipantIndexes, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double blackAdvantageRating, double scenarioProbability, double[,] placeProbabilities, int promotedInnovCount = 0)
{
    if (promotedInnovCount > 0)
    {
        AccumulateVariableTop8PlaceProbabilities(wins, apexParticipantIndexes, innovParticipantIndexes, additionalApexCount, promotedInnovCount, scenarioProbability, placeProbabilities);
        return;
    }

    if (boundaryRescueMode == BoundaryRescueMode.Off)
    {
        AccumulateGroupPlaceProbabilities(wins, apexParticipantIndexes, 0, scenarioProbability, placeProbabilities);
        AccumulateGroupPlaceProbabilities(wins, innovParticipantIndexes, apexParticipantIndexes.Count + additionalApexCount, scenarioProbability, placeProbabilities);
        return;
    }

    var apexRanking = BuildRankingForParticipantIndexes(wins, apexParticipantIndexes);
    var innovRanking = BuildRankingForParticipantIndexes(wins, innovParticipantIndexes);

    var apexBoundaryIndexes = GetTiedParticipantIndexesAtPosition(apexRanking, apexRanking.Length - 1);
    var innovBoundaryIndexes = GetTiedParticipantIndexesAtPosition(innovRanking, 0);

    var rescueScenarioProbability = scenarioProbability / (apexBoundaryIndexes.Count * innovBoundaryIndexes.Count);
    foreach (var apexBoundaryIndex in apexBoundaryIndexes)
    {
        foreach (var innovBoundaryIndex in innovBoundaryIndexes)
        {
            var blackWinsProbability = GetWinProbability(participants[innovBoundaryIndex], participants[apexBoundaryIndex], blackAdvantageRating);
            AccumulateFinalStagePlaceProbabilitiesWithBoundaryRescue(
                wins,
                apexRanking,
                innovRanking,
                additionalApexCount,
                apexBoundaryIndex,
                innovBoundaryIndex,
                rescueScenarioProbability,
                blackWinsProbability,
                placeProbabilities);
        }
    }

static void AccumulateVariableTop8PlaceProbabilities(int[] wins, IReadOnlyList<int> apexParticipantIndexes, IReadOnlyList<int> innovParticipantIndexes, int additionalApexCount, int promotedInnovCount, double scenarioProbability, double[,] placeProbabilities)
{
    var apexRanking = BuildRankingForParticipantIndexes(wins, apexParticipantIndexes);
    var innovRanking = BuildRankingForParticipantIndexes(wins, innovParticipantIndexes);
    var actualPromotedInnovCount = Math.Min(promotedInnovCount, Math.Min(apexRanking.Length, innovRanking.Length));
    var leadingApexCount = Math.Max(0, apexRanking.Length - actualPromotedInnovCount);

    var leadingApexRanking = apexRanking.Take(leadingApexCount).ToArray();
    var trailingApexRanking = apexRanking.Skip(leadingApexCount).ToArray();
    var promotedInnovRanking = innovRanking.Take(actualPromotedInnovCount).ToArray();
    var remainingInnovRanking = innovRanking.Skip(actualPromotedInnovCount).ToArray();

    AccumulateRankingProbabilities(leadingApexRanking, 0, scenarioProbability, placeProbabilities);
    AccumulateRankingProbabilities(promotedInnovRanking, leadingApexCount, scenarioProbability, placeProbabilities);
    AccumulateRankingProbabilities(trailingApexRanking, leadingApexCount + actualPromotedInnovCount, scenarioProbability, placeProbabilities);
    AccumulateRankingProbabilities(remainingInnovRanking, apexRanking.Length + actualPromotedInnovCount + additionalApexCount, scenarioProbability, placeProbabilities);
}
}

static ParticipantScore[] BuildRankingForParticipantIndexes(int[] wins, IReadOnlyList<int> participantIndexes)
{
    return participantIndexes
        .Select(index => new ParticipantScore(index, wins[index]))
        .OrderByDescending(x => x.Wins)
        .ThenBy(x => x.ParticipantIndex)
        .ToArray();
}

static List<int> GetTiedParticipantIndexesAtPosition(IReadOnlyList<ParticipantScore> ranking, int position)
{
    var tiedParticipantIndexes = new List<int>();
    var targetWins = ranking[position].Wins;
    var index = position;
    while (index > 0 && ranking[index - 1].Wins == targetWins)
    {
        index--;
    }

    while (index < ranking.Count && ranking[index].Wins == targetWins)
    {
        tiedParticipantIndexes.Add(ranking[index].ParticipantIndex);
        index++;
    }

    return tiedParticipantIndexes;
}

static void AccumulateFinalStagePlaceProbabilitiesWithBoundaryRescue(
    int[] wins,
    IReadOnlyList<ParticipantScore> apexRanking,
    IReadOnlyList<ParticipantScore> innovRanking,
    int additionalApexCount,
    int apexBoundaryIndex,
    int innovBoundaryIndex,
    double rescueScenarioProbability,
    double innovWinsProbability,
    double[,] placeProbabilities)
{
    AccumulateBoundaryRescueOutcome(wins, apexRanking, innovRanking, additionalApexCount, apexBoundaryIndex, innovBoundaryIndex, rescueScenarioProbability * innovWinsProbability, innovWins: true, placeProbabilities);
    AccumulateBoundaryRescueOutcome(wins, apexRanking, innovRanking, additionalApexCount, apexBoundaryIndex, innovBoundaryIndex, rescueScenarioProbability * (1.0 - innovWinsProbability), innovWins: false, placeProbabilities);
}

static void AccumulateBoundaryRescueOutcome(
    int[] wins,
    IReadOnlyList<ParticipantScore> apexRanking,
    IReadOnlyList<ParticipantScore> innovRanking,
    int additionalApexCount,
    int apexBoundaryIndex,
    int innovBoundaryIndex,
    double scenarioProbability,
    bool innovWins,
    double[,] placeProbabilities)
{
    var apexGroupSize = apexRanking.Count;
    var rescuedApexRanking = apexRanking
        .Where(x => x.ParticipantIndex != apexBoundaryIndex)
        .ToArray();
    var rescuedInnovRanking = innovRanking
        .Where(x => x.ParticipantIndex != innovBoundaryIndex)
        .ToArray();

    AccumulateRankingProbabilities(rescuedApexRanking, 0, scenarioProbability, placeProbabilities);

    if (innovWins)
    {
        placeProbabilities[innovBoundaryIndex, apexGroupSize - 1] += scenarioProbability;
        placeProbabilities[apexBoundaryIndex, apexGroupSize + additionalApexCount] += scenarioProbability;
    }
    else
    {
        placeProbabilities[apexBoundaryIndex, apexGroupSize - 1] += scenarioProbability;
        placeProbabilities[innovBoundaryIndex, apexGroupSize + additionalApexCount] += scenarioProbability;
    }

    AccumulateRankingProbabilities(
        rescuedInnovRanking,
        apexGroupSize + additionalApexCount + 1,
        scenarioProbability,
        placeProbabilities);
}

static void AccumulateRankingProbabilities(IReadOnlyList<ParticipantScore> ranking, int placeOffset, double scenarioProbability, double[,] placeProbabilities)
{
    var currentPlace = 0;
    while (currentPlace < ranking.Count)
    {
        var groupEnd = currentPlace + 1;
        while (groupEnd < ranking.Count && ranking[groupEnd].Wins == ranking[currentPlace].Wins)
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
                placeProbabilities[participantIndex, placeOffset + place] += splitProbability;
            }
        }

        currentPlace = groupEnd;
    }
}

static void AccumulateGroupPlaceProbabilities(int[] wins, IReadOnlyList<int> participantIndexes, int placeOffset, double scenarioProbability, double[,] placeProbabilities)
{
    var ranking = participantIndexes
        .Select(index => new ParticipantScore(index, wins[index]))
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
                placeProbabilities[participantIndex, placeOffset + place] += splitProbability;
            }
        }

        currentPlace = groupEnd;
    }
}

static bool ValidateFinalStageMatches(IReadOnlyList<Participant> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, IReadOnlyList<Match> matches, out string errorMessage)
{
    errorMessage = string.Empty;

    for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
    {
        var match = matches[matchIndex];
        var blackParticipant = participants[match.Black];
        var whiteParticipant = participants[match.White];

        var blackGroup = groupMap[blackParticipant.Name];
        var whiteGroup = groupMap[whiteParticipant.Name];

        if (blackGroup == whiteGroup)
        {
            errorMessage = $"{matchIndex + 1} 局目の対局 '{blackParticipant.Name} vs {whiteParticipant.Name}' は同グループ同士です。";
            return false;
        }

        if (blackGroup != FinalStageGroup.Innov)
        {
            errorMessage = $"{matchIndex + 1} 局目の黒番 '{blackParticipant.Name}' は Innov である必要があります。";
            return false;
        }

        if (whiteGroup != FinalStageGroup.Apex)
        {
            errorMessage = $"{matchIndex + 1} 局目の白番 '{whiteParticipant.Name}' は Apex である必要があります。";
            return false;
        }
    }

    return true;
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

static List<FinalStageResultRow> BuildFinalStageResultRows(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, CalculationResult result, double blackAdvantagePercent, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount)
{
    var standardRows = BuildResultRows(participants, matches, result, blackAdvantagePercent);
    var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
    var innovCount = participants.Count - apexCount;

    return standardRows
        .Select(row =>
        {
            var group = groupMap[row.Name];
            var groupStartIndex = group == FinalStageGroup.Apex ? 0 : apexCount + additionalApexCount;
            var groupSize = group == FinalStageGroup.Apex ? apexCount : innovCount;
            var groupPlaceAverage = Enumerable.Range(0, groupSize)
                .Sum(offset => (offset + 1) * row.PlaceProbabilities[groupStartIndex + offset]);

            return new FinalStageResultRow(
                row.Name,
                group.ToString(),
                row.OriginalRating,
                row.EffectiveRating,
                row.RatingDelta,
                row.BlackCount,
                row.WhiteCount,
                row.BlackWinRate,
                row.WhiteWinRate,
                row.PlaceProbabilities[groupStartIndex],
                groupPlaceAverage,
                row.PlaceProbabilities[0],
                row.AveragePlace,
                row.PlaceProbabilities,
                row.PlaceCounts);
        })
        .ToList();
}

static List<QualityParticipantRow> BuildQualityParticipantRows(IReadOnlyList<ResultRow> resultRows, IReadOnlyDictionary<string, FinalStageGroup> groupMap, IReadOnlyList<Participant> additionalApexParticipants, AdditionalApexPlacementMode placementMode)
{
    var allParticipants = resultRows
        .Select(row => new Participant(row.Name, row.OriginalRating))
        .Concat(placementMode == AdditionalApexPlacementMode.Off ? additionalApexParticipants : Enumerable.Empty<Participant>())
        .ToList();

    var eloRanks = allParticipants
        .OrderByDescending(x => x.Rating)
        .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
        .Select((participant, index) => new { participant.Name, Rank = index + 1 })
        .ToDictionary(x => x.Name, x => x.Rank, StringComparer.OrdinalIgnoreCase);

    return resultRows
        .Select(row =>
        {
            var eloRank = eloRanks[row.Name];
            var overallTop8Probability = row.PlaceProbabilities.Take(Math.Min(8, row.PlaceProbabilities.Length)).Sum();
            return new QualityParticipantRow(
                row.Name,
                groupMap[row.Name].ToString(),
                row.OriginalRating,
                eloRank,
                row.AveragePlace,
                row.AveragePlace - eloRank,
                row.ChampionshipProbability,
                overallTop8Probability);
        })
        .OrderBy(x => x.EloRank)
        .ToList();
}

static QualitySummary BuildQualitySummary(IReadOnlyList<QualityParticipantRow> participantRows)
{
    var spearmanCorrelation = CalculateSpearmanCorrelation(participantRows);
    var meanAbsoluteRankError = participantRows.Average(x => Math.Abs(x.OverallPlaceDeltaFromEloRank));
    var averageTop8Retention = participantRows
        .Where(x => x.EloRank <= 8)
        .Sum(x => x.OverallTop8Probability);

    var topEloParticipant = participantRows.OrderBy(x => x.EloRank).First();
    var mostPenalizedParticipant = participantRows.OrderByDescending(x => x.OverallPlaceDeltaFromEloRank).First();
    var mostAdvantagedParticipant = participantRows.OrderBy(x => x.OverallPlaceDeltaFromEloRank).First();

    return new QualitySummary(
        spearmanCorrelation,
        meanAbsoluteRankError,
        averageTop8Retention,
        topEloParticipant.OverallTop1Probability,
        mostPenalizedParticipant.Name,
        mostPenalizedParticipant.OverallPlaceDeltaFromEloRank,
        mostAdvantagedParticipant.Name,
        mostAdvantagedParticipant.OverallPlaceDeltaFromEloRank);
}

static double CalculateSpearmanCorrelation(IReadOnlyList<QualityParticipantRow> participantRows)
{
    if (participantRows.Count <= 1)
    {
        return 1.0;
    }

    var eloRanks = participantRows
        .OrderBy(x => x.EloRank)
        .Select(x => (double)x.EloRank)
        .ToArray();
    var overallPlaceRanks = GetAverageRanks(participantRows.Select(x => x.ExpectedOverallPlace).ToArray());

    return CalculatePearsonCorrelation(eloRanks, overallPlaceRanks);
}

static double[] GetAverageRanks(IReadOnlyList<double> values)
{
    var ordered = values
        .Select((value, index) => new { Value = value, Index = index })
        .OrderBy(x => x.Value)
        .ToArray();

    var ranks = new double[values.Count];
    var current = 0;
    while (current < ordered.Length)
    {
        var end = current + 1;
        while (end < ordered.Length && ordered[end].Value.Equals(ordered[current].Value))
        {
            end++;
        }

        var averageRank = (current + 1 + end) / 2.0;
        for (var i = current; i < end; i++)
        {
            ranks[ordered[i].Index] = averageRank;
        }

        current = end;
    }

    return ranks;
}

static double CalculatePearsonCorrelation(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
{
    var meanX = xs.Average();
    var meanY = ys.Average();
    var covariance = 0.0;
    var varianceX = 0.0;
    var varianceY = 0.0;

    for (var i = 0; i < xs.Count; i++)
    {
        var dx = xs[i] - meanX;
        var dy = ys[i] - meanY;
        covariance += dx * dy;
        varianceX += dx * dx;
        varianceY += dy * dy;
    }

    if (varianceX <= 0.0 || varianceY <= 0.0)
    {
        return 1.0;
    }

    return covariance / Math.Sqrt(varianceX * varianceY);
}

static void PrintQualitySummary(QualitySummary summary)
{
    Console.WriteLine("品質評価サマリー:");
    Console.WriteLine($"- Spearman 相関: {summary.SpearmanCorrelation.ToString("F4", CultureInfo.InvariantCulture)}");
    Console.WriteLine($"- 平均順位ずれ: {summary.MeanAbsoluteRankError.ToString("F3", CultureInfo.InvariantCulture)}");
    Console.WriteLine($"- Elo上位8名の総合上位8位残留人数（平均）: {summary.AverageTop8Retention.ToString("F3", CultureInfo.InvariantCulture)}");
    Console.WriteLine($"- Elo1位の総合1位確率: {FormatPercent(summary.EloTop1OverallTop1Probability)}");
    Console.WriteLine($"- 最大不利益: {summary.MostPenalizedParticipantName} ({summary.MostPenalizedDelta.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture)})");
    Console.WriteLine($"- 最大利益: {summary.MostAdvantagedParticipantName} ({summary.MostAdvantagedDelta.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture)})\n");
}

static void PrintQualityParticipantHighlights(IReadOnlyList<QualityParticipantRow> participantRows)
{
    Console.WriteLine("品質評価 参加者別ハイライト:");
    Console.WriteLine("Elo順位  名前                 期待総合順位   ずれ      総合1位確率   総合上位8確率");

    foreach (var row in participantRows.Take(8))
    {
        Console.WriteLine(
            row.EloRank.ToString(CultureInfo.InvariantCulture).PadLeft(6)
            + "  " + row.Name.PadRight(20)
            + row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
            + row.OverallPlaceDeltaFromEloRank.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture).PadLeft(10)
            + FormatPercent(row.OverallTop1Probability).PadLeft(14)
            + FormatPercent(row.OverallTop8Probability).PadLeft(14));
    }

    Console.WriteLine();
}

static void WriteQualitySummaryCsv(string outputCsvPath, QualitySummary summary, ExperimentalReportGroupingOptions options)
{
    var directoryPath = Path.GetDirectoryName(outputCsvPath);
    if (!string.IsNullOrWhiteSpace(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    var lines = new List<string>
    {
        "metricName,metricValue,note",
        $"spearmanCorrelation,{summary.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture)},Elo順位と期待総合順位の相関",
        $"meanAbsoluteRankError,{summary.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture)},期待総合順位とElo順位のずれの絶対値平均",
        $"averageTop8Retention,{summary.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture)},Elo上位8名が総合上位8位に残る人数の期待値",
        $"eloTop1OverallTop1Probability,{(summary.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture)},Elo1位が総合1位になる確率(%)",
        $"mostPenalizedParticipantDelta,{summary.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture)},{EscapeCsv(summary.MostPenalizedParticipantName)}",
        $"mostAdvantagedParticipantDelta,{summary.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture)},{EscapeCsv(summary.MostAdvantagedParticipantName)}"
    };

    if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
    {
        lines.Add($"evaluationMemo,,{EscapeCsv(options.EvaluationMemo)}");
    }

    File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
}

static string BuildQualitySweepDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, ExperimentalReportGroupingOptions options)
{
    var fileName = $"quality_sweep_{placementMode}_{boundaryRescueMode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
    if (!options.IsEnabled)
    {
        return Path.GetFullPath(fileName);
    }

    var outcomeFolderName = options.Outcome == ExperimentalReportOutcome.Bad ? "Bad" : "Good";
    var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
    Directory.CreateDirectory(baseDirectory);
    return Path.Combine(baseDirectory, fileName);
}

static void WriteQualitySweepCsv(string outputCsvPath, IReadOnlyList<QualitySweepRow> sweepRows, ExperimentalReportGroupingOptions options)
{
    var directoryPath = Path.GetDirectoryName(outputCsvPath);
    if (!string.IsNullOrWhiteSpace(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    var lines = new List<string>
    {
        "blackAdvantagePercent,spearmanCorrelation,meanAbsoluteRankError,averageTop8Retention,eloTop1OverallTop1ProbabilityPercent,mostPenalizedParticipant,mostPenalizedDelta,mostAdvantagedParticipant,mostAdvantagedDelta"
    };

    lines.AddRange(sweepRows.Select(row => string.Join(",",
        row.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture),
        row.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture),
        row.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture),
        row.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture),
        (row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture),
        EscapeCsv(row.MostPenalizedParticipantName),
        row.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture),
        EscapeCsv(row.MostAdvantagedParticipantName),
        row.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture))));

    if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
    {
        lines.Add($"evaluationMemo,,,,,,,{EscapeCsv(options.EvaluationMemo)},");
    }

    File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
}

static void WriteQualityParticipantCsv(string outputCsvPath, IReadOnlyList<QualityParticipantRow> participantRows)
{
    var directoryPath = Path.GetDirectoryName(outputCsvPath);
    if (!string.IsNullOrWhiteSpace(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    var lines = new List<string>
    {
        "participantName,group,originalElo,eloRank,expectedOverallPlace,overallPlaceDeltaFromEloRank,overallTop1ProbabilityPercent,overallTop8ProbabilityPercent"
    };

    lines.AddRange(participantRows.Select(row => string.Join(",",
        EscapeCsv(row.Name),
        EscapeCsv(row.Group),
        FormatRating(row.OriginalRating),
        row.EloRank.ToString(CultureInfo.InvariantCulture),
        row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture),
        row.OverallPlaceDeltaFromEloRank.ToString("F3", CultureInfo.InvariantCulture),
        (row.OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
        (row.OverallTop8Probability * 100).ToString("F2", CultureInfo.InvariantCulture))));

    File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
}

static string BuildSiblingOutputCsvPath(string baseCsvPath, string fileNamePrefix)
{
    var directoryPath = Path.GetDirectoryName(baseCsvPath) ?? Path.GetFullPath(".");
    return Path.Combine(directoryPath, $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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

static void PrintFinalStageResult(CalculationResult result, double blackAdvantagePercent, IReadOnlyList<FinalStageResultRow> resultRows)
{
    Console.WriteLine($"計算方法: {result.Mode}\n");
    Console.WriteLine($"同Elo対局時の先手勝率: {blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%\n");

    var nameWidth = Math.Max(6, resultRows.Max(x => x.Name.Length) + 2);
    var header = "対局者".PadRight(nameWidth)
        + "群".PadLeft(8)
        + "元Elo".PadLeft(10)
        + "実効Elo".PadLeft(10)
        + "差分".PadLeft(10)
        + "黒番".PadLeft(8)
        + "白番".PadLeft(8)
        + "群1位".PadLeft(10)
        + "群平均".PadLeft(10)
        + "総合1位".PadLeft(10)
        + "総合平均".PadLeft(10);

    Console.WriteLine(header);
    Console.WriteLine(new string('-', header.Length));

    foreach (var row in resultRows)
    {
        var line = row.Name.PadRight(nameWidth)
            + row.Group.PadLeft(8)
            + FormatRating(row.OriginalRating).PadLeft(10)
            + FormatRating(row.EffectiveRating).PadLeft(10)
            + FormatSignedRating(row.RatingDelta).PadLeft(10)
            + row.BlackCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
            + row.WhiteCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
            + FormatPercent(row.GroupPlace1Probability).PadLeft(10)
            + row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture).PadLeft(10)
            + FormatPercent(row.OverallPlace1Probability).PadLeft(10)
            + row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture).PadLeft(10);

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

static void WriteFinalStageResultCsv(string outputCsvPath, string mode, double blackAdvantagePercent, IReadOnlyList<FinalStageResultRow> resultRows)
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
        "group",
        "originalElo",
        "effectiveElo",
        "eloDelta",
        "blackCount",
        "whiteCount",
        "blackWinRatePercent",
        "whiteWinRatePercent",
        "groupPlace1ProbabilityPercent",
        "groupPlaceAverage",
        "overallPlace1ProbabilityPercent",
        "overallPlaceAverage"
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
            row.Group,
            FormatRating(row.OriginalRating),
            FormatRating(row.EffectiveRating),
            FormatSignedRating(row.RatingDelta),
            row.BlackCount.ToString(CultureInfo.InvariantCulture),
            row.WhiteCount.ToString(CultureInfo.InvariantCulture),
            FormatOptionalPercentValue(row.BlackWinRate),
            FormatOptionalPercentValue(row.WhiteWinRate),
            (row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
            row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture),
            (row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
            row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)
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
    if (input is null)
    {
        throw new OperationCanceledException("文字列入力中に入力ストリームが終了しました。");
    }

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

//static int ReadInt(string prompt, int min)
//{
//    while (true)
//    {
//        Console.Write(prompt);
//        var input = Console.ReadLine();
//        if (int.TryParse(input, out var value) && value >= min)
//        {
//            return value;
//        }

//        Console.WriteLine($"{min} 以上の整数を入力してください。");
//    }
//}

static int ReadIntWithDefault(string prompt, int defaultValue, int min)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();
        if (input is null)
        {
            throw new OperationCanceledException("整数入力中に入力ストリームが終了しました。");
        }

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
        if (input is null)
        {
            throw new OperationCanceledException("数値入力中に入力ストリームが終了しました。");
        }

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

static bool TryParseFinalStageGroups(IReadOnlyList<string> lines, out Dictionary<string, FinalStageGroup> groupMap, out string errorMessage)
{
    groupMap = new Dictionary<string, FinalStageGroup>(StringComparer.OrdinalIgnoreCase);
    errorMessage = string.Empty;

    var startIndex = 0;
    var firstColumns = SplitCsvLine(lines[0]);
    if (IsFinalStageGroupHeaderRow(firstColumns))
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

        var groupText = columns[0].Trim();
        var name = columns[1].Trim();
        if (string.IsNullOrWhiteSpace(groupText) || string.IsNullOrWhiteSpace(name))
        {
            errorMessage = $"{i + 1} 行目のグループ名または選手名が空です。";
            return false;
        }

        if (!TryParseFinalStageGroup(groupText, out var group))
        {
            errorMessage = $"{i + 1} 行目のグループ名 '{groupText}' は Apex または Innov で入力してください。";
            return false;
        }

        if (groupMap.ContainsKey(name))
        {
            errorMessage = $"{i + 1} 行目の選手名 '{name}' は重複しています。";
            return false;
        }

        groupMap.Add(name, group);
    }

    if (groupMap.Count == 0)
    {
        errorMessage = "グループ対応CSVに 1 行以上のデータが必要です。";
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

static bool IsFinalStageGroupHeaderRow(IReadOnlyList<string> columns)
{
    if (columns.Count < 2)
    {
        return false;
    }

    var first = columns[0].Trim();
    var second = columns[1].Trim();

    return (first.Equals("group", StringComparison.OrdinalIgnoreCase)
            || first.Equals("グループ", StringComparison.OrdinalIgnoreCase))
        && (second.Equals("name", StringComparison.OrdinalIgnoreCase)
            || second.Equals("名前", StringComparison.OrdinalIgnoreCase)
            || second.Equals("participantName", StringComparison.OrdinalIgnoreCase)
            || second.Equals("選手名", StringComparison.OrdinalIgnoreCase));
}

static bool TryParseFinalStageGroup(string value, out FinalStageGroup group)
{
    if (value.Equals("Apex", StringComparison.OrdinalIgnoreCase))
    {
        group = FinalStageGroup.Apex;
        return true;
    }

    if (value.Equals("Innov", StringComparison.OrdinalIgnoreCase))
    {
        group = FinalStageGroup.Innov;
        return true;
    }

    group = default;
    return false;
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
    PrintMatchesCsv(participants, matches, "生成された対局CSV:");
}

static void PrintMatchesCsv(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, string title)
{
    Console.WriteLine(title);
    Console.WriteLine("black,white");

    foreach (var match in matches)
    {
        Console.WriteLine($"{EscapeCsv(participants[match.Black].Name)},{EscapeCsv(participants[match.White].Name)}");
    }

    Console.WriteLine();
}

static void WriteReferenceMatchCsv(string outputCsvPath, IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches)
{
    var directoryPath = Path.GetDirectoryName(outputCsvPath);
    if (!string.IsNullOrWhiteSpace(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    var lines = new List<string>
    {
        "black,white"
    };

    lines.AddRange(matches.Select(match => $"{EscapeCsv(participants[match.Black].Name)},{EscapeCsv(participants[match.White].Name)}"));
    File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
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

readonly record struct FinalStageResultRow(
    string Name,
    string Group,
    double OriginalRating,
    double EffectiveRating,
    double RatingDelta,
    int BlackCount,
    int WhiteCount,
    double? BlackWinRate,
    double? WhiteWinRate,
    double GroupPlace1Probability,
    double GroupPlaceAverage,
    double OverallPlace1Probability,
    double OverallPlaceAverage,
    double[] PlaceProbabilities,
    double[]? PlaceCounts);

readonly record struct QualityParticipantRow(
    string Name,
    string Group,
    double OriginalRating,
    int EloRank,
    double ExpectedOverallPlace,
    double OverallPlaceDeltaFromEloRank,
    double OverallTop1Probability,
    double OverallTop8Probability);

readonly record struct QualitySummary(
    double SpearmanCorrelation,
    double MeanAbsoluteRankError,
    double AverageTop8Retention,
    double EloTop1OverallTop1Probability,
    string MostPenalizedParticipantName,
    double MostPenalizedDelta,
    string MostAdvantagedParticipantName,
    double MostAdvantagedDelta);

readonly record struct QualityEvaluationRun(
    IReadOnlyList<QualityParticipantRow> ParticipantRows,
    QualitySummary Summary);

readonly record struct QualitySweepOptions(
    bool IsEnabled,
    double StartPercent,
    double EndPercent,
    double StepPercent);

readonly record struct QualitySweepRow(
    double BlackAdvantagePercent,
    double SpearmanCorrelation,
    double MeanAbsoluteRankError,
    double AverageTop8Retention,
    double EloTop1OverallTop1Probability,
    string MostPenalizedParticipantName,
    double MostPenalizedDelta,
    string MostAdvantagedParticipantName,
    double MostAdvantagedDelta);

readonly record struct ExperimentalReportGroupingOptions(
    bool IsEnabled,
    ExperimentalReportOutcome? Outcome,
    string EvaluationMemo);

enum FinalStageGroup
{
    Apex,
    Innov,
}

enum AdditionalApexPlacementMode
{
    Off,
    On,
}

enum BoundaryRescueMode
{
    Off,
    On,
}

enum VariableTop8Mode
{
    Off,
    On,
}

enum ExperimentalReportOutcome
{
    Good,
    Bad,
}
