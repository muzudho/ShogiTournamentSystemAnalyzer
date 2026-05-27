/*
 * ［アプリケーション］のうち、6大境界データの組み立て役
 */
namespace ShogiTournamentSystemAnalyzer.Application.Helpers;

using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.PlayerList;
using ShogiTournamentSystemAnalyzer.Domain.RankingSettings;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

/// <summary>
/// 境界データビルダー
/// </summary>
internal static class BoundaryDataBuilders
{
    // ========================================
    // 入力3境界
    // ========================================

    /// <summary>
    /// ［大会ルール］組立
    /// </summary>
    /// <param name="context"></param>
    /// <param name="dslDefinition"></param>
    /// <returns></returns>
    internal static TournamentRuleData BuildTournamentRuleBoundaryData(TournamentFrameworkModeContext context, TournamentDslDefinition? dslDefinition)
    {
        return new TournamentRuleData(
            RuleProfileMode.TournamentFramework,
            context.TournamentRuleSetMode,
            context.RuleFilePath,
            context.FirstPlayerWinRatePercent,
            context.RandomSeed,
            dslDefinition is null
                ? "大会進行フレームワークの大会ルールデータ"
                : "大会進行フレームワークの大会ルールデータ（DSL読込あり）");
    }

    /// <summary>
    /// ［選手一覧］組立
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    internal static PlayerListData BuildPlayerListBoundaryData(IReadOnlyList<PlayerEntry> players)
    {
        return new PlayerListData(players);
    }

    /// <summary>
    /// ［順位設定］組立
    /// </summary>
    /// <param name="tournamentRuleData"></param>
    /// <returns></returns>
    internal static RankingSettingsData BuildRankingSettingsBoundaryData(TournamentRuleData tournamentRuleData)
    {
        return new RankingSettingsData(
            tournamentRuleData.TournamentRuleSetMode ?? TournamentRuleSetMode.Neutral,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位設定データ");
    }

    // ========================================
    // 主線: TournamentFinalState -> FinalRanking
    // ========================================

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

    // ========================================
    // 大会品質レポート
    // ========================================

    /// <summary>
    /// ［大会品質評価レポート］組立
    /// </summary>
    /// <param name="qualityEvaluationRun"></param>
    /// <returns></returns>
    internal static TournamentQualityReportData BuildTournamentQualityReportBoundaryData(TournamentQualityReportRun qualityEvaluationRun)
    {
        return new TournamentQualityReportData(
            qualityEvaluationRun.PlayerRows,
            qualityEvaluationRun.Summary,
            qualityEvaluationRun.CalculationMode,
            qualityEvaluationRun.Suggestion);
    }

    /// <summary>
    /// ［大会品質評価レポート（スイープ）］組立
    /// </summary>
    /// <param name="sweepRows"></param>
    /// <param name="stoppedByTimeout"></param>
    /// <param name="suggestion"></param>
    /// <returns></returns>
    internal static TournamentQualitySweepReportData BuildTournamentQualitySweepReportBoundaryData(
        IReadOnlyList<TournamentQualitySweepReportRow> sweepRows,
        bool stoppedByTimeout,
        TournamentQualityNextCycleSuggestion suggestion)
    {
        return new TournamentQualitySweepReportData(sweepRows, stoppedByTimeout, suggestion);
    }
}
