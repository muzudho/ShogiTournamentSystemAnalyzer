/*
 * ［アプリケーション　＞　ユースケース　＞　大会品質評価域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal static class QualityEvaluationDomain
{
    internal static bool TryExecute(AnalysisStepRequest step)
    {
        switch (step)
        {
            case StandardQualityEvaluationRequest standardQualityEvaluationRequest:
                ExecuteStandardQualityEvaluation(standardQualityEvaluationRequest);
                return true;

            case FinalStageQualityEvaluationRequest finalStageQualityEvaluationRequest:
                ExecuteFinalStageQualityEvaluation(finalStageQualityEvaluationRequest);
                return true;

            default:
                return false;
        }
    }

    static void ExecuteStandardQualityEvaluation(StandardQualityEvaluationRequest request)
    {
        TournamentQualityEvaluationMainline.Run(
            request.Input,
            request.RuleDefinition,
            request.ExecutionOptions,
            request.OutputOptions);
    }

    static void ExecuteFinalStageQualityEvaluation(FinalStageQualityEvaluationRequest request)
    {
        TournamentQualityEvaluationMainline.Run(
            request.Input,
            request.RuleDefinition,
            request.ExecutionOptions,
            request.OutputOptions);
    }
}
