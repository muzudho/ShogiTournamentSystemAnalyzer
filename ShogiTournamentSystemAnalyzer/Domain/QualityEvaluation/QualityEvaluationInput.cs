internal readonly record struct TournamentQualityEvaluationInput(
    IReadOnlyList<Player> Participants,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches,
    QualityInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode,
    int InnovExpectedRankOffsetCount);

