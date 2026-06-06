/*
 * ［アプリケーション　＞　要求パース　＞　分析ステップのルールプロファイル属性］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class AnalysisStepRuleProfileAttributes
{
    internal static RuleProfileAttributes GetRuleProfileAttributes(this AnalysisStepRequest step)
    {
        return step switch
        {
            StandardSimulationRequest request => CreateStandardAttributes(request.TournamentRuleSetMode),
            FinalStageSimulationRequest request => CreateFinalStageAttributes(
                request.TournamentRuleSetMode,
                request.AdditionalApexPlacementMode,
                request.BoundaryRescueMode,
                UsesVariableTop8: false,
                request.ReferenceMatches.Count > 0),
            TournamentFrameworkSimulationRequest request => RuleProfileAttributes.FromCompatibilityLabel(
                RuleProfileMode.TournamentFramework,
                request.TournamentRuleSetMode),
            EmptySimulationRequest => RuleProfileAttributes.FromCompatibilityLabel(RuleProfileMode.Empty),
            StandardQualityEvaluationRequest request => CreateAttributes(request.RuleDefinition, request.Input.ReferenceMatches.Count > 0),
            DeferredStandardQualityEvaluationRequest request => CreateStandardAttributes(request.TournamentRuleSetMode),
            FinalStageQualityEvaluationRequest request => CreateAttributes(request.RuleDefinition, request.Input.ReferenceMatches.Count > 0),
            DeferredFinalStageQualityEvaluationRequest request => CreateFinalStageAttributes(
                TournamentRuleSetMode.Neutral,
                AdditionalApexPlacementMode.On,
                BoundaryRescueMode.On,
                request.VariableTop8Mode == VariableTop8Mode.On,
                HasReferenceMatches: true),
            _ => throw new InvalidOperationException($"未対応の分析要求です: {step.GetType().Name}"),
        };
    }

    internal static RuleProfileMode GetCompatibilityRuleProfileMode(this AnalysisStepRequest step)
    {
        return step.GetRuleProfileAttributes().ToCompatibilityLabel();
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
            : CreateStandardAttributes(ruleDefinition.TournamentRuleSetMode, hasReferenceMatches);
    }

    static RuleProfileAttributes CreateStandardAttributes(
        TournamentRuleSetMode tournamentRuleSetMode,
        bool hasReferenceMatches = false)
    {
        return new RuleProfileAttributes(
            RuleProfileSimulationShape.ScheduledMatches,
            UsesFinalStageGrouping: false,
            UsesAdditionalApexPlacement: false,
            UsesBoundaryRescue: false,
            UsesVariableTop8: false,
            tournamentRuleSetMode,
            hasReferenceMatches,
            RuleProfilePairingSource.ScheduledMatches);
    }

    static RuleProfileAttributes CreateFinalStageAttributes(
        TournamentRuleSetMode tournamentRuleSetMode,
        AdditionalApexPlacementMode additionalApexPlacementMode,
        BoundaryRescueMode boundaryRescueMode,
        bool UsesVariableTop8,
        bool HasReferenceMatches)
    {
        return new RuleProfileAttributes(
            RuleProfileSimulationShape.FinalStageGrouped,
            UsesFinalStageGrouping: true,
            additionalApexPlacementMode == AdditionalApexPlacementMode.On,
            boundaryRescueMode == BoundaryRescueMode.On,
            UsesVariableTop8,
            tournamentRuleSetMode,
            HasReferenceMatches,
            RuleProfilePairingSource.ScheduledMatches);
    }
}
