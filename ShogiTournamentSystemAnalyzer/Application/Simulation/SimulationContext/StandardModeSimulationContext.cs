/*
 * ［アプリケーション　＞　モード　＞　シミュレーションコンテキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.SimulationContext;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal sealed record class StandardModeSimulationContext(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    IReadOnlyList<Player> AllPlayers,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches)
    : AbstractSimulationContext(
        TournamentRuleSetMode,
        FirstPlayerWinRatePercent,
        FirstPlayerWinRateRating,
        Players,
        Matches)
{
    internal int ExcludedPlayerCount => AllPlayers.Count - Players.Count;
}

