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
}
