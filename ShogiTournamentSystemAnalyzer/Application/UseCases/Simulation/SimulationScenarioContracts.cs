namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class Program
{
    interface ISimulationScenario
    {
        RuleProfileMode RuleProfileMode { get; }
        void PrintOverview();
        bool TryPrepareExecution(out Action execute);
    }

    static class SimulationScenarioRunner
    {
        internal static void Run(ISimulationScenario scenario)
        {
            scenario.PrintOverview();
            if (!scenario.TryPrepareExecution(out var execute)) return;

            execute();
        }
    }
}
