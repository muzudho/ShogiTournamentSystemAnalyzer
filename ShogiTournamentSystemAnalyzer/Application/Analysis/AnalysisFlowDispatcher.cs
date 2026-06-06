/*
 * ［アプリケーション　＞　実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class AnalysisFlowDispatcher
{
    internal static void Execute(AnalysisFlowSelection flowSelection, RuleProfileMode ruleProfileMode)
    {
        foreach (var flowMode in flowSelection.Steps)
        {
            ExecuteSingle(flowMode, ruleProfileMode);
        }
    }

    static void ExecuteSingle(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        if (SimulationFlowDispatcher.TryExecute(flowMode, ruleProfileMode)) return;
        if (QualityEvaluationFlowDispatcher.TryExecute(flowMode, ruleProfileMode)) return;

        throw new InvalidOperationException("未対応のモードです。");
    }
}
