/*
 * ［アプリケーション　＞　実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Modes;
using ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class AnalysisFlowDispatcher
{
    internal static void Execute(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        switch ((flowMode, ruleProfileMode))
        {
            case (AnalysisFlowMode.Simulation, RuleProfileMode.Standard):
            case (AnalysisFlowMode.Simulation, RuleProfileMode.FinalStage):
                SimulationScenarioRunner.Run(SimulationScenarioFactory.Create(ruleProfileMode));
                break;

            case (AnalysisFlowMode.Simulation, RuleProfileMode.TournamentFramework):
                SimulationTournamentFrameworkMode.Run();
                break;

            case (AnalysisFlowMode.Simulation, RuleProfileMode.Empty):
                SimulationEmptyMode.Run();
                break;

            case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.Standard):
            case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.FinalStage):
                TournamentQualityEvaluationMode.Run(ruleProfileMode);
                break;

            default:
                throw new InvalidOperationException("未対応のモードです。");
        }
    }
}
