/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal interface ISimulationScenario
{
    RuleProfileMode RuleProfileMode { get; }

    /// <summary>
    /// 概要表示
    /// </summary>
    void PrintOverview();


    bool TryPrepareExecution(out SimulationExecutionPlan plan);
}
