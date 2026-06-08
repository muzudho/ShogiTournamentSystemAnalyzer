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

        if (request.SimulationDomainRequest is not null)
        {
            ExecuteSimulationDomain(request.SimulationDomainRequest.StepRequest, context);
        }

        if (request.FinalRankingDomainRequest is not null)
        {
            ExecuteFinalRankingDomain(context);
        }

        if (request.QualityEvaluationDomainRequest is not null)
        {
            ExecuteQualityEvaluationDomain(request.QualityEvaluationDomainRequest.StepRequest, context);
        }
    }

    static void ExecuteSimulationDomain(AnalysisStepRequest stepRequest, AnalysisExecutionContext context)
    {
        if (!SimulationRequestDispatcher.TryExecute(stepRequest, out var simulationResult))
        {
            throw new InvalidOperationException($"未対応のシミュレーション域要求です: {stepRequest.GetType().Name}");
        }

        context.SetSimulationResult(stepRequest, simulationResult);
    }

    static void ExecuteFinalRankingDomain(AnalysisExecutionContext context)
    {
        if (!FinalRankingRequestDispatcher.TryExecute(context))
        {
            throw new InvalidOperationException("最終順位付け域へ渡すシミュレーション結果がありません。");
        }
    }

    static void ExecuteQualityEvaluationDomain(AnalysisStepRequest stepRequest, AnalysisExecutionContext context)
    {
        if (QualityEvaluationRequestDispatcher.TryExecute(stepRequest, context)) return;

        throw new InvalidOperationException($"未対応の大会品質評価域要求です: {stepRequest.GetType().Name}");
    }
}
