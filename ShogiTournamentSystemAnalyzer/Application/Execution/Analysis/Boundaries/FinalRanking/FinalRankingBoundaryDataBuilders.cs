/*
 * ［分析　＞　境界　＞　最終順位］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Helpers;

using ShogiTournamentSystemAnalyzer.Application.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;

/// <summary>
/// 境界データビルダー
/// </summary>
internal static partial class BoundaryDataBuilders
{
    /// <summary>
    /// ［最終順位］組立
    /// </summary>
    /// <param name="executionResult"></param>
    /// <returns></returns>
    internal static FinalRankingData BuildFinalRankingBoundaryData(TournamentFrameworkExecutionResult executionResult)
    {
        return new FinalRankingData(
            executionResult.OverallRanking,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位データ");
    }
}