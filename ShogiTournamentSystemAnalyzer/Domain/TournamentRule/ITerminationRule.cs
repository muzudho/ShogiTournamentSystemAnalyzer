/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface ITerminationRule
{
    bool ShouldFinish(TournamentState state);
}
