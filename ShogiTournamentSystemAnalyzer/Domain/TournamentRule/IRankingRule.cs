using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IRankingRule
{
    IReadOnlyList<PlayerRankRow> Rank(TournamentState state, int? stageId);
}
