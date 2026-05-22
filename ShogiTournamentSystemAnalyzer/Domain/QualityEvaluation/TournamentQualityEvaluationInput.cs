using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using System.Text.RegularExpressions;
using Match = ShogiTournamentSystemAnalyzer.Domain.Simulation.Match;

internal readonly record struct TournamentQualityEvaluationInput(
    IReadOnlyList<Player> Participants,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches,
    TournamentQualityEvaluationInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode,
    int InnovExpectedRankOffsetCount);

