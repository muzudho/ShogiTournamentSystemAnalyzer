/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// 大会進行フレームワーク実行サマリーをコンソールへ表示する。
/// </summary>
static class TournamentFrameworkExecutionSummaryPrinter
{
    internal static void Print(
        TournamentFrameworkSimulationAggregate aggregateResult,
        TournamentFrameworkFinalRankingResult finalRankingResult,
        TournamentDslDefinition? dslDefinition)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(finalRankingResult.TournamentRuleSetMode)}");

        if (aggregateResult.IsExactCalculation)
        {
            Console.WriteLine("計算種別: 厳密計算");
            Console.WriteLine($"進行Tick数: {aggregateResult.AverageTickCount:F2}");
            Console.WriteLine($"自然終了: {(aggregateResult.CompletedNaturallyCount > 0 ? "Yes" : "No")}");
        }
        else
        {
            Console.WriteLine($"集計試行回数: {aggregateResult.CompletedSimulationCount:N0}");
            Console.WriteLine($"平均進行Tick数: {aggregateResult.AverageTickCount:F2}");
            Console.WriteLine($"自然終了率: {aggregateResult.CompletedNaturallyCount:N0}/{aggregateResult.CompletedSimulationCount:N0}");
        }

        Console.WriteLine($"代表実行Tick数: {finalRankingResult.RepresentativeTournamentFinalState.TickCount}");
        Console.WriteLine($"代表実行の自然終了: {(finalRankingResult.RepresentativeTournamentFinalState.CompletedNaturally ? "Yes" : "No")}");
        Console.WriteLine($"ステージ数: {finalRankingResult.RepresentativeStages.Count}");
        Console.WriteLine($"総対局数: {finalRankingResult.RepresentativeTournamentFinalState.MatchRecords.Count}\n");

        if (dslDefinition is not null)
        {
            Console.WriteLine($"DSL TimeAxis: {dslDefinition.TimeAxis}");
            Console.WriteLine($"DSL OverallRanking: {dslDefinition.OverallRankingRuleName}\n");
        }
    }
}