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
/// 
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
readonly record struct ResultRow(
    string Name,
    double OriginalRating,
    double EffectiveRating,
    double RatingDelta,
    int FirstPlayerCount,
    int SecondPlayerCount,
    double? FirstPlayerWinRate,
    double? SecondPlayerWinRate,
    double ChampionshipProbability,
    double AveragePlace,
    double[] PlaceProbabilities,
    double[]? PlaceCounts);

/// <summary>
/// 
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
    string Name,
    string Group,
    double OriginalRating,
    double EffectiveRating,
    double RatingDelta,
    int FirstPlayerCount,
    int SecondPlayerCount,
    double? FirstPlayerWinRate,
    double? SecondPlayerWinRate,
    double GroupPlace1Probability,
    double GroupPlaceAverage,
    double OverallPlace1Probability,
    double OverallPlaceAverage,
    double[] PlaceProbabilities,
    double[]? PlaceCounts);

