/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal abstract class AbstractSimulationMainline
{
    public void RunDynamic(AbstractSimulationContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");

        RunDynamicCore(context);
    }

    protected virtual void RunDynamicCore(AbstractSimulationContext context)
    {
    }

    protected static void PrintMatchesAndCount(AbstractSimulationContext context, string matchCountLabel)
    {
        ConsoleResultPrinter.PrintMatchesCsv(context.Players, context.Matches);
        Console.WriteLine($"\n{matchCountLabel}: {context.Matches.Count}");
    }

    protected static void PrintTimeLimitIfNeeded(CalculationResult result)
    {
        if (!result.Mode.Contains("時間切れ", StringComparison.Ordinal)) return;

        Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
    }
}
