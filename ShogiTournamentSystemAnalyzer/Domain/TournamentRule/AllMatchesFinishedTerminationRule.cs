using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal sealed class AllMatchesFinishedTerminationRule : ITerminationRule
{
    internal static readonly AllMatchesFinishedTerminationRule Instance = new();

    public bool ShouldFinish(TournamentState state)
    {
        return state.MatchRecords.All(match => match.Status is MatchStatus.Finished or MatchStatus.Cancelled);
    }
}
