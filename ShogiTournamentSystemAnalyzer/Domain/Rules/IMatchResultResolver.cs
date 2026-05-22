using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IMatchResultResolver
{
    TournamentMatchRecord Resolve(TournamentState state, TournamentMatchRecord match, Random random);
}
