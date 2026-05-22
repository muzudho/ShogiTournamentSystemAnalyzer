using ShogiTournamentSystemAnalyzer.Domain.Simulation;

interface IPairingRule
{
    TournamentState Apply(TournamentState state, int stageId);
}
