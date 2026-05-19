sealed record class TournamentFrameworkExecutionResult(
    TournamentState FinalState,
    IReadOnlyList<PlayerRankRow> OverallRanking,
    int TickCount,
    bool CompletedNaturally);
