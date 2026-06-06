/*
 * ［アプリケーション　＞　実行　＞　シミュレーションフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［シミュレーション域］
/// </summary>
internal static class SimulationFlowDispatcher
{
    /// <summary>
    /// シミュレーションフローを実行する。
    /// </summary>
    /// <param name="flowMode"></param>
    /// <param name="ruleProfileMode"></param>
    /// <returns></returns>
    internal static bool TryExecute(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        if (flowMode != AnalysisFlowMode.Simulation) return false;

        switch (ruleProfileMode)
        {
            // TODO: ［標準ルール］とか、［本戦ルール］のような分類は、将来的にこのシステムから廃止したい（＾～＾）もっと細かいルールの違いを、ルールプロファイルの属性で表現したい（＾～＾）
            case RuleProfileMode.Standard:
            case RuleProfileMode.FinalStage:
                SimulationScenarioRunner.Run(SimulationScenarioFactory.Create(ruleProfileMode));
                return true;

            case RuleProfileMode.TournamentFramework:
                SimulationTournamentFrameworkMode.Run();
                return true;

            case RuleProfileMode.Empty:
                SimulationEmptyMode.Run();
                return true;

            default:
                return false;
        }
    }
}
