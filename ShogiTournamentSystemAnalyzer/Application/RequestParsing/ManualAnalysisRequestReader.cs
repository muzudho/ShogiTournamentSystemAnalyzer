/*
 * ［アプリケーション　＞　要求パース　＞　手入力分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class ManualAnalysisRequestReader
{
    internal static bool TryRead(
        AnalysisFlowSelection analysisFlowSelection,
        RuleProfileMode ruleProfileMode,
        out AnalysisRequest? analysisRequest)
    {
        analysisRequest = null;
        if (analysisFlowSelection.Steps.Count != 1) return false;

        AnalysisStepRequest stepRequest;
        if (analysisFlowSelection.Steps[0] == AnalysisFlowMode.Simulation)
        {
            if (!TryReadSimulationRequest(ruleProfileMode, out stepRequest)) return false;
        }
        else if (analysisFlowSelection.Steps[0] == AnalysisFlowMode.QualityEvaluation
            && (ruleProfileMode == RuleProfileMode.Standard || ruleProfileMode == RuleProfileMode.FinalStage))
        {
            if (!TryReadQualityEvaluationRequest(ruleProfileMode, out stepRequest)) return false;
        }
        else
        {
            return false;
        }

        analysisRequest = new AnalysisRequest(
            analysisFlowSelection,
            ruleProfileMode,
            new[] { stepRequest });
        return true;
    }
}
