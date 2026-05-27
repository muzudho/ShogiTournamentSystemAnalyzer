/*
 * ［大会品質レポートという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［６大境界］のうち、［大会品質レポート］境界データのスイープ実験版だ。
/// </summary>
/// <param name="SweepRows"></param>
/// <param name="StoppedByTimeout"></param>
/// <param name="Suggestion"></param>
sealed record class TournamentQualitySweepReportData(
    IReadOnlyList<TournamentQualitySweepReportRow> SweepRows,
    bool StoppedByTimeout,
    TournamentQualityNextCycleSuggestion Suggestion);
