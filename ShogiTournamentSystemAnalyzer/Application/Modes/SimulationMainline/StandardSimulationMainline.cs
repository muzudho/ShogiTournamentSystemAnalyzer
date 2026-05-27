/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［シミュレーション　＞　標準モード］の主フロー
/// </summary>
internal class StandardSimulationMainline
    : AbstractSimulationMainline
{
    protected override void BeforeExecuteSimulationContext(AbstractSimulationContext context)
    {
        PrintStandardModeContext((StandardModeSimulationContext)context);
    }

    protected override SimulationMainlineExecutionResult ExecuteSimulation(AbstractSimulationContext context)
    {
        var standardContext = (StandardModeSimulationContext)context;
        var tournamentFinalState = ExecuteTournamentFinalState(standardContext);
        var finalRankingRows = BuildStandardResultRows(standardContext, tournamentFinalState);
        return new SimulationMainlineExecutionResult(tournamentFinalState, finalRankingRows);
    }

    protected override void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        var standardContext = (StandardModeSimulationContext)context;
        ConsoleResultPrinter.PrintResult(
            standardContext.Players.Count,
            executionResult.Result,
            standardContext.FirstPlayerWinRatePercent,
            executionResult.StandardResultRows!);
    }

    protected override void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        var standardContext = (StandardModeSimulationContext)context;
        WriteFinalRankingOutputsForStandardMode(standardContext, executionResult.Result, executionResult.StandardResultRows!);
    }

    static CalculationResult ExecuteTournamentFinalState(StandardModeSimulationContext context)
    {
        return ExecuteStandardTournamentFinalState(
            context,
            exactCalculationMessage: "厳密計算を行います。\n",
            simulationPrompt: "局数が多いためシミュレーションで近似します。試行回数を入力してください [200000]: ");
    }

    static void PrintStandardModeContext(StandardModeSimulationContext context)
    {
        if (context.ExcludedPlayerCount > 0)
        {
            Console.WriteLine($"未対局の選手 {context.ExcludedPlayerCount} 人を結果から除外します。\n");
        }

        PrintCommonSimulationContext(context, "総対局数");
    }

    static void WriteFinalRankingOutputsForStandardMode(StandardModeSimulationContext context, CalculationResult tournamentFinalState, IReadOnlyList<ResultRow> finalRankingRows)
    {
        var (outputCsvPath, outputMarkdownPath) = ResolveFinalRankingOutputPaths($"standard_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        WriteStandardFinalRankingOutputs(outputCsvPath, outputMarkdownPath, tournamentFinalState, context.FirstPlayerWinRatePercent, finalRankingRows);

        PrintFinalRankingOutputCompleted(outputCsvPath, outputMarkdownPath);
    }
}

