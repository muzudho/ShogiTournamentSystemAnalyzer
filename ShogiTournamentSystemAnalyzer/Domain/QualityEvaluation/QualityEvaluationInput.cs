internal readonly record struct QualityEvaluationInput(
    IReadOnlyList<Player> Participants,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches);

