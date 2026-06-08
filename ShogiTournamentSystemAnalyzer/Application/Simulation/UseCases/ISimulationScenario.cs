/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal interface ISimulationScenario
{
    RuleProfileAttributes RuleProfileAttributes { get; }

    /// <summary>
    /// 概要表示
    /// </summary>
    void PrintOverview();


    bool TryPrepareExecution(out SimulationExecutionPlan plan);
}
