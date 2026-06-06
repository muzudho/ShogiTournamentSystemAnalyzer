/*
 * ［アプリケーション　＞　実行　＞　分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

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
        if (SimulationRequestDispatcher.TryExecute(step)) return;
        if (QualityEvaluationRequestDispatcher.TryExecute(step)) return;

        throw new InvalidOperationException($"未対応の分析要求です: {step.GetType().Name}");
    }
}
