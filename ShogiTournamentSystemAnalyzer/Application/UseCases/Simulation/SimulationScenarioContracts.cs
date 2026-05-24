namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class Program
{
    interface ISimulationScenario
    {
        RuleProfileMode RuleProfileMode { get; }
        void Run();
    }

    static class SimulationScenarioRunner
    {
        internal static void Run(ISimulationScenario scenario)
        {
            scenario.Run();
        }
    }
}
