internal static partial class Program
{
    static TournamentRuleData BuildTournamentRuleData(TournamentFrameworkModeContext context, TournamentDslDefinition? dslDefinition)
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

    static PlayerListData BuildPlayerListData(IReadOnlyList<PlayerEntry> players)
    {
        return new PlayerListData(players);
    }

    static RankingSettingsData BuildRankingSettingsData(TournamentRuleData tournamentRuleData)
    {
        return new RankingSettingsData(
            tournamentRuleData.TournamentRuleSetMode ?? TournamentRuleSetMode.Neutral,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位設定データ");
    }

    static TournamentResultData BuildTournamentResultData(TournamentFrameworkExecutionResult executionResult)
    {
        return new TournamentResultData(
            executionResult.FinalState.MatchRecords,
            executionResult.FinalState.CurrentTime,
            executionResult.TickCount,
            executionResult.CompletedNaturally);
    }

    static FinalRankingData BuildFinalRankingData(TournamentFrameworkExecutionResult executionResult)
    {
        return new FinalRankingData(
            executionResult.OverallRanking,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位データ");
    }

    static TournamentQualityReportData BuildTournamentQualityReportData(QualityEvaluationRun qualityEvaluationRun)
    {
        return new TournamentQualityReportData(qualityEvaluationRun);
    }
}
