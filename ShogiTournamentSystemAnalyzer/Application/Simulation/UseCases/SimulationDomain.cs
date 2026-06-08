/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Application.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Simulation.SimulationMainline;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class SimulationDomain
{
    internal static bool TryExecute(AnalysisStepRequest step)
    {
        return TryExecute(step, out _);
    }

    internal static bool TryExecute(AnalysisStepRequest step, out SimulationDomainResult? result)
    {
        if (step is not SimulationStepRequest request)
        {
            result = null;
            return false;
        }

        result = ExecuteSimulation(request);
        return true;
    }

    static SimulationDomainResult? ExecuteSimulation(SimulationStepRequest request)
    {
        if (request.RuleProfileAttributes.IsTournamentFrameworkProfile)
        {
            return CreatePendingOnlyResult(ExecuteTournamentFrameworkSimulation(request));
        }

        if (request.RuleProfileAttributes.IsEmptyProfile)
        {
            return CreatePendingOnlyResult(SimulationEmptyMode.Run(request.OutputPath));
        }

        if (request.RuleProfileAttributes.UsesFinalStageGrouping)
        {
            return ExecuteFinalStageSimulation(request);
        }

        return ExecuteStandardSimulation(request);
    }

    static SimulationDomainResult ExecuteStandardSimulation(SimulationStepRequest request)
    {
        var input = request.ScheduledMatchesInput
            ?? throw new OperationCanceledException("通常シミュレーションの対局入力がありません。");
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new StandardModeSimulationContext(
            input.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            input.AllPlayers,
            input.Players,
            input.Matches);

        var mainline = new StandardSimulationMainline();
        var mainlineResult = mainline.Run(context, request.SimulationCount);
        return CreateResult(
            mainlineResult,
            new FinalRankingDomainInput(
                FinalRankingDomainInputKind.StandardSimulation,
                mainlineResult.SimulationResult.TournamentFinalState,
                context.FirstPlayerWinRatePercent,
                mainlineResult.FinalRankingResult,
                request.OutputPath,
                context.Players,
                Array.Empty<Match>(),
                WriteReferenceMatchesForMarkdown: false));
    }

    static SimulationDomainResult ExecuteFinalStageSimulation(SimulationStepRequest request)
    {
        var input = request.ScheduledMatchesInput
            ?? throw new OperationCanceledException("本戦シミュレーションの対局入力がありません。");
        var grouping = request.FinalStageGrouping
            ?? throw new OperationCanceledException("本戦シミュレーションのグループ入力がありません。");
        var additionalApexPlacement = request.AdditionalApexPlacement
            ?? new AdditionalApexPlacementRequest(Array.Empty<Player>(), AdditionalApexPlacementMode.Off, 0);
        var boundaryRescue = request.BoundaryRescue
            ?? new BoundaryRescueRequest(BoundaryRescueMode.Off);

        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new FinalStageModeSimulationContext(
            input.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            input.Players,
            grouping.GroupingMode,
            grouping.GroupMap,
            additionalApexPlacement.AdditionalApexPlayers,
            additionalApexPlacement.AdditionalApexPlacementMode,
            additionalApexPlacement.EffectiveAdditionalApexCount,
            boundaryRescue.BoundaryRescueMode,
            grouping.ApexCount,
            grouping.InnovCount,
            input.Matches,
            input.ReferenceMatches);

        var mainline = new FinalStageSimulationMainline();
        var mainlineResult = mainline.Run(context, request.SimulationCount);
        return CreateResult(
            mainlineResult,
            new FinalRankingDomainInput(
                FinalRankingDomainInputKind.FinalStageSimulation,
                mainlineResult.SimulationResult.TournamentFinalState,
                context.FirstPlayerWinRatePercent,
                mainlineResult.FinalRankingResult,
                request.OutputPath,
                context.Players,
                context.ReferenceMatches,
                WriteReferenceMatchesForMarkdown: mainlineResult.Presentation == SimulationMainlineResultPresentation.GroupedOverall));
    }

    static FinalRankingDomainInput ExecuteTournamentFrameworkSimulation(SimulationStepRequest request)
    {
        var input = request.TournamentFrameworkInput
            ?? throw new OperationCanceledException("大会進行フレームワークの入力がありません。");
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new TournamentFrameworkModeContext(
            input.PlayersCsvPath,
            input.StagesCsvPath,
            input.TournamentMatchRecordsCsvPath,
            input.RuleFilePath,
            input.RandomSeed,
            request.SimulationCount,
            input.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            request.OutputPath);

        return SimulationTournamentFrameworkMode.Run(context);
    }

    static SimulationDomainResult CreatePendingOnlyResult(FinalRankingDomainInput pendingFinalRankingInput)
    {
        return new SimulationDomainResult(
            null,
            null,
            pendingFinalRankingInput);
    }

    static SimulationDomainResult CreateResult(
        SimulationMainlineResult mainlineResult,
        FinalRankingDomainInput pendingFinalRankingInput)
    {
        return new SimulationDomainResult(
            mainlineResult.SimulationResult,
            mainlineResult.FinalRankingResult,
            pendingFinalRankingInput);
    }
}
