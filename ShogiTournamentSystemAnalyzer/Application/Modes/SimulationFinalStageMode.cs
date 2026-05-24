/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static void RunFinalStageMode()
    {
        SimulationScenarioRunner.Run(FinalStageSimulationScenario.Instance);
    }

    static void RunMainlineToFinalRanking(FinalStageModeContext context)
    {
        var result = ExecuteTournamentFinalStateAndFinalRanking(context, out var standardResultRows, out var finalStageResultRows);
        PrintFinalStageModeContext(context);
        WriteFinalRankingOutputsForFinalStageMode(context, result, standardResultRows, finalStageResultRows);
    }
}

