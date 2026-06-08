/*
 * ［アプリケーション　＞　実行　＞　シミュレーションフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［シミュレーション域］
/// </summary>
internal static class SimulationFlowDispatcher
{
    internal static bool TryExecute(AnalysisFlowMode flowMode, RuleProfileAttributes ruleProfileAttributes)
    {
        if (flowMode != AnalysisFlowMode.Simulation) return false;

        switch (ruleProfileAttributes.SimulationShape)
        {
            case RuleProfileSimulationShape.ScheduledMatches:
            case RuleProfileSimulationShape.FinalStageGrouped:
                SimulationScenarioRunner.Run(SimulationScenarioFactory.Create(ruleProfileAttributes));
                return true;

            case RuleProfileSimulationShape.TournamentFramework:
                ExecutePendingFinalRanking(SimulationTournamentFrameworkMode.Run());
                return true;

            case RuleProfileSimulationShape.Empty:
                ExecutePendingFinalRanking(SimulationEmptyMode.Run());
                return true;

            default:
                return false;
        }
    }

    static void ExecutePendingFinalRanking(FinalRankingDomainInput input)
    {
        var context = new AnalysisExecutionContext();
        context.SetPendingFinalRanking(input);
        if (!FinalRankingRequestDispatcher.TryExecute(context))
        {
            throw new InvalidOperationException("未対応の最終順位付け域です。");
        }
    }
}
