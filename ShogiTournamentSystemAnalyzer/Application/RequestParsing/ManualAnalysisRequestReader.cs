/*
 * ［アプリケーション　＞　要求パース　＞　手入力分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class ManualAnalysisRequestReader
{
    internal static bool TryRead(
        AnalysisFlowSelection analysisFlowSelection,
        RuleProfileAttributes ruleProfileAttributes,
        out AnalysisRequest? analysisRequest)
    {
        analysisRequest = null;
        if (analysisFlowSelection.RunsSimulationDomain && analysisFlowSelection.RunsQualityEvaluationDomain) return false;

        AnalysisStepRequest stepRequest;
        if (analysisFlowSelection.RunsSimulationDomain)
        {
            if (!TryReadSimulationRequest(ruleProfileAttributes, out stepRequest)) return false;
        }
        else if (analysisFlowSelection.RunsQualityEvaluationDomain
            && ruleProfileAttributes.PairingSource == RuleProfilePairingSource.ScheduledMatches)
        {
            if (!TryReadQualityEvaluationRequest(ruleProfileAttributes, out stepRequest)) return false;
        }
        else
        {
            return false;
        }

        analysisRequest = new AnalysisRequest(
            analysisFlowSelection,
            new[] { stepRequest });
        return true;
    }
}
