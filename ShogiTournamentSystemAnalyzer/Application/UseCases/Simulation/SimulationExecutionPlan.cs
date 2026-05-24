/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal readonly record struct SimulationExecutionPlan(
    RuleProfileMode RuleProfileMode,
    string ExecutionLabel,
    Action Execute);
