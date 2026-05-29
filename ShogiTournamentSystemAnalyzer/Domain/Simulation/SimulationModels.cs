/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.Simulation;

using System.Globalization;

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

internal interface ISimulationResultRow
{
    string Name { get; }
    double OriginalRating { get; }
    double EffectiveRating { get; }
    double RatingDelta { get; }
    int FirstPlayerCount { get; }
    int SecondPlayerCount { get; }
    double? FirstPlayerWinRate { get; }
    double? SecondPlayerWinRate { get; }
    double[] PlaceProbabilities { get; }
    double[]? PlaceCounts { get; }

    /// <summary>
    /// ［最終順位という境界］のCSV形式データの指定列を取得
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<string> GetFinalRankingCsvSpecificColumns();
}

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
///     <pre>
/// ［標準版］の結果の行
/// 
/// TODO: なんで［本戦版］と違う構造にしてしまったのか（＾～＾）？　統一してほしい（＾～＾）
/// TODO: 統一が無理なら、共通部と、専門部を分けてほしい（＾～＾）
///     </pre>
/// </summary>
/// <param name="Name"></param>
/// <param name="OriginalRating"></param>
/// <param name="EffectiveRating"></param>
/// <param name="RatingDelta"></param>
/// <param name="FirstPlayerCount"></param>
/// <param name="SecondPlayerCount"></param>
/// <param name="FirstPlayerWinRate"></param>
/// <param name="SecondPlayerWinRate"></param>
/// <param name="ChampionshipProbability"></param>
/// <param name="AveragePlace"></param>
/// <param name="PlaceProbabilities"></param>
/// <param name="PlaceCounts"></param>
readonly record struct StandardResultRow(
    SimulationResultRowCommonData CommonData,
    double ChampionshipProbability,     // ［標準版］優勝確率
    double AveragePlace)                // ［標準版］平均順位
    : ISimulationResultRow
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

    /// <summary>
    /// ［最終順位という境界］のCSV形式データの指定列を取得
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<string> GetFinalRankingCsvSpecificColumns()
    {
        return
        [
            Name,
            SimulationRatingMath.FormatRating(OriginalRating),
            SimulationRatingMath.FormatRating(EffectiveRating),
            SimulationRatingMath.FormatSignedRating(RatingDelta),
            FirstPlayerCount.ToString(CultureInfo.InvariantCulture),
            SecondPlayerCount.ToString(CultureInfo.InvariantCulture),
            SimulationRatingMath.FormatOptionalPercentValue(FirstPlayerWinRate),
            SimulationRatingMath.FormatOptionalPercentValue(SecondPlayerWinRate),
            (ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture),
            AveragePlace.ToString("F3", CultureInfo.InvariantCulture)
        ];
    }
}

/// <summary>
///     <pre>
/// ［本戦版］の結果の行
/// 
/// TODO: なんで［標準版］と違う構造にしてしまったのか（＾～＾）？　統一してほしい（＾～＾）
/// TODO: 統一が無理なら、共通部と、専門部を分けてほしい（＾～＾）
///     </pre>
/// </summary>
/// <param name="Name"></param>
/// <param name="Group"></param>
/// <param name="OriginalRating"></param>
/// <param name="EffectiveRating"></param>
/// <param name="RatingDelta"></param>
/// <param name="FirstPlayerCount"></param>
/// <param name="SecondPlayerCount"></param>
/// <param name="FirstPlayerWinRate"></param>
/// <param name="SecondPlayerWinRate"></param>
/// <param name="GroupPlace1Probability"></param>
/// <param name="GroupPlaceAverage"></param>
/// <param name="OverallPlace1Probability"></param>
/// <param name="OverallPlaceAverage"></param>
/// <param name="PlaceProbabilities"></param>
/// <param name="PlaceCounts"></param>
readonly record struct FinalStageResultRow(
    SimulationResultRowCommonData CommonData,
    string Group,   // ［本戦版］の行にはグループ列がある
    double GroupPlace1Probability,      // ［本戦版］グループ内1位の確率
    double GroupPlaceAverage,           // ［本戦版］グループ内の平均順位
    double OverallPlace1Probability,    // ［本戦版］全体で1位の確率
    double OverallPlaceAverage)         // ［本戦版］全体の平均順位
    : ISimulationResultRow
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

    /// <summary>
    /// ［最終順位という境界］のCSV形式データの指定列を取得
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<string> GetFinalRankingCsvSpecificColumns()
    {
        return
        [
            Name,
            Group,
            SimulationRatingMath.FormatRating(OriginalRating),
            SimulationRatingMath.FormatRating(EffectiveRating),
            SimulationRatingMath.FormatSignedRating(RatingDelta),
            FirstPlayerCount.ToString(CultureInfo.InvariantCulture),
            SecondPlayerCount.ToString(CultureInfo.InvariantCulture),
            SimulationRatingMath.FormatOptionalPercentValue(FirstPlayerWinRate),
            SimulationRatingMath.FormatOptionalPercentValue(SecondPlayerWinRate),
            (GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
            GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture),
            (OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
            OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)
        ];
    }
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
