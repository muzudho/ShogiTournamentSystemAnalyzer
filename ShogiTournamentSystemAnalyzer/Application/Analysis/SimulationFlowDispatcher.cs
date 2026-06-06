/*
 * ［アプリケーション　＞　実行　＞　シミュレーションフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class SimulationFlowDispatcher
{
    internal static bool TryExecute(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        if (flowMode != AnalysisFlowMode.Simulation) return false;

        switch (ruleProfileMode)
        {
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
