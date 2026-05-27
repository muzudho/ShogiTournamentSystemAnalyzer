/*
 * ［アプリケーション　＞　モード　＞　シミュレーションコンテキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal readonly record struct StandardModeSimulationContext(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    IReadOnlyList<Player> AllPlayers,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches)
{
    internal int ExcludedPlayerCount => AllPlayers.Count - Players.Count;
}

