/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［シミュレーション域］の主線
/// </summary>
internal abstract class AbstractSimulationMainline
{
    /// <summary>
    /// 実行
    /// </summary>
    /// <param name="context"></param>
    public SimulationMainlineResult Run(AbstractSimulationContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");
        BeforeExecuteSimulationContext(context);

        var mainlineResult = ExecuteSimulation(context);

        AfterExecuteSimulationContext(context, mainlineResult);
        PrintSimulationResult(context, mainlineResult);
        PrintTimeLimitIfNeeded(mainlineResult.SimulationResult.TournamentFinalState);
        WriteSimulationOutputs(context, mainlineResult);
        return mainlineResult;
    }

    protected virtual void BeforeExecuteSimulationContext(AbstractSimulationContext context)
    {
    }

    protected virtual void AfterExecuteSimulationContext(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
    {
    }

    protected abstract SimulationMainlineResult ExecuteSimulation(AbstractSimulationContext context);

    protected abstract void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineResult mainlineResult);

    protected virtual void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
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
        string simulationPrompt,
        int? requestedSimulationCount = null)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine(exactCalculationMessage);
            using var exactCalculationBudget = SimulationTimeBudget.BeginSimulationBudget();
            return StandardCalculationEngine.CalculateExactly(context.Players, context.Matches, context.FirstPlayerWinRateRating, context.TournamentRuleSetMode);
        }

        const int defaultSimulationCount = 200_000;
        var simulationCount = requestedSimulationCount
            ?? ConsolePromptReaders.ReadIntWithDefault(
                simulationPrompt,
                defaultSimulationCount,
                min: 1);

        Console.WriteLine();
        using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return StandardCalculationEngine.CalculateBySimulation(context.Players, context.Matches, context.FirstPlayerWinRateRating, simulationCount, context.TournamentRuleSetMode);
    }

    protected static IReadOnlyList<GeneralSimulationResultRow> BuildStandardResultRows(AbstractSimulationContext context, CalculationResult result)
    {
        return FinalRankingDomain.BuildStandardResultRows(context.Players, context.Matches, result, context.FirstPlayerWinRatePercent);
    }

}

internal enum SimulationMainlineResultPresentation
{
    Championship,
    GroupedOverall
}

internal sealed record SimulationMainlineResult(
    SimulationResult SimulationResult,
    FinalRankingResult FinalRankingResult,
    SimulationMainlineResultPresentation Presentation)
{
    internal SimulationMainlineResult(
        CalculationResult tournamentFinalState,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        SimulationMainlineResultPresentation presentation)
        : this(new SimulationResult(tournamentFinalState), new FinalRankingResult(resultRows), presentation)
    {
    }
}
