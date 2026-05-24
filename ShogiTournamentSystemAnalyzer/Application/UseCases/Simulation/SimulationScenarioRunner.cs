/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;

internal static class SimulationScenarioRunner
{
    internal static void Run(ISimulationScenario scenario)
    {
        scenario.PrintOverview();
        if (!scenario.TryPrepareExecution(out var plan)) return;

        plan.Execute();
    }
}
