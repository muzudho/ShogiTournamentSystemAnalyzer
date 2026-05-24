/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;

internal static partial class Program
{
    static void RunFinalStageMode()
    {
        SimulationScenarioRunner.Run(FinalStageSimulationScenario.Instance);
    }

    internal static void RunMainlineToFinalRanking(FinalStageModeContext context)
    {
        var result = ExecuteTournamentFinalStateAndFinalRanking(context, out var standardResultRows, out var finalStageResultRows);
        PrintFinalStageModeContext(context);
        WriteFinalRankingOutputsForFinalStageMode(context, result, standardResultRows, finalStageResultRows);
    }
}

