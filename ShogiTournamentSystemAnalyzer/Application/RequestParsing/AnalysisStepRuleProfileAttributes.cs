/*
 * ［アプリケーション　＞　要求パース　＞　分析ステップのルールプロファイル属性］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class AnalysisStepRuleProfileAttributes
{
    internal static RuleProfileAttributes GetRuleProfileAttributes(this AnalysisStepRequest stepRequest)
    {
        return stepRequest switch
        {
            StandardSimulationRequest request => RuleProfileAttributes.CreateStandardScheduled(request.TournamentRuleSetMode),
            FinalStageSimulationRequest request => CreateFinalStageAttributes(
                request.TournamentRuleSetMode,
                request.AdditionalApexPlacementMode,
                request.BoundaryRescueMode,
                UsesVariableTop8: false,
                request.ReferenceMatches.Count > 0),
            TournamentFrameworkSimulationRequest request => RuleProfileAttributes.CreateTournamentFramework(request.TournamentRuleSetMode),
            EmptySimulationRequest => RuleProfileAttributes.CreateEmpty(),
            StandardQualityEvaluationRequest request => CreateAttributes(request.RuleDefinition, request.Input.ReferenceMatches.Count > 0),
            DeferredStandardQualityEvaluationRequest request => RuleProfileAttributes.CreateStandardScheduled(request.TournamentRuleSetMode),
            FinalStageQualityEvaluationRequest request => CreateAttributes(request.RuleDefinition, request.Input.ReferenceMatches.Count > 0),
            DeferredFinalStageQualityEvaluationRequest request => CreateFinalStageAttributes(
                TournamentRuleSetMode.Neutral,
                AdditionalApexPlacementMode.On,
                BoundaryRescueMode.On,
                request.VariableTop8Mode == VariableTop8Mode.On,
                HasReferenceMatches: true),
            _ => throw new InvalidOperationException($"未対応の分析要求です: {stepRequest.GetType().Name}"),
        };
    }


    static RuleProfileAttributes CreateAttributes(
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        bool hasReferenceMatches)
    {
        return ruleDefinition.UsesFinalStageGrouping
            ? CreateFinalStageAttributes(
                ruleDefinition.TournamentRuleSetMode,
                ruleDefinition.AdditionalApexPlacementMode,
                ruleDefinition.BoundaryRescueMode,
                ruleDefinition.VariableTop8Mode == VariableTop8Mode.On,
                hasReferenceMatches)
            : RuleProfileAttributes.CreateStandardScheduled(ruleDefinition.TournamentRuleSetMode, hasReferenceMatches);
    }

    static RuleProfileAttributes CreateFinalStageAttributes(
        TournamentRuleSetMode tournamentRuleSetMode,
        AdditionalApexPlacementMode additionalApexPlacementMode,
        BoundaryRescueMode boundaryRescueMode,
        bool UsesVariableTop8,
        bool HasReferenceMatches)
    {
        return RuleProfileAttributes.CreateFinalStageGrouped(
            tournamentRuleSetMode,
            additionalApexPlacementMode == AdditionalApexPlacementMode.On,
            boundaryRescueMode == BoundaryRescueMode.On,
            UsesVariableTop8,
            HasReferenceMatches);
    }
}
