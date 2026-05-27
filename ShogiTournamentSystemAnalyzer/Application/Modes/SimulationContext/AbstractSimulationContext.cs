namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal abstract record class AbstractSimulationContext(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches);
