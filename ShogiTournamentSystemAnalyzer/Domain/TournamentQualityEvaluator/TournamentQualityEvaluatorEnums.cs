/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［イノベーターモードで順位オフセットモード］
/// </summary>
enum TournamentQualityEvaluationInnovExpectedRankOffsetMode
{
    Off,
    On,
}

enum FinalStageGroupingMode
{
    Off,
    On,
}

enum TournamentQualityEvaluationReportOutcome
{
    Good,
    Bad,
}

enum AnalysisFlowMode
{
    Simulation,
    QualityEvaluation,
}

/// <summary>
/// ［大会ルール］の種類
/// </summary>
enum RuleProfileMode
{
    Standard,
    FinalStage,
    TournamentFramework,
    Empty,
}

