/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal enum TournamentQualityEvaluationOutputProfile
{
    Standard,
    FinalStage,
}

internal readonly record struct TournamentQualityEvaluationOutputOptions(
    TournamentQualityEvaluationReportGroupingOptions ReportGroupingOptions,
    string OutputCsvPath,
    string? PlayerCsvPath = null,
    string? RequestInputLogPath = null,
    TournamentQualityEvaluationOutputProfile OutputProfile = TournamentQualityEvaluationOutputProfile.Standard)
{
    internal RuleProfileAttributes GetRuleProfileAttributes()
    {
        return OutputProfile switch
        {
            TournamentQualityEvaluationOutputProfile.Standard => RuleProfileAttributes.CreateStandardScheduled(),
            TournamentQualityEvaluationOutputProfile.FinalStage => RuleProfileAttributes.CreateFinalStageGrouped(),
            _ => throw new InvalidOperationException($"未対応の品質評価出力プロファイル: {OutputProfile}")
        };
    }
}
