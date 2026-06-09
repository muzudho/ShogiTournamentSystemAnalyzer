/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.TournamentQualityEvaluator.Modes;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;

internal static class TournamentQualityEvaluationSingleRunExecutor
{
    internal static TournamentQualityReportRun ExecuteRun(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityScoreRule scoreRule)
    {
        var firstPlayerWinRatePercent = executionOptions.FirstPlayerWinRatePercent!.Value;
        var tournamentFinalState = ExecuteTournamentFinalState(input, ruleDefinition, executionOptions, firstPlayerWinRatePercent);
        var finalRankingRows = BuildFinalRankingRows(input, tournamentFinalState, firstPlayerWinRatePercent);
        var qualityPlayerRows = TournamentQualityEvaluationReportBuilder.BuildTournamentQualityReportPlayerRows(
            finalRankingRows,
            ruleDefinition.GroupMap,
            ruleDefinition.AdditionalApexPlayers,
            ruleDefinition.AdditionalApexPlacementMode,
            input.InnovExpectedRankOffsetMode,
            input.InnovExpectedRankOffsetCount);
        var qualitySummary = TournamentQualityEvaluationReportBuilder.BuildTournamentQualityReportSummary(qualityPlayerRows, scoreRule);
        var calculationMode = qualityPlayerRows.Count == 0 && !tournamentFinalState.Mode.Contains("時間切れ", StringComparison.Ordinal)
            ? tournamentFinalState.Mode + " (0回)"
            : tournamentFinalState.Mode;
        var suggestion = TournamentQualityNextCycleSuggestionBuilder.BuildForSingleRun(input, executionOptions, tournamentFinalState.Mode.Contains("時間切れ", StringComparison.Ordinal), qualityPlayerRows.Count);
        return new TournamentQualityReportRun(qualityPlayerRows, qualitySummary, calculationMode, suggestion);
    }

    internal static TournamentQualityReportData ExecuteReport(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityScoreRule scoreRule)
    {
        var qualityEvaluationRun = ExecuteRun(input, ruleDefinition, executionOptions, scoreRule);
        return BoundaryDataBuilders.BuildTournamentQualityReportBoundaryData(qualityEvaluationRun);
    }

    static CalculationResult ExecuteTournamentFinalState(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        double firstPlayerWinRatePercent)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return ruleDefinition.GroupingMode == FinalStageGroupingMode.On
            ? executionOptions.SimulationCount.HasValue
                ? FinalStageCalculationEngine.CalculateFinalStageBySimulation(input.Players, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.PromotedInnovCount)
                : FinalStageCalculationEngine.CalculateFinalStageExactly(input.Players, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, firstPlayerWinRateRating, ruleDefinition.PromotedInnovCount)
            : executionOptions.SimulationCount.HasValue
                ? StandardCalculationEngine.CalculateBySimulation(input.Players, input.Matches, firstPlayerWinRateRating, executionOptions.SimulationCount.Value, ruleDefinition.TournamentRuleSetMode)
                : StandardCalculationEngine.CalculateExactly(input.Players, input.Matches, firstPlayerWinRateRating, ruleDefinition.TournamentRuleSetMode);
    }

    static IReadOnlyList<GeneralSimulationResultRow> BuildFinalRankingRows(
        TournamentQualityEvaluationInput input,
        CalculationResult tournamentFinalState,
        double firstPlayerWinRatePercent)
    {
        return RankingResultRowBuilder.BuildGeneralResultRows(input.Players, input.Matches, tournamentFinalState, firstPlayerWinRatePercent);
    }
}
