/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class FinalStageSimulationExecutor
{
    internal static CalculationResult Execute(FinalStageModeSimulationContext context)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine("本戦専用の厳密計算を行います。\n");
            using var exactCalculationBudget = SimulationTimeBudget.BeginSimulationBudget();
            return FinalStageCalculationEngine.CalculateFinalStageExactly(
                context.Players,
                context.Matches,
                context.GroupMap!,
                context.EffectiveAdditionalApexCount,
                context.BoundaryRescueMode,
                context.FirstPlayerWinRateRating);
        }

        const int finalStageDefaultSimulationCount = 200_000;
        var finalStageSimulationCount = ConsolePromptReaders.ReadIntWithDefault(
            $"局数が多いため本戦専用シミュレーションで近似します。試行回数を入力してください [{finalStageDefaultSimulationCount}]: ",
            finalStageDefaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var finalStageSimulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return FinalStageCalculationEngine.CalculateFinalStageBySimulation(
            context.Players,
            context.Matches,
            context.GroupMap!,
            context.EffectiveAdditionalApexCount,
            context.BoundaryRescueMode,
            context.FirstPlayerWinRateRating,
            finalStageSimulationCount);
    }
}
