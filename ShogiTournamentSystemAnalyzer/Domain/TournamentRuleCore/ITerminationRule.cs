/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface ITerminationRule
{
    bool ShouldFinish(TournamentState state);
}
