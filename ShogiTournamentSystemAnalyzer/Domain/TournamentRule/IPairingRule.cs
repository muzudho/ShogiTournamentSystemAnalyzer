/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IPairingRule
{
    TournamentState Apply(TournamentState state, int stageId);
}
