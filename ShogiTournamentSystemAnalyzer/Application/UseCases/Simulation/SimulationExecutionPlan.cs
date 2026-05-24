/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal readonly record struct SimulationExecutionPlan(
    RuleProfileMode RuleProfileMode,
    string ExecutionLabel,
    Action Execute);
