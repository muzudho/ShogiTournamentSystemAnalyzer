/*
 * ［アプリケーション　＞　実行　＞　品質評価フロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class QualityEvaluationFlowDispatcher
{
    internal static bool TryExecute(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        if (flowMode != AnalysisFlowMode.QualityEvaluation) return false;

        switch (ruleProfileMode)
        {
            case RuleProfileMode.Standard:
            case RuleProfileMode.FinalStage:
                TournamentQualityEvaluationMode.Run(ruleProfileMode);
                return true;

            default:
                return false;
        }
    }
}
