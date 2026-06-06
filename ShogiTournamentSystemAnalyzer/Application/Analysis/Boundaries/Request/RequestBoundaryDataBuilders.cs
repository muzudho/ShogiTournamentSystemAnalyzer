/*
 * ［分析　＞　境界　＞　要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;

using ShogiTournamentSystemAnalyzer.Domain.Request.PlayerList;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// 境界データビルダー
/// </summary>
internal static partial class BoundaryDataBuilders
{
    internal static PlayerListData BuildPlayerListBoundaryData(IReadOnlyList<PlayerEntry> players)
    {
        return new PlayerListData(players);
    }
}