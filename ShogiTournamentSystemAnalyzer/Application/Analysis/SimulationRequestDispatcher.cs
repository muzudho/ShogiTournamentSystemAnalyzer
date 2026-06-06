/*
 * ［アプリケーション　＞　実行　＞　シミュレーション要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal static class SimulationRequestDispatcher
{
    internal static bool TryExecute(AnalysisStepRequest step)
    {
        switch (step)
        {
            case StandardSimulationRequest standardSimulationRequest:
                ExecuteStandardSimulation(standardSimulationRequest);
                return true;

            case FinalStageSimulationRequest finalStageSimulationRequest:
                ExecuteFinalStageSimulation(finalStageSimulationRequest);
                return true;

            case TournamentFrameworkSimulationRequest tournamentFrameworkSimulationRequest:
                ExecuteTournamentFrameworkSimulation(tournamentFrameworkSimulationRequest);
                return true;

            case EmptySimulationRequest emptySimulationRequest:
                ExecuteEmptySimulation(emptySimulationRequest);
                return true;

            default:
                return false;
        }
    }

    static void ExecuteStandardSimulation(StandardSimulationRequest request)
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
        mainline.Run(context, request.OutputPath, request.SimulationCount);
    }

    static void ExecuteFinalStageSimulation(FinalStageSimulationRequest request)
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
        mainline.Run(context, request.OutputPath, request.SimulationCount);
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
}
