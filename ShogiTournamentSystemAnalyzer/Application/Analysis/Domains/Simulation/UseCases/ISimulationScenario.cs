/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

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
