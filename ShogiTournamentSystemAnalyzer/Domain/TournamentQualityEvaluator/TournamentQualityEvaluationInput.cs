/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using Match = ShogiTournamentSystemAnalyzer.Domain.Simulation.Match;

internal readonly record struct TournamentQualityEvaluationInput(
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches,
    TournamentQualityEvaluationInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode,
    int InnovExpectedRankOffsetCount);

