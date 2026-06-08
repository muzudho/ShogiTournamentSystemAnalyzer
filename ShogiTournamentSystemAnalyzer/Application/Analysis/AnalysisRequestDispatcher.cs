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

        // 要求ファイルから読んだ具体要求は、3大域プロパティから実行順に復元して扱う。
        foreach (var requestedStep in request.GetExecutableAnalysisSteps())
        {
            ExecuteSingle(requestedStep, context);
        }
    }

    static void ExecuteSingle(AnalysisStepRequest stepRequest, AnalysisExecutionContext context)
    {
        if (SimulationRequestDispatcher.TryExecute(stepRequest, out var simulationResult))
        {
            context.SetSimulationResult(stepRequest, simulationResult);
            return;
        }

        if (QualityEvaluationRequestDispatcher.TryExecute(stepRequest, context)) return;

        throw new InvalidOperationException($"未対応の分析要求です: {stepRequest.GetType().Name}");
    }
}
