internal readonly record struct QualityEvaluationInput(
    IReadOnlyList<Participant> Participants,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches);
