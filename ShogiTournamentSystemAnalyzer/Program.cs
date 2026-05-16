using System.Globalization;
using System.Text;

/// <summary>
/// ここがプログラムだぜ（＾▽＾）！
/// </summary>
internal static partial class Program
{


    // ========================================
    // 概要
    // ========================================


    /// <summary>
    /// ここからプログラムが始まるぜ（＾▽＾）！
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            // このプログラムの説明を最初にするぜ（＾▽＾）！
            Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！\n");

            // 入力方法を選ばせる（＾▽＾）！
            ConfigureInputSource(args);

            // 大きくモードが分かれるぜ（＾▽＾）！
            //
            // 📍 TODO: ［ルール選択］→［パラメーター設定］→［試行］→［品質評価・レポート作成］の４ステップのシーケンスにした方がいいのでは（＾～＾）？
            //
            switch (ReadMode())
            {
                // ［通常ルール］モード
                case 1:
                    RunStandardMode();
                    break;

                // ［本戦ルール］モード
                case 2:
                    RunFinalStageMode();
                    break;

                // ［品質評価］モード
                case 3:
                    RunQualityEvaluationMode();
                    break;

                default:
                    throw new InvalidOperationException("未対応のモードです。");
            }
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }


    // ========================================
    // 詳細
    // ========================================


    private static readonly TimeSpan SimulationTimeLimit = TimeSpan.FromMinutes(3);
    private static DateTime? _simulationDeadlineUtc;

    /// <summary>
    /// 入力方法を選ばせる（＾▽＾）！
    /// </summary>
    /// <param name="args"></param>
    /// <exception cref="OperationCanceledException"></exception>
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

static Dictionary<string, FinalStageGroup>? ReadOptionalFinalStageGroupMap(FinalStageGroupingMode groupingMode, IReadOnlyList<Participant> participants)
{
    if (groupingMode == FinalStageGroupingMode.Off)
    {
        return null;
    }

    return ReadFinalStageGroupMap();
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

static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, ExperimentalReportGroupingOptions options)
{
    if (groupingMode == FinalStageGroupingMode.Off)
    {
        var fileName = $"quality_summary_neutral_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!options.IsEnabled)
        {
            return Path.GetFullPath(fileName);
        }

        var outcomeFolderName = options.Outcome == ExperimentalReportOutcome.Bad ? "Bad" : "Good";
        var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
        Directory.CreateDirectory(baseDirectory);
        return Path.Combine(baseDirectory, fileName);
    }

    return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
}

static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, ExperimentalReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
{
    if (groupingMode == FinalStageGroupingMode.Off)
    {
        var ruleName = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "twill" : "neutral";
        var fileName = $"quality_summary_{ruleName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!options.IsEnabled)
        {
            return Path.GetFullPath(fileName);
        }

        var outcomeFolderName = options.Outcome == ExperimentalReportOutcome.Bad ? "Bad" : "Good";
        var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
        Directory.CreateDirectory(baseDirectory);
        return Path.Combine(baseDirectory, fileName);
    }

    return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
}

static CalculationResult CalculateExactly(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, double blackAdvantageRating, TournamentRuleSetMode tournamentRuleSetMode = TournamentRuleSetMode.Neutral)
{
    var placeProbabilities = new double[participants.Count, participants.Count];
    var wins = tournamentRuleSetMode == TournamentRuleSetMode.Neutral ? new int[participants.Count] : null;
    var outcomes = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? new bool[matches.Count] : null;

    void Explore(int matchIndex, double scenarioProbability)
    {
        if (matchIndex == matches.Count)
        {
            if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
            {
                TwillTournamentRule.AccumulatePlaceProbabilities(matches, outcomes!, scenarioProbability, placeProbabilities);
            }
            else
            {
                AccumulatePlaceProbabilities(wins!, scenarioProbability, placeProbabilities);
            }

            return;
        }

        var match = matches[matchIndex];
        var blackWinsProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);

        if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
        {
            outcomes![matchIndex] = true;
        }
        else
        {
            wins![match.Black]++;
        }

        Explore(matchIndex + 1, scenarioProbability * blackWinsProbability);
        if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
        {
            wins![match.Black]--;
        }

        if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
        {
            outcomes![matchIndex] = false;
        }
        else
        {
            wins![match.White]++;
        }

        Explore(matchIndex + 1, scenarioProbability * (1.0 - blackWinsProbability));
        if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
        {
            wins![match.White]--;
        }
    }

    Explore(0, 1.0);
    var modeLabel = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "厳密計算 / Twill" : "厳密計算";
    return new CalculationResult(placeProbabilities, modeLabel, null);
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

