using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static partial class Program
{
    static TournamentRuleData BuildTournamentRuleBoundaryData(TournamentFrameworkModeContext context, TournamentDslDefinition? dslDefinition)
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

    static PlayerListData BuildPlayerListBoundaryData(IReadOnlyList<PlayerEntry> players)
    {
        return new PlayerListData(players);
    }

    static RankingSettingsData BuildRankingSettingsBoundaryData(TournamentRuleData tournamentRuleData)
    {
        return new RankingSettingsData(
            tournamentRuleData.TournamentRuleSetMode ?? TournamentRuleSetMode.Neutral,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位設定データ");
    }

    static TournamentResultData BuildTournamentResultBoundaryData(TournamentFrameworkExecutionResult executionResult)
    {
        return new TournamentResultData(
            executionResult.FinalState.MatchRecords,
            executionResult.FinalState.CurrentTime,
            executionResult.TickCount,
            executionResult.CompletedNaturally);
    }

    static FinalRankingData BuildFinalRankingBoundaryData(TournamentFrameworkExecutionResult executionResult)
    {
        return new FinalRankingData(
            executionResult.OverallRanking,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位データ");
    }

    static TournamentQualityReportData BuildTournamentQualityReportBoundaryData(TournamentQualityReportRun qualityEvaluationRun)
    {
        return new TournamentQualityReportData(
            qualityEvaluationRun.PlayerRows,
            qualityEvaluationRun.Summary,
            qualityEvaluationRun.CalculationMode);
    }

    static TournamentQualitySweepReportData BuildTournamentQualitySweepReportBoundaryData(
        IReadOnlyList<TournamentQualitySweepReportRow> sweepRows,
        bool stoppedByTimeout)
    {
        return new TournamentQualitySweepReportData(sweepRows, stoppedByTimeout);
    }
}
