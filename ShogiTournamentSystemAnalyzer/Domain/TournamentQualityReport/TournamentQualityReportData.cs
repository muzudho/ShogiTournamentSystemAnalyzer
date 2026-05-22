/// <summary>
/// ［６大境界］のうち、［大会品質レポート］境界データだ。
/// </summary>
/// <param name="PlayerRows"></param>
/// <param name="Summary"></param>
/// <param name="CalculationMode"></param>
sealed record class TournamentQualityReportData(
    IReadOnlyList<TournamentQualityReportPlayerRow> PlayerRows,
    TournamentQualityReportSummary Summary,
    string CalculationMode);