static CalculationResult CalculateBySimulation(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, double blackAdvantageRating, int simulationCount, TournamentRuleSetMode tournamentRuleSetMode = TournamentRuleSetMode.Neutral)
{
    var placeProbabilities = new double[participants.Count, participants.Count];
    var wins = tournamentRuleSetMode == TournamentRuleSetMode.Neutral ? new int[participants.Count] : null;
    var outcomes = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? new bool[matches.Count] : null;
    var completedSimulationCount = 0;

    for (var simulation = 0; simulation < simulationCount; simulation++)
    {
        if (!HasSimulationTimeRemaining())
        {
            break;
        }

        if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
        {
            Array.Clear(wins!);
        }

        for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
        {
            var match = matches[matchIndex];
            var blackWinsProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);
            if (Random.Shared.NextDouble() < blackWinsProbability)
            {
                if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
                {
                    outcomes![matchIndex] = true;
                }
                else
                {
                    wins![match.Black]++;
                }
            }
            else
            {
                if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
                {
                    outcomes![matchIndex] = false;
                }
                else
                {
                    wins![match.White]++;
                }
            }
        }

        if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
        {
            TwillTournamentRule.AccumulatePlaceProbabilities(matches, outcomes!, 1.0, placeProbabilities);
        }
        else
        {
            AccumulatePlaceProbabilities(wins!, 1.0, placeProbabilities);
        }

        completedSimulationCount++;
    }

    NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

    var modeCoreLabel = tournamentRuleSetMode == TournamentRuleSetMode.Twill
        ? "シミュレーション / Twill"
        : "シミュレーション";
    var modeLabel = completedSimulationCount < simulationCount
        ? $"{modeCoreLabel} ({completedSimulationCount:N0}/{simulationCount:N0}回, 時間切れ)"
        : $"{modeCoreLabel} ({simulationCount:N0}回)";
    return new CalculationResult(placeProbabilities, modeLabel, completedSimulationCount);
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
    var apexParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Apex);
    var innovParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Innov);
    var completedSimulationCount = 0;

    for (var simulation = 0; simulation < simulationCount; simulation++)
    {
        if (!HasSimulationTimeRemaining())
        {
            break;
        }

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

        AccumulateFinalStagePlaceProbabilities(wins, participants, apexParticipantIndexes, innovParticipantIndexes, additionalApexCount, boundaryRescueMode, blackAdvantageRating, 1.0, placeProbabilities, promotedInnovCount);
        completedSimulationCount++;
    }

    NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

    var modeLabel = completedSimulationCount < simulationCount
        ? $"本戦専用 シミュレーション ({completedSimulationCount:N0}/{simulationCount:N0}回, 時間切れ)"
        : $"本戦専用 シミュレーション ({simulationCount:N0}回)";
    return new CalculationResult(placeProbabilities, modeLabel, completedSimulationCount);
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

static List<QualityParticipantRow> BuildQualityParticipantRows(IReadOnlyList<ResultRow> resultRows, IReadOnlyDictionary<string, FinalStageGroup>? groupMap, IReadOnlyList<Participant> additionalApexParticipants, AdditionalApexPlacementMode placementMode)
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
                groupMap is null ? "Neutral" : groupMap[row.Name].ToString(),
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

static string BuildQualitySweepDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, ExperimentalReportGroupingOptions options)
{
    if (groupingMode == FinalStageGroupingMode.Off)
    {
        var fileName = $"quality_sweep_neutral_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!options.IsEnabled)
        {
            return Path.GetFullPath(fileName);
        }

        var outcomeFolderName = options.Outcome == ExperimentalReportOutcome.Bad ? "Bad" : "Good";
        var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
        Directory.CreateDirectory(baseDirectory);
        return Path.Combine(baseDirectory, fileName);
    }

    return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
}

static string BuildQualitySweepDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, ExperimentalReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
{
    if (groupingMode == FinalStageGroupingMode.Off)
    {
        var ruleName = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "twill" : "neutral";
        var fileName = $"quality_sweep_{ruleName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!options.IsEnabled)
        {
            return Path.GetFullPath(fileName);
        }

        var outcomeFolderName = options.Outcome == ExperimentalReportOutcome.Bad ? "Bad" : "Good";
        var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
        Directory.CreateDirectory(baseDirectory);
        return Path.Combine(baseDirectory, fileName);
    }

    return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
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

static SimulationBudgetScope BeginSimulationBudget()
{
    var ownsBudget = !_simulationDeadlineUtc.HasValue;
    if (ownsBudget)
    {
        _simulationDeadlineUtc = DateTime.UtcNow + SimulationTimeLimit;
    }

    return new SimulationBudgetScope(ownsBudget);
}

static bool HasSimulationTimeRemaining()
{
    return !_simulationDeadlineUtc.HasValue || DateTime.UtcNow < _simulationDeadlineUtc.Value;
}

static void NormalizePlaceProbabilities(double[,] placeProbabilities, int sampleCount)
{
    if (sampleCount <= 0)
    {
        return;
    }

    for (var row = 0; row < placeProbabilities.GetLength(0); row++)
    {
        for (var column = 0; column < placeProbabilities.GetLength(1); column++)
        {
            placeProbabilities[row, column] /= sampleCount;
        }
    }
}

readonly record struct SimulationBudgetScope(bool OwnsBudget) : IDisposable
{
    public void Dispose()
    {
        if (OwnsBudget)
        {
            _simulationDeadlineUtc = null;
        }
    }
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
    QualitySummary Summary,
    string CalculationMode);

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

enum FinalStageGroupingMode
{
    Off,
    On,
}

enum ExperimentalReportOutcome
{
    Good,
    Bad,
}
