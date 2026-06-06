/*
 * ［アプリケーション　＞　実行　＞　分析大域シーケンス］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［分析］のアプリケーション上の固定実行順序。
/// </summary>
internal sealed class AnalysisDomainSequence
{
    readonly AnalysisFlowSelection flowSelection;
    readonly RuleProfileMode ruleProfileMode;

    internal AnalysisDomainSequence(
        AnalysisFlowSelection flowSelection,
        RuleProfileMode ruleProfileMode)
    {
        this.flowSelection = flowSelection;
        this.ruleProfileMode = ruleProfileMode;
    }

    internal void Execute()
    {
        ExecuteSimulationDomain();
        ExecuteFinalRankingDomain();
        ExecuteQualityEvaluationDomain();
    }

    void ExecuteSimulationDomain()
    {
        if (!flowSelection.RunsSimulation) return;
        if (SimulationFlowDispatcher.TryExecute(AnalysisFlowMode.Simulation, ruleProfileMode)) return;

        throw new InvalidOperationException("未対応のシミュレーション域です。");
    }

    void ExecuteFinalRankingDomain()
    {
        // 現時点の手入力フローでは、最終順位付け域の処理はシミュレーション域の中から呼ばれる。
        // アプリケーション直下の順序としてはここに置き、後続分離時の差し込み位置を固定する。
    }

    void ExecuteQualityEvaluationDomain()
    {
        if (!flowSelection.RunsQualityEvaluation) return;
        if (QualityEvaluationFlowDispatcher.TryExecute(AnalysisFlowMode.QualityEvaluation, ruleProfileMode)) return;

        throw new InvalidOperationException("未対応の大会品質評価域です。");
    }
}
