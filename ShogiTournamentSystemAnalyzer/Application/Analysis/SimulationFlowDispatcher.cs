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

        var ruleProfileAttributes = RuleProfileAttributes.FromCompatibilityLabel(ruleProfileMode);
        switch (ruleProfileAttributes.SimulationShape)
        {
            case RuleProfileSimulationShape.ScheduledMatches:
            case RuleProfileSimulationShape.FinalStageGrouped:
                SimulationScenarioRunner.Run(SimulationScenarioFactory.Create(ruleProfileAttributes));
                return true;

            case RuleProfileSimulationShape.TournamentFramework:
                SimulationTournamentFrameworkMode.Run();
                return true;

            case RuleProfileSimulationShape.Empty:
                SimulationEmptyMode.Run();
                return true;

            default:
                return false;
        }
    }
}
