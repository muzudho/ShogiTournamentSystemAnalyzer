/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
///     <pre>
/// ［シミュレーション　＞　標準モード］の主フロー
/// 
/// TODO: 今は［標準版］とか［本戦版］とかに分かれているが、将来的には［ルールを一覧から選択する］ということを撤廃して、処理を主線１本に統合して、DSL ファイルを入力するようにしたいぜ（＾▽＾）！
///     </pre>
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
        return new SimulationMainlineExecutionResult<ResultRow>(tournamentFinalState, finalRankingRows);
    }

    protected override void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        var standardContext = (StandardModeSimulationContext)context;
        var standardExecutionResult = (SimulationMainlineExecutionResult<ResultRow>)executionResult;
        ConsoleResultPrinter.PrintResult(
            standardContext.Players.Count,
            standardExecutionResult.Result,
            standardContext.FirstPlayerWinRatePercent,
            standardExecutionResult.ResultRows);
    }

    protected override void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        var standardContext = (StandardModeSimulationContext)context;
        var standardExecutionResult = (SimulationMainlineExecutionResult<ResultRow>)executionResult;
        WriteFinalRankingOutputsForStandardMode(standardContext, standardExecutionResult.Result, standardExecutionResult.ResultRows);
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

        FinalRankingDataFileWriter finalRankingDataFileWriter = new(RuleProfileMode.Standard);
        WriteStandardFinalRankingOutputs(
            finalRankingDataFileWriter,
            outputCsvPath,
            outputMarkdownPath,
            tournamentFinalState,
            context.FirstPlayerWinRatePercent,
            finalRankingRows);

        PrintFinalRankingOutputCompleted(outputCsvPath, outputMarkdownPath);
    }
}

