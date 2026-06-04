/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal readonly record struct TournamentQualityEvaluationOutputOptions(
    TournamentQualityEvaluationReportGroupingOptions ReportGroupingOptions,
    string OutputCsvPath,
    string? PlayerCsvPath = null,
    RuleProfileMode RuleProfileMode = RuleProfileMode.Standard);