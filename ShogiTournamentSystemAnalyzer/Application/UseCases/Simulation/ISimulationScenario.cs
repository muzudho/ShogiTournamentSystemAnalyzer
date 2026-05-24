/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal interface ISimulationScenario
{
    RuleProfileMode RuleProfileMode { get; }
    void PrintOverview();
    bool TryPrepareExecution(out SimulationExecutionPlan plan);
}
