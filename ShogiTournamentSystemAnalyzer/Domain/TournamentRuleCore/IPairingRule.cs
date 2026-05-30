/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IPairingRule
{
    TournamentState Apply(TournamentState state, int stageId);
}
