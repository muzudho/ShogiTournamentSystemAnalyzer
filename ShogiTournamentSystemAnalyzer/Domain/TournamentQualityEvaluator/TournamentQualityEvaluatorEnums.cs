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

/// <summary>
/// 要求ファイル境界で使う分析フロー名。
/// </summary>
enum AnalysisFlowMode
{
    Simulation,
    QualityEvaluation,
}

internal sealed class AnalysisFlowSelection
{
    internal AnalysisFlowSelection(
        bool runsSimulationDomain,
        bool runsFinalRankingDomain,
        bool runsQualityEvaluationDomain)
    {
        if (!runsSimulationDomain && !runsFinalRankingDomain && !runsQualityEvaluationDomain)
        {
            throw new ArgumentException("分析フローが空です。");
        }

        RunsSimulationDomain = runsSimulationDomain;
        RunsFinalRankingDomain = runsFinalRankingDomain;
        RunsQualityEvaluationDomain = runsQualityEvaluationDomain;
    }

    internal bool RunsSimulationDomain { get; }

    internal bool RunsFinalRankingDomain { get; }

    internal bool RunsQualityEvaluationDomain { get; }

    internal static AnalysisFlowSelection FromSingle(AnalysisFlowMode mode)
    {
        return mode switch
        {
            AnalysisFlowMode.Simulation => new AnalysisFlowSelection(
                runsSimulationDomain: true,
                runsFinalRankingDomain: true,
                runsQualityEvaluationDomain: false),
            AnalysisFlowMode.QualityEvaluation => new AnalysisFlowSelection(
                runsSimulationDomain: false,
                runsFinalRankingDomain: false,
                runsQualityEvaluationDomain: true),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };
    }

    internal static AnalysisFlowSelection FromFlags(bool runsSimulation, bool runsQualityEvaluation)
    {
        return new AnalysisFlowSelection(
            runsSimulationDomain: runsSimulation,
            runsFinalRankingDomain: runsSimulation,
            runsQualityEvaluationDomain: runsQualityEvaluation);
    }

    internal string ToPromptLabel()
    {
        var domains = new List<string>();
        if (RunsSimulationDomain) domains.Add("SimulationDomain");
        if (RunsFinalRankingDomain) domains.Add("FinalRankingDomain");
        if (RunsQualityEvaluationDomain) domains.Add("QualityEvaluationDomain");
        return string.Join(" -> ", domains);
    }
}
