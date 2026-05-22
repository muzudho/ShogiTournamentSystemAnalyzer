using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface ITerminationRule
{
    bool ShouldFinish(TournamentState state);
}
