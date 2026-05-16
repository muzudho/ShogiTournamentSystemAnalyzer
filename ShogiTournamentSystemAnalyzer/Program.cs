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
