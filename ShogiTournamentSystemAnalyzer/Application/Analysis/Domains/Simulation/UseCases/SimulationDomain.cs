/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal static class SimulationDomain
{
    internal static bool TryExecute(AnalysisStepRequest step)
    {
        return TryExecute(step, out _);
    }

    internal static bool TryExecute(AnalysisStepRequest step, out SimulationDomainResult? result)
    {
        switch (step)
        {
            case StandardSimulationRequest standardSimulationRequest:
                result = ExecuteStandardSimulation(standardSimulationRequest);
                return true;

            case FinalStageSimulationRequest finalStageSimulationRequest:
                result = ExecuteFinalStageSimulation(finalStageSimulationRequest);
                return true;

            case TournamentFrameworkSimulationRequest tournamentFrameworkSimulationRequest:
                ExecuteTournamentFrameworkSimulation(tournamentFrameworkSimulationRequest);
                result = null;
                return true;

            case EmptySimulationRequest emptySimulationRequest:
                ExecuteEmptySimulation(emptySimulationRequest);
                result = null;
                return true;

            default:
                result = null;
                return false;
        }
    }

    static SimulationDomainResult ExecuteStandardSimulation(StandardSimulationRequest request)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new StandardModeSimulationContext(
            request.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            request.AllPlayers,
            request.Players,
            request.Matches);

        var mainline = new StandardSimulationMainline();
        var mainlineResult = mainline.Run(context, request.SimulationCount);
        FinalRankingDomain.WriteStandardSimulationOutputs(
            mainlineResult.SimulationResult.TournamentFinalState,
            context.FirstPlayerWinRatePercent,
            mainlineResult.FinalRankingResult,
            request.OutputPath);
        return CreateResult(mainlineResult);
    }

    static SimulationDomainResult ExecuteFinalStageSimulation(FinalStageSimulationRequest request)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new FinalStageModeSimulationContext(
            request.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            request.Players,
            request.GroupingMode,
            request.GroupMap,
            request.AdditionalApexPlayers,
            request.AdditionalApexPlacementMode,
            request.EffectiveAdditionalApexCount,
            request.BoundaryRescueMode,
            request.ApexCount,
            request.InnovCount,
            request.Matches,
            request.ReferenceMatches);

        var mainline = new FinalStageSimulationMainline();
        var mainlineResult = mainline.Run(context, request.SimulationCount);
        FinalRankingDomain.WriteFinalStageSimulationOutputs(
            mainlineResult.SimulationResult.TournamentFinalState,
            context.FirstPlayerWinRatePercent,
            mainlineResult.FinalRankingResult,
            request.OutputPath,
            context.Players,
            context.ReferenceMatches,
            writeReferenceMatchesForMarkdown: mainlineResult.Presentation == SimulationMainlineResultPresentation.GroupedOverall);
        return CreateResult(mainlineResult);
    }

    static void ExecuteTournamentFrameworkSimulation(TournamentFrameworkSimulationRequest request)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(request.FirstPlayerWinRatePercent);
        var context = new TournamentFrameworkModeContext(
            request.PlayersCsvPath,
            request.StagesCsvPath,
            request.TournamentMatchRecordsCsvPath,
            request.RuleFilePath,
            request.RandomSeed,
            request.SimulationCount,
            request.TournamentRuleSetMode,
            request.FirstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            request.OutputPath);

        SimulationTournamentFrameworkMode.Run(context);
    }

    static void ExecuteEmptySimulation(EmptySimulationRequest request)
    {
        SimulationEmptyMode.Run(request.OutputPath);
    }

    static SimulationDomainResult CreateResult(SimulationMainlineResult mainlineResult)
    {
        return new SimulationDomainResult(mainlineResult.SimulationResult, mainlineResult.FinalRankingResult);
    }
}
