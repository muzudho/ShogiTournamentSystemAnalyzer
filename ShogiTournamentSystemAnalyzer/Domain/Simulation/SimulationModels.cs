/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// ［選手］だ。
/// </summary>
/// <param name="Name">選手名</param>
/// <param name="Rating">選手のレーティング</param>
readonly record struct Player(string Name, double Rating);

/// <summary>
/// ［対局者のペア］だ。
/// </summary>
/// <param name="FirstPlayer">先手選手</param>
/// <param name="SecondPlayer">後手選手</param>
readonly record struct Match(int FirstPlayer, int SecondPlayer);

/// <summary>
/// 選手の［勝ち星数］だ。
/// </summary>
/// <param name="PlayerIndex">選手インデックス</param>
/// <param name="Wins">勝ち星数</param>
readonly record struct PlayerScore(int PlayerIndex, int Wins);

/// <summary>
/// 
/// </summary>
/// <param name="PlaceProbabilities"></param>
/// <param name="Mode"></param>
/// <param name="SimulationCount"></param>
readonly record struct CalculationResult(double[,] PlaceProbabilities, string Mode, int? SimulationCount);

/// <summary>
/// ［シミュレーション］結果の行の共通データ
/// </summary>
/// <param name="Name"></param>
/// <param name="OriginalRating"></param>
/// <param name="EffectiveRating"></param>
/// <param name="RatingDelta"></param>
/// <param name="FirstPlayerCount"></param>
/// <param name="SecondPlayerCount"></param>
/// <param name="FirstPlayerWinRate"></param>
/// <param name="SecondPlayerWinRate"></param>
/// <param name="PlaceProbabilities"></param>
/// <param name="PlaceCounts"></param>
readonly record struct SimulationResultRowCommonData(
    string Name,
    double OriginalRating,
    double EffectiveRating,
    double RatingDelta,
    int FirstPlayerCount,
    int SecondPlayerCount,
    double? FirstPlayerWinRate,
    double? SecondPlayerWinRate,
    double[] PlaceProbabilities,
    double[]? PlaceCounts);

/// <summary>
/// ［シミュレーション］結果行の自由形式列だ。
/// </summary>
/// <param name="Key">列を識別するキー</param>
/// <param name="CsvValue">CSV出力向けの値</param>
/// <param name="DisplayValue">表示向けの値</param>
readonly record struct SimulationResultFreeColumn(
    string Key,
    string CsvValue,
    string DisplayValue);

/// <summary>
/// ［シミュレーション］結果行の意味付き数値だ。
/// </summary>
/// <param name="Key">値を識別するキー</param>
/// <param name="Value">計算や注目点で使う値</param>
readonly record struct SimulationResultMetric(
    string Key,
    double Value);

/// <summary>
/// ［シミュレーション］結果行を、共通部分と自由形式部分に分けた表現だ。
/// </summary>
/// <param name="CommonData">共通部分</param>
/// <param name="FreeColumns">ルールやレポートごとに変わる自由形式列</param>
/// <param name="Metrics">注目点やチャートなどで使う意味付き数値</param>
readonly record struct GeneralSimulationResultRow(
    SimulationResultRowCommonData CommonData,
    IReadOnlyList<SimulationResultFreeColumn> FreeColumns,
    IReadOnlyDictionary<string, SimulationResultMetric> Metrics)
{
    public string Name => CommonData.Name;
    public double OriginalRating => CommonData.OriginalRating;
    public double EffectiveRating => CommonData.EffectiveRating;
    public double RatingDelta => CommonData.RatingDelta;
    public int FirstPlayerCount => CommonData.FirstPlayerCount;
    public int SecondPlayerCount => CommonData.SecondPlayerCount;
    public double? FirstPlayerWinRate => CommonData.FirstPlayerWinRate;
    public double? SecondPlayerWinRate => CommonData.SecondPlayerWinRate;
    public double[] PlaceProbabilities => CommonData.PlaceProbabilities;
    public double[]? PlaceCounts => CommonData.PlaceCounts;
}

/// <summary>
/// ［選手エントリー］だ。
/// </summary>
/// <param name="PlayerId"></param>
/// <param name="Name"></param>
/// <param name="Rating"></param>
readonly record struct PlayerEntry(
    int PlayerId,
    string Name,
    double Rating);

/// <summary>
/// ［ステージエントリー］だ。
/// </summary>
/// <param name="StageId"></param>
/// <param name="StageName"></param>
/// <param name="StageType"></param>
/// <param name="ParentStageId"></param>
/// <param name="OrderNo"></param>
readonly record struct StageEntry(
    int StageId,
    string StageName,
    string StageType,
    int? ParentStageId,
    int OrderNo);

/// <summary>
/// ［対局レコード］だ。
/// </summary>
/// <param name="MatchId"></param>
/// <param name="StageId"></param>
/// <param name="FirstPlayerId"></param>
/// <param name="SecondPlayerId"></param>
/// <param name="StartTime"></param>
/// <param name="EndTime"></param>
/// <param name="Status"></param>
/// <param name="ResultType"></param>
/// <param name="RoundNo"></param>
readonly record struct TournamentMatchRecord(
    int MatchId,
    int StageId,
    int FirstPlayerId,
    int SecondPlayerId,
    int StartTime,
    int EndTime,
    MatchStatus Status,
    MatchResultType ResultType,
    int? RoundNo);

/// <summary>
/// ［対局状態］だ。
/// </summary>
/// <param name="CurrentTime"></param>
/// <param name="Players"></param>
/// <param name="Stages"></param>
/// <param name="MatchRecords"></param>
sealed record class TournamentState(
    int CurrentTime,
    IReadOnlyList<PlayerEntry> Players,
    IReadOnlyList<StageEntry> Stages,
    IReadOnlyList<TournamentMatchRecord> MatchRecords);
