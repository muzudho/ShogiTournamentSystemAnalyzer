/*
 * ［アプリケーション　＞　実行　＞　分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal static class AnalysisRequestDispatcher
{
    internal static void Execute(AnalysisRequest request)
    {
        var context = new AnalysisExecutionContext();

        foreach (var step in request.Steps)
        {
            ExecuteSingle(step, context);
        }
    }

    static void ExecuteSingle(AnalysisStepRequest step, AnalysisExecutionContext context)
    {
        if (SimulationRequestDispatcher.TryExecute(step, out var simulationResult))
        {
            context.SetSimulationResult(step, simulationResult);
            return;
        }

        if (QualityEvaluationRequestDispatcher.TryExecute(step, context)) return;

        throw new InvalidOperationException($"未対応の分析要求です: {step.GetType().Name}");
    }
}
