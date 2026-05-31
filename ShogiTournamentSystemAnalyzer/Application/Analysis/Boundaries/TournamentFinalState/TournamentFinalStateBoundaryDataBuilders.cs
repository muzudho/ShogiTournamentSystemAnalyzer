/*
 * ［分析　＞　境界　＞　大会最終状態］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;

/// <summary>
/// 境界データビルダー
/// </summary>
internal static partial class BoundaryDataBuilders
{
    /// <summary>
    /// ［大会最終状態］組立
    /// </summary>
    /// <param name="executionResult"></param>
    /// <returns></returns>
    internal static TournamentFinalStateData BuildTournamentFinalStateBoundaryData(TournamentFrameworkExecutionResult executionResult)
    {
        return new TournamentFinalStateData(
            executionResult.FinalState.MatchRecords,
            executionResult.FinalState.CurrentTime,
            executionResult.TickCount,
            executionResult.CompletedNaturally);
    }
}