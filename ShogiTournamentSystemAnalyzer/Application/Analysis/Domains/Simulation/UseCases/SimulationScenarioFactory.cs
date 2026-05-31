/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class SimulationScenarioFactory
{
    internal static ISimulationScenario Create(RuleProfileMode ruleProfileMode)
    {
        return ruleProfileMode switch
        {
            RuleProfileMode.Standard => StandardSimulationScenario.Instance,
            RuleProfileMode.FinalStage => FinalStageSimulationScenario.Instance,
            _ => throw new InvalidOperationException($"Simulation 用の scenario が未対応です: {ruleProfileMode}")
        };
    }
}
