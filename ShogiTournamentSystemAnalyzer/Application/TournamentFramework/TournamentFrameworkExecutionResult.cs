sealed record class TournamentFrameworkExecutionResult(
    TournamentState FinalState,
    IReadOnlyList<PlayerRankRow> OverallRanking,
    int TickCount,
    bool CompletedNaturally)
{
    internal TournamentResultData ToTournamentResultData()
    {
        return new TournamentResultData(
            FinalState.MatchRecords,
            FinalState.CurrentTime,
            TickCount,
            CompletedNaturally);
    }

    internal FinalRankingData ToFinalRankingData()
    {
        return new FinalRankingData(
            OverallRanking,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位データ");
    }
}
