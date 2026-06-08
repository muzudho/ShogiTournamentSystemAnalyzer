/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// ［シミュレーション］の結果だ。
/// </summary>
/// <param name="TournamentFinalState">大会最終状態の計算結果。</param>
internal sealed record class SimulationResult(CalculationResult TournamentFinalState);
