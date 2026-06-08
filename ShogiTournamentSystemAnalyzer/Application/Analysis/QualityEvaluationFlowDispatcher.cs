/*
 * ［アプリケーション　＞　実行　＞　品質評価フロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［大会品質評価域］
/// </summary>
internal static class QualityEvaluationFlowDispatcher
{
    internal static bool TryExecute(AnalysisFlowMode flowMode, RuleProfileAttributes ruleProfileAttributes)
    {
        if (flowMode != AnalysisFlowMode.QualityEvaluation) return false;

        if (ruleProfileAttributes.PairingSource != RuleProfilePairingSource.ScheduledMatches) return false;

        TournamentQualityEvaluationMode.Run(ruleProfileAttributes);
        return true;
    }
}
