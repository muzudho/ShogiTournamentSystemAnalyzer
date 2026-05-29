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
///     <pre>
/// ［大会ルール］の種類
/// 
///     - TODO: 最終的には、DSL / ルール設定ファイルへ外出ししたい（＾～＾）
///     </pre>
/// </summary>
enum RuleProfileMode
{
    Standard,
    FinalStage,
    TournamentFramework,
    Empty,
}

