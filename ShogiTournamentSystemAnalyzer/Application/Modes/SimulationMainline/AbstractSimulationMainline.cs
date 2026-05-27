/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
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

    protected static void PrintCommonSimulationContext(AbstractSimulationContext context, string matchCountLabel)
    {
        PrintMatchesAndCount(context, matchCountLabel);
    }

    protected static void PrintReferenceMatchesIfAny(IReadOnlyList<Player> players, IReadOnlyList<Match> referenceMatches)
    {
        if (referenceMatches.Count == 0) return;

        ConsoleResultPrinter.PrintMatchesCsv(players, referenceMatches, "参考対局CSV:");
        Console.WriteLine($"参考対局数: {referenceMatches.Count}");
        Console.WriteLine("参考対局は順位計算に含めません。\n");
    }

    protected static void PrintTimeLimitIfNeeded(CalculationResult result)
    {
        if (!result.Mode.Contains("時間切れ", StringComparison.Ordinal)) return;

        Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
    }

    protected static CalculationResult ExecuteStandardTournamentFinalState(
        AbstractSimulationContext context,
        string exactCalculationMessage,
        string simulationPrompt)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine(exactCalculationMessage);
            return StandardCalculationEngine.CalculateExactly(context.Players, context.Matches, context.FirstPlayerWinRateRating, context.TournamentRuleSetMode);
        }

        const int defaultSimulationCount = 200_000;
        var simulationCount = ConsolePromptReaders.ReadIntWithDefault(
            simulationPrompt,
            defaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return StandardCalculationEngine.CalculateBySimulation(context.Players, context.Matches, context.FirstPlayerWinRateRating, simulationCount, context.TournamentRuleSetMode);
    }

    protected static IReadOnlyList<ResultRow> BuildStandardResultRows(AbstractSimulationContext context, CalculationResult result)
    {
        return RankingResultRowBuilder.BuildResultRows(context.Players, context.Matches, result, context.FirstPlayerWinRatePercent);
    }
}
