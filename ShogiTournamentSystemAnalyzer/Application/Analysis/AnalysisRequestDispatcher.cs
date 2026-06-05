/*
 * ［アプリケーション　＞　実行　＞　分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal static class AnalysisRequestDispatcher
{
    internal static void Execute(AnalysisRequest request)
    {
        foreach (var step in request.Steps)
        {
            ExecuteSingle(step);
        }
    }

    static void ExecuteSingle(AnalysisStepRequest step)
    {
        switch (step)
        {
            case StandardSimulationRequest standardSimulationRequest:
                ExecuteStandardSimulation(standardSimulationRequest);
                break;

            case StandardQualityEvaluationRequest standardQualityEvaluationRequest:
                ExecuteStandardQualityEvaluation(standardQualityEvaluationRequest);
                break;

            case FinalStageSimulationRequest finalStageSimulationRequest:
                ExecuteFinalStageSimulation(finalStageSimulationRequest);
                break;

            case FinalStageQualityEvaluationRequest finalStageQualityEvaluationRequest:
                ExecuteFinalStageQualityEvaluation(finalStageQualityEvaluationRequest);
                break;

            default:
                throw new InvalidOperationException($"未対応の分析要求です: {step.GetType().Name}");
        }
    }

    static void ExecuteStandardSimulation(StandardSimulationRequest request)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new StandardModeSimulationContext(
            request.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            request.AllPlayers,
            request.Players,
            request.Matches);

        var mainline = new StandardSimulationMainline();
        mainline.Run(context, request.OutputPath);
    }

    static void ExecuteStandardQualityEvaluation(StandardQualityEvaluationRequest request)
    {
        TournamentQualityEvaluationMainline.Run(
            request.Input,
            request.RuleDefinition,
            request.ExecutionOptions,
            request.OutputOptions);
    }

    static void ExecuteFinalStageSimulation(FinalStageSimulationRequest request)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new FinalStageModeSimulationContext(
            request.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            request.Players,
            request.GroupingMode,
            request.GroupMap,
            request.AdditionalApexPlayers,
            request.AdditionalApexPlacementMode,
            request.EffectiveAdditionalApexCount,
            request.BoundaryRescueMode,
            request.ApexCount,
            request.InnovCount,
            request.Matches,
            request.ReferenceMatches);

        var mainline = new FinalStageSimulationMainline();
        mainline.Run(context, request.OutputPath);
    }

    static void ExecuteFinalStageQualityEvaluation(FinalStageQualityEvaluationRequest request)
    {
        TournamentQualityEvaluationMainline.Run(
            request.Input,
            request.RuleDefinition,
            request.ExecutionOptions,
            request.OutputOptions);
    }
}
