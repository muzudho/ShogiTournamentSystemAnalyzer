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

enum RuleProfileMode
{
    Standard,
    FinalStage,
    TournamentFramework,
    Empty,
}

