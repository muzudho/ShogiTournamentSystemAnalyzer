/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IRankingRule
{
    IReadOnlyList<PlayerRankRow> Rank(TournamentState state, int? stageId);
}
