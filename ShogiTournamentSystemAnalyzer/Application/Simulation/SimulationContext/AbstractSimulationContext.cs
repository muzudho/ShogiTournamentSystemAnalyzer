namespace ShogiTournamentSystemAnalyzer.Application.Simulation.SimulationContext;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal abstract record class AbstractSimulationContext(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches);
