/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.UseCases;

internal readonly record struct SimulationExecutionPlan(
    string ExecutionLabel,
    Func<SimulationDomainResult?> Execute);
