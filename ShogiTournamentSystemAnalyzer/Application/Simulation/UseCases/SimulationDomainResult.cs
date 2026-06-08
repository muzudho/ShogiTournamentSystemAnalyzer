/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域結果］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Application.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal sealed record class SimulationDomainResult(
    SimulationResult? SimulationResult,
    FinalRankingResult? FinalRankingResult,
    FinalRankingDomainInput? PendingFinalRankingInput);
