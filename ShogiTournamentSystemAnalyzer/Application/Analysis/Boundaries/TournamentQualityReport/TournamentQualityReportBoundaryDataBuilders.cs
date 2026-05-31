/*
 * ［分析　＞　境界　＞　大会品質レポート］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;

/// <summary>
/// 境界データビルダー
/// </summary>
internal static partial class BoundaryDataBuilders
{
    /// <summary>
    /// ［大会品質評価レポート］組立
    /// </summary>
    /// <param name="qualityEvaluationRun"></param>
    /// <returns></returns>
    internal static TournamentQualityReportData BuildTournamentQualityReportBoundaryData(TournamentQualityReportRun qualityEvaluationRun)
    {
        return new TournamentQualityReportData(
            qualityEvaluationRun.PlayerRows,
            qualityEvaluationRun.Summary,
            qualityEvaluationRun.CalculationMode,
            qualityEvaluationRun.Suggestion);
    }

    /// <summary>
    /// ［大会品質評価レポート（スイープ）］組立
    /// </summary>
    /// <param name="sweepRows"></param>
    /// <param name="stoppedByTimeout"></param>
    /// <param name="suggestion"></param>
    /// <returns></returns>
    internal static TournamentQualitySweepReportData BuildTournamentQualitySweepReportBoundaryData(
        IReadOnlyList<TournamentQualitySweepReportRow> sweepRows,
        bool stoppedByTimeout,
        TournamentQualityNextCycleSuggestion suggestion)
    {
        return new TournamentQualitySweepReportData(sweepRows, stoppedByTimeout, suggestion);
    }
}