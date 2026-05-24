/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static void RunStandardMode()
    {
        SimulationScenarioRunner.Run(StandardSimulationScenario.Instance);
    }

    static void RunMainlineToFinalRanking(StandardModeContext context)
    {
        ExecuteStandardMode(context);
    }
}

