/*
 * ［アプリケーション　＞　実行　＞　最終順位付け要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;

internal static class FinalRankingRequestDispatcher
{
    internal static bool TryExecute(AnalysisExecutionContext context)
    {
        if (context.PendingFinalRanking is null) return false;

        var executed = FinalRankingDomain.TryExecute(context.PendingFinalRanking);
        if (executed) context.ClearPendingFinalRanking();
        return executed;
    }
}
