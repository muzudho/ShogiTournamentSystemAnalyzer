/*
 * ［大会品質レポートという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［６大境界］のうち、［大会品質レポート］境界データだ。
/// </summary>
/// <param name="PlayerRows"></param>
/// <param name="Summary"></param>
/// <param name="CalculationMode"></param>
/// <param name="Suggestion"></param>
sealed record class TournamentQualityReportData(
    IReadOnlyList<TournamentQualityReportPlayerRow> PlayerRows,
    TournamentQualityReportSummary Summary,
    string CalculationMode,
    TournamentQualityNextCycleSuggestion Suggestion);
