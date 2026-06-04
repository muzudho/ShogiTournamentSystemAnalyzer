/*
 * ［アプリケーション　＞　実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class AnalysisFlowDispatcher
{
    internal static void Execute(AnalysisFlowSelection flowSelection, RuleProfileMode ruleProfileMode)
    {
        foreach (var flowMode in flowSelection.Steps)
        {
            ExecuteSingle(flowMode, ruleProfileMode);
        }
    }

    static void ExecuteSingle(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
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