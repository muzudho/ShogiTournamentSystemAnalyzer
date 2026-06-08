/*
 * ［アプリケーション　＞　ユースケース　＞　最終順位付け域入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.FinalRanking.UseCases;

using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal enum FinalRankingDomainInputKind
{
    StandardSimulation,
    FinalStageSimulation,
    TournamentFrameworkSimulation,
    EmptySimulation,
}

internal sealed record FinalRankingDomainInput(
    FinalRankingDomainInputKind Kind,
    CalculationResult? TournamentFinalState,
    double FirstPlayerWinRatePercent,
    FinalRankingResult? FinalRankingResult,
    string? OutputPath,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> ReferenceMatches,
    bool WriteReferenceMatchesForMarkdown,
    TournamentFrameworkFinalRankingResult? TournamentFrameworkFinalRankingResult = null,
    EmptyFinalRankingDomainInput? EmptyFinalRankingInput = null);

internal sealed record EmptyFinalRankingDomainInput(string? OutputPath);
