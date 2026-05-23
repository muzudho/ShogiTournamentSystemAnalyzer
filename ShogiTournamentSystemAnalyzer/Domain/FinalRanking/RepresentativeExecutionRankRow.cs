/*
 * ［最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.FinalRanking;

internal readonly record struct RepresentativeExecutionRankRow(
    string Name,
    int Points,
    string RankLabel,
    double AveragePlace,
    double FirstPlaceProbability);
