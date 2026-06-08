/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

internal readonly record struct SimulationExecutionPlan(
    string ExecutionLabel,
    Func<SimulationDomainResult?> Execute);
