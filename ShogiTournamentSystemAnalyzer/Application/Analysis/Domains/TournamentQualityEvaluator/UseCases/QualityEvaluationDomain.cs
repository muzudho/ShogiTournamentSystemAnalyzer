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
            case QualityEvaluationStepRequest request:
                ExecuteQualityEvaluation(request);
                return true;

            case DeferredQualityEvaluationStepRequest request:
                ExecuteQualityEvaluation(CreateQualityEvaluationRequest(request, context));
                return true;

            default:
                return false;
        }
    }

    static void ExecuteQualityEvaluation(QualityEvaluationStepRequest request)
    {
        TournamentQualityEvaluationMainline.Run(
            request.Input,
            request.RuleDefinition,
            request.ExecutionOptions,
            request.OutputOptions);
    }

    static QualityEvaluationStepRequest CreateQualityEvaluationRequest(
        DeferredQualityEvaluationStepRequest request,
        AnalysisExecutionContext? context)
    {
        if (context?.LastSimulationRequest is not SimulationStepRequest simulationRequest)
        {
            throw new OperationCanceledException("入力省略の品質評価は、直前にシミュレーションが実行されている場合だけ実行できます。");
        }

        var scheduledInput = simulationRequest.ScheduledMatchesInput
            ?? throw new OperationCanceledException("入力省略の品質評価は、直前のシミュレーションに対局入力がある場合だけ実行できます。");

        return request.RuleProfileAttributes.UsesFinalStageGrouping
            ? CreateGroupedQualityEvaluationStepRequest(request, simulationRequest, scheduledInput)
            : CreateUngroupedQualityEvaluationStepRequest(request, scheduledInput);
    }

    static QualityEvaluationStepRequest CreateUngroupedQualityEvaluationStepRequest(
        DeferredQualityEvaluationStepRequest request,
        ScheduledMatchesSimulationInput simulationInput)
    {
        var ruleDefinition = new TournamentQualityEvaluationRuleDefinition(
            FinalStageGroupingMode.Off,
            simulationInput.TournamentRuleSetMode,
            null,
            Array.Empty<Player>(),
            AdditionalApexPlacementMode.Off,
            0,
            BoundaryRescueMode.Off,
            VariableTop8Mode.Off,
            0);
        var input = new TournamentQualityEvaluationInput(
            simulationInput.Players,
            simulationInput.Matches,
            Array.Empty<Match>(),
            TournamentQualityEvaluationInnovExpectedRankOffsetMode.Off,
            0);

        return new QualityEvaluationStepRequest(
            request.RuleProfileAttributes,
            ruleDefinition,
            input,
            EnsureSimulationCountIfNeeded(request.ExecutionOptions, input.Matches.Count),
            request.OutputOptions);
    }

    static QualityEvaluationStepRequest CreateGroupedQualityEvaluationStepRequest(
        DeferredQualityEvaluationStepRequest request,
        SimulationStepRequest simulationRequest,
        ScheduledMatchesSimulationInput simulationInput)
    {
        var grouping = simulationRequest.FinalStageGrouping
            ?? throw new OperationCanceledException("入力省略の本戦品質評価は、直前のシミュレーションにグループ入力がある場合だけ実行できます。");
        var additionalApexPlacement = simulationRequest.AdditionalApexPlacement
            ?? new AdditionalApexPlacementRequest(Array.Empty<Player>(), AdditionalApexPlacementMode.Off, 0);
        var boundaryRescue = simulationRequest.BoundaryRescue
            ?? new BoundaryRescueRequest(BoundaryRescueMode.Off);

        var variableTop8Mode = request.DeferredOptions.VariableTop8Mode;
        var innovExpectedRankOffsetMode = request.DeferredOptions.InnovExpectedRankOffsetMode;
        var ruleDefinition = new TournamentQualityEvaluationRuleDefinition(
            grouping.GroupingMode,
            simulationInput.TournamentRuleSetMode,
            grouping.GroupMap,
            additionalApexPlacement.AdditionalApexPlayers,
            additionalApexPlacement.AdditionalApexPlacementMode,
            additionalApexPlacement.EffectiveAdditionalApexCount,
            boundaryRescue.BoundaryRescueMode,
            variableTop8Mode,
            VariableTop8Rule.GetPromotedInnovCount(variableTop8Mode, additionalApexPlacement.AdditionalApexPlayers.Count));
        var input = new TournamentQualityEvaluationInput(
            simulationInput.Players,
            simulationInput.Matches,
            simulationInput.ReferenceMatches,
            innovExpectedRankOffsetMode,
            TournamentQualityEvaluationInnovExpectedRankOffsetRule.GetComparisonRankOffset(
                additionalApexPlacement.EffectiveAdditionalApexCount,
                innovExpectedRankOffsetMode));

        return new QualityEvaluationStepRequest(
            request.RuleProfileAttributes,
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