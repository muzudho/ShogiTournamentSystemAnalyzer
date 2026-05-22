using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal readonly record struct TournamentQualityEvaluationInput(
    IReadOnlyList<Player> Participants,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches,
    TournamentQualityEvaluationInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode,
    int InnovExpectedRankOffsetCount);

