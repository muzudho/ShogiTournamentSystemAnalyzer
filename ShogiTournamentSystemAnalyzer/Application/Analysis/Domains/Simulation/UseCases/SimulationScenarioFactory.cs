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
            _ when ruleProfileAttributes.IsStandardScheduledProfile => StandardSimulationScenario.Instance,
            _ when ruleProfileAttributes.IsFinalStageScheduledProfile => FinalStageSimulationScenario.Instance,
            _ => throw new InvalidOperationException($"Simulation 用の scenario が未対応です: {ruleProfileAttributes}")
        };
    }
}
