/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;

internal static partial class Program
{
    static void RunStandardMode()
    {
        SimulationScenarioRunner.Run(StandardSimulationScenario.Instance);
    }

    internal static void RunMainlineToFinalRanking(StandardModeContext context)
    {
        ExecuteStandardMode(context);
    }
}

