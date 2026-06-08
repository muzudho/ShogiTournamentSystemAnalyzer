namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// Aggregated TournamentFramework simulation result.
/// </summary>
/// <param name="PlaceProbabilities"></param>
/// <param name="RequestedSimulationCount"></param>
/// <param name="CompletedSimulationCount"></param>
/// <param name="CompletedNaturallyCount"></param>
/// <param name="AverageTickCount"></param>
/// <param name="IsExactCalculation"></param>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="RepresentativeExecutionResult"></param>
sealed record class TournamentFrameworkSimulationAggregate(
    double[,] PlaceProbabilities,
    int RequestedSimulationCount,
    int CompletedSimulationCount,
    int CompletedNaturallyCount,
    double AverageTickCount,
    bool IsExactCalculation,
    TournamentRuleSetMode TournamentRuleSetMode,
    TournamentFrameworkExecutionResult RepresentativeExecutionResult);
