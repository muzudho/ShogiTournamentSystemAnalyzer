/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域結果］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal sealed record class SimulationDomainResult(
    SimulationResult SimulationResult,
    FinalRankingResult FinalRankingResult,
    FinalRankingDomainInput? PendingFinalRankingInput);
