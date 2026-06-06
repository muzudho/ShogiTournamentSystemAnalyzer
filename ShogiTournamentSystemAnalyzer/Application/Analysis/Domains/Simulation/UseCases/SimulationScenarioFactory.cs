/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class SimulationScenarioFactory
{
    internal static ISimulationScenario Create(RuleProfileAttributes ruleProfileAttributes)
    {
        return ruleProfileAttributes.SimulationShape switch
        {
            RuleProfileSimulationShape.ScheduledMatches when !ruleProfileAttributes.UsesFinalStageGrouping => StandardSimulationScenario.Instance,
            RuleProfileSimulationShape.FinalStageGrouped => FinalStageSimulationScenario.Instance,
            RuleProfileSimulationShape.ScheduledMatches when ruleProfileAttributes.UsesFinalStageGrouping => FinalStageSimulationScenario.Instance,
            _ => throw new InvalidOperationException($"Simulation 用の scenario が未対応です: {ruleProfileAttributes}")
        };
    }
}
