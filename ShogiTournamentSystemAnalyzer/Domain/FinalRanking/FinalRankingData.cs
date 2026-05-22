/*
 * ［最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.FinalRanking;

/// <summary>
/// ［６大境界］のうち、［最終順位］境界データだ。
/// </summary>
/// <param name="RankRows"></param>
/// <param name="IsIntermediate"></param>
/// <param name="Note"></param>
sealed record class FinalRankingData(
    IReadOnlyList<PlayerRankRow> RankRows,
    bool IsIntermediate,
    string? Note);
