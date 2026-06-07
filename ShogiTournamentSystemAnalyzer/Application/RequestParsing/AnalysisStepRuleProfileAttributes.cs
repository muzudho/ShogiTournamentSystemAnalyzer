/*
 * ［アプリケーション　＞　要求パース　＞　分析ステップのルールプロファイル属性］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class AnalysisStepRuleProfileAttributes
{
    internal static RuleProfileAttributes GetRuleProfileAttributes(this AnalysisStepRequest stepRequest)
    {
        return stepRequest switch
        {
            SimulationStepRequest request => request.RuleProfileAttributes,
            QualityEvaluationStepRequest request => request.RuleProfileAttributes,
            DeferredQualityEvaluationStepRequest request => request.RuleProfileAttributes,
            _ => throw new InvalidOperationException($"未対応の分析要求です: {stepRequest.GetType().Name}"),
        };
    }
}