/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal readonly record struct SimulationExecutionPlan(
    RuleProfileMode RuleProfileMode,
    string ExecutionLabel,
    Action Execute);
