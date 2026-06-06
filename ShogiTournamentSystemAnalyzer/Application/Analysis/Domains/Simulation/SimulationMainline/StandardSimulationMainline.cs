/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
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
    int? requestedSimulationCount;

    internal SimulationMainlineResult Run(StandardModeSimulationContext context, int? requestedSimulationCount = null)
    {
        this.requestedSimulationCount = requestedSimulationCount;
        try
        {
            return Run((AbstractSimulationContext)context);
        }
        finally
        {
            this.requestedSimulationCount = null;
        }
    }

    protected override void BeforeExecuteSimulationContext(AbstractSimulationContext context)
    {
        PrintStandardModeContext((StandardModeSimulationContext)context);
    }

    protected override SimulationMainlineResult ExecuteSimulation(AbstractSimulationContext context)
    {
        var standardContext = (StandardModeSimulationContext)context;
        var tournamentFinalState = ExecuteTournamentFinalState(standardContext);
        var finalRankingRows = BuildStandardResultRows(standardContext, tournamentFinalState);
        return new SimulationMainlineResult(tournamentFinalState, finalRankingRows, SimulationMainlineResultPresentation.Championship);
    }

    protected override void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
    {
        var standardContext = (StandardModeSimulationContext)context;
        ConsoleResultPrinter.PrintResult(
            standardContext.Players.Count,
            mainlineResult.SimulationResult.TournamentFinalState,
            standardContext.FirstPlayerWinRatePercent,
            mainlineResult.FinalRankingResult.Rows);
    }

    CalculationResult ExecuteTournamentFinalState(StandardModeSimulationContext context)
    {
        return ExecuteStandardTournamentFinalState(
            context,
            exactCalculationMessage: "厳密計算を行います。\n",
            simulationPrompt: "局数が多いためシミュレーションで近似します。試行回数を入力してください [200000]: ",
            requestedSimulationCount: requestedSimulationCount);
    }

    static void PrintStandardModeContext(StandardModeSimulationContext context)
    {
        if (context.ExcludedPlayerCount > 0)
        {
            Console.WriteLine($"未対局の選手 {context.ExcludedPlayerCount} 人を結果から除外します。\n");
        }

        PrintCommonSimulationContext(context, "総対局数");
    }
}
