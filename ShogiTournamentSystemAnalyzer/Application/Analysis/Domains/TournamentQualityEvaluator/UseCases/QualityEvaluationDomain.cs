/*
 * ［アプリケーション　＞　ユースケース　＞　大会品質評価域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using Match = ShogiTournamentSystemAnalyzer.Domain.Simulation.Match;

internal static class QualityEvaluationDomain
{
    const int ExactCalculationMatchThreshold = 20;
    const int DefaultApproximationSimulationCount = 200_000;

    internal static bool TryExecute(AnalysisStepRequest step)
    {
        return TryExecute(step, context: null);
    }

    internal static bool TryExecute(AnalysisStepRequest step, AnalysisExecutionContext? context)
    {
        switch (step)
        {
            case StandardQualityEvaluationRequest standardQualityEvaluationRequest:
                ExecuteStandardQualityEvaluation(standardQualityEvaluationRequest);
                return true;

            case DeferredStandardQualityEvaluationRequest deferredStandardQualityEvaluationRequest:
                ExecuteStandardQualityEvaluation(CreateStandardQualityEvaluationRequest(deferredStandardQualityEvaluationRequest, context));
                return true;

            case FinalStageQualityEvaluationRequest finalStageQualityEvaluationRequest:
                ExecuteFinalStageQualityEvaluation(finalStageQualityEvaluationRequest);
                return true;

            case DeferredFinalStageQualityEvaluationRequest deferredFinalStageQualityEvaluationRequest:
                ExecuteFinalStageQualityEvaluation(CreateFinalStageQualityEvaluationRequest(deferredFinalStageQualityEvaluationRequest, context));
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

    static StandardQualityEvaluationRequest CreateStandardQualityEvaluationRequest(
        DeferredStandardQualityEvaluationRequest request,
        AnalysisExecutionContext? context)
    {
        if (context?.LastSimulationRequest is not StandardSimulationRequest simulationRequest)
        {
            throw new OperationCanceledException("入力省略の標準品質評価は、直前に標準シミュレーションが実行されている場合だけ実行できます。");
        }

        var ruleDefinition = new TournamentQualityEvaluationRuleDefinition(
            FinalStageGroupingMode.Off,
            request.TournamentRuleSetMode,
            null,
            Array.Empty<Player>(),
            AdditionalApexPlacementMode.Off,
            0,
            BoundaryRescueMode.Off,
            VariableTop8Mode.Off,
            0);
        var input = new TournamentQualityEvaluationInput(
            simulationRequest.Players,
            simulationRequest.Matches,
            Array.Empty<Match>(),
            TournamentQualityEvaluationInnovExpectedRankOffsetMode.Off,
            0);

        return new StandardQualityEvaluationRequest(
            ruleDefinition,
            input,
            EnsureSimulationCountIfNeeded(request.ExecutionOptions, input.Matches.Count),
            request.OutputOptions);
    }

    static FinalStageQualityEvaluationRequest CreateFinalStageQualityEvaluationRequest(
        DeferredFinalStageQualityEvaluationRequest request,
        AnalysisExecutionContext? context)
    {
        if (context?.LastSimulationRequest is not FinalStageSimulationRequest simulationRequest)
        {
            throw new OperationCanceledException("入力省略の本戦品質評価は、直前に本戦シミュレーションが実行されている場合だけ実行できます。");
        }

        var ruleDefinition = new TournamentQualityEvaluationRuleDefinition(
            FinalStageGroupingMode.On,
            TournamentRuleSetMode.Neutral,
            simulationRequest.GroupMap,
            simulationRequest.AdditionalApexPlayers,
            simulationRequest.AdditionalApexPlacementMode,
            simulationRequest.EffectiveAdditionalApexCount,
            simulationRequest.BoundaryRescueMode,
            request.VariableTop8Mode,
            VariableTop8Rule.GetPromotedInnovCount(request.VariableTop8Mode, simulationRequest.AdditionalApexPlayers.Count));
        var input = new TournamentQualityEvaluationInput(
            simulationRequest.Players,
            simulationRequest.Matches,
            simulationRequest.ReferenceMatches,
            request.InnovExpectedRankOffsetMode,
            TournamentQualityEvaluationInnovExpectedRankOffsetRule.GetComparisonRankOffset(
                simulationRequest.EffectiveAdditionalApexCount,
                request.InnovExpectedRankOffsetMode));

        return new FinalStageQualityEvaluationRequest(
            ruleDefinition,
            input,
            EnsureSimulationCountIfNeeded(request.ExecutionOptions, input.Matches.Count),
            request.OutputOptions);
    }

    static TournamentQualityEvaluationExecutionOptions EnsureSimulationCountIfNeeded(
        TournamentQualityEvaluationExecutionOptions executionOptions,
        int matchCount)
    {
        if (matchCount <= ExactCalculationMatchThreshold || executionOptions.SimulationCount.HasValue) return executionOptions;

        return executionOptions with { SimulationCount = DefaultApproximationSimulationCount };
    }
}
