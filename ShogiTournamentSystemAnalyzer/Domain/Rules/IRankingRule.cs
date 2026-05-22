using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IRankingRule
{
    IReadOnlyList<PlayerRankRow> Rank(TournamentState state, int? stageId);
}
