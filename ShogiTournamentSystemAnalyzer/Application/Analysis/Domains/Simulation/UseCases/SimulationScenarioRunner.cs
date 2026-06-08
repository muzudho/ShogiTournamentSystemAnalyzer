/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis;

/// <summary>
/// ［シミュレーション域　＞　シナリオランナー］
/// </summary>
internal static class SimulationScenarioRunner
{
    /// <summary>
    /// シナリオ実行
    /// </summary>
    /// <param name="scenario">シナリオ</param>
    internal static void Run(ISimulationScenario scenario)
    {
        // 概要表示
        scenario.PrintOverview();

        // 実行準備
        if (!scenario.TryPrepareExecution(out var plan)) return;

        // 実行
        var result = plan.Execute();
        if (result is null) return;

        var context = new AnalysisExecutionContext();
        context.SetSimulationResult(request: null, result);
        FinalRankingRequestDispatcher.TryExecute(context);
    }
}
