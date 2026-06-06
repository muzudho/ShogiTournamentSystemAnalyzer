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

internal sealed class AnalysisFlowSelection
{
    internal AnalysisFlowSelection(IReadOnlyList<AnalysisFlowMode> steps)
    {
        if (steps.Count == 0) throw new ArgumentException("分析フローのステップが空です。", nameof(steps));

        Steps = steps.ToArray();
    }

    internal IReadOnlyList<AnalysisFlowMode> Steps { get; }

    internal bool RunsSimulation => Steps.Contains(AnalysisFlowMode.Simulation);

    internal bool RunsQualityEvaluation => Steps.Contains(AnalysisFlowMode.QualityEvaluation);

    internal static AnalysisFlowSelection FromSingle(AnalysisFlowMode mode)
    {
        return new AnalysisFlowSelection(new[] { mode });
    }

    internal static AnalysisFlowSelection FromFlags(bool runsSimulation, bool runsQualityEvaluation)
    {
        var steps = new List<AnalysisFlowMode>();
        if (runsSimulation) steps.Add(AnalysisFlowMode.Simulation);
        if (runsQualityEvaluation) steps.Add(AnalysisFlowMode.QualityEvaluation);

        return new AnalysisFlowSelection(steps);
    }

    internal string ToRequestFileValue()
    {
        return string.Join(",", Steps.Select(FormatStep));
    }

    internal string ToPromptLabel()
    {
        return string.Join(" -> ", Steps.Select(FormatStep));
    }

    static string FormatStep(AnalysisFlowMode step)
    {
        return step switch
        {
            AnalysisFlowMode.Simulation => "Simulation",
            AnalysisFlowMode.QualityEvaluation => "QualityEvaluation",
            _ => step.ToString(),
        };
    }
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

    /// <summary>
    /// TODO: "TournamentFramework" は［４大域］のいずれかに含めてほしいぜ（＾～＾）
    /// </summary>
    TournamentFramework,

    Empty,
}
