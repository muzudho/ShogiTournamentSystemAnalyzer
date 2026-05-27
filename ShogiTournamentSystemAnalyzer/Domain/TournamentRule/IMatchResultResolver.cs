/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IMatchResultResolver
{
    TournamentMatchRecord Resolve(TournamentState state, TournamentMatchRecord match, Random random);
}
