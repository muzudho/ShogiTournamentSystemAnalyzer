namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Ranking;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentUser;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Request;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;

internal class AnalysisWorkflowNewVersion
{
    public static void Run(RequestBoundary requestBoundary)
    {
        //［大会利用者域］                        `TournamentUser`
        TournamentUserWorkflow.Run(requestBoundary);
        //　　｜
        //　　｜　［大会ルールという境界］        `TournamentRule`
        //　　｜　［プレイヤー一覧という境界］    `PlayerList`
        //　　｜　［順位付けの設定という境界］    `RankingSettings`
        //　　↓
        TournamentFinalStateBoundary tournamentFinalStateBoundary = new();
        //［シミュレーション域］
        SimulationWorkflow.Run(requestBoundary, tournamentFinalStateBoundary);
        //　　｜
        //　　｜　［大会最終状態という境界］      `TournamentFinalState`
        //　　↓
        //［順位付け域］
        FinalRankingBoundary finalRankingBoundary = new();
        RankingWorkflow.Run(tournamentFinalStateBoundary, finalRankingBoundary);
        //　　｜
        //　　｜　［最終順位という境界］          `FinalRanking`
        //　　↓
        //［大会品質評価フロー域］                `TournamentQualityEvaluator`
        TournamentQualityEvaluatorWorkflow.Run(finalRankingBoundary);
        //　　｜
        //　　｜　［大会品質レポートという境界］  `TournamentQualityReport`
        //　　↓
    }
}
