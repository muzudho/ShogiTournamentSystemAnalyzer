interface IRankingRule
{
    IReadOnlyList<PlayerRankRow> Rank(TournamentState state, int? stageId);
}
