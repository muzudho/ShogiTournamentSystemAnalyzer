/// <summary>
/// ［大会進行フレームワーク］の実行結果を表すクラス。
/// </summary>
/// <param name="FinalState">大会の最終状態</param>
/// <param name="OverallRanking">大会の最終順位</param>
/// <param name="TickCount">大会の進行Tick数</param>
/// <param name="CompletedNaturally">大会が自然終了したかどうか</param>
sealed record class TournamentFrameworkExecutionResult(
    TournamentState FinalState,
    IReadOnlyList<PlayerRankRow> OverallRanking,
    int TickCount,
    bool CompletedNaturally)
{
    /// <summary>
    /// ［大会結果データ］として出力
    /// </summary>
    /// <returns></returns>
    internal TournamentResultData ToTournamentResultData()
    {
        return new TournamentResultData(
            FinalState.MatchRecords,
            FinalState.CurrentTime,
            TickCount,
            CompletedNaturally);
    }

    /// <summary>
    /// ［最終順位データ］として出力
    /// </summary>
    /// <returns></returns>
    internal FinalRankingData ToFinalRankingData()
    {
        return new FinalRankingData(
            OverallRanking,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位データ");
    }
}
