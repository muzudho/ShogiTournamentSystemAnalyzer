/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
///     <pre>
/// ［シミュレーション　＞　本戦モード］の主フロー
/// 
/// TODO: 今は［標準版］とか［本戦版］とかに分かれているが、将来的には［ルールを一覧から選択する］ということを撤廃して、処理を主線１本に統合して、DSL ファイルを入力するようにしたいぜ（＾▽＾）！
///     </pre>
/// </summary>
internal class FinalStageSimulationMainline
    : AbstractSimulationMainline
{
    int? simulationCountOverride;

    internal SimulationMainlineResult Run(FinalStageModeSimulationContext context, int? simulationCountOverride)
    {
        this.simulationCountOverride = simulationCountOverride;
        try
        {
            return Run((AbstractSimulationContext)context);
        }
        finally
        {
            this.simulationCountOverride = null;
        }
    }

    protected override SimulationMainlineResult ExecuteSimulation(AbstractSimulationContext context)
    {
        var finalStageContext = (FinalStageModeSimulationContext)context;
        return ExecuteTournamentFinalStateAndFinalRanking(finalStageContext, simulationCountOverride);
    }

    protected override void AfterExecuteSimulationContext(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
    {
        PrintFinalStageModeContext((FinalStageModeSimulationContext)context);
    }

    protected override void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
    {
        var finalStageContext = (FinalStageModeSimulationContext)context;
        if (mainlineResult.Presentation == SimulationMainlineResultPresentation.Championship)
        {
            ConsoleResultPrinter.PrintResult(finalStageContext.Players.Count, mainlineResult.SimulationResult.TournamentFinalState, finalStageContext.FirstPlayerWinRatePercent, mainlineResult.FinalRankingResult.Rows);
            return;
        }

        ConsoleResultPrinter.PrintFinalStageResult(mainlineResult.SimulationResult.TournamentFinalState, finalStageContext.FirstPlayerWinRatePercent, mainlineResult.FinalRankingResult.Rows);
    }

    /// <summary>
    /// シミュレーションして、最終順位付け。
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requestedSimulationCount"></param>
    /// <returns></returns>
    static SimulationMainlineResult ExecuteTournamentFinalStateAndFinalRanking(FinalStageModeSimulationContext context, int? requestedSimulationCount)
    {
        if (context.GroupingMode == FinalStageGroupingMode.Off)
        {
            var result = ExecuteStandardMainline();
            var standardResultRows = BuildStandardResultRows(context, result);
            return new SimulationMainlineResult(result, standardResultRows, SimulationMainlineResultPresentation.Championship);
        }

        var finalStageResult = FinalStageSimulationExecutor.Execute(context, requestedSimulationCount);
        var finalStageResultRows = FinalRankingDomain.BuildFinalStageResultRows(context.Players, context.Matches, finalStageResult, context.FirstPlayerWinRatePercent, context.GroupMap!, context.EffectiveAdditionalApexCount);
        return new SimulationMainlineResult(finalStageResult, finalStageResultRows, SimulationMainlineResultPresentation.GroupedOverall);

        // 以下、ローカル関数

        CalculationResult ExecuteStandardMainline()
        {
            var ruleLabel = TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode);
            return ExecuteStandardTournamentFinalState(
                context,
                exactCalculationMessage: $"{ruleLabel} の厳密計算を行います。\n",
                simulationPrompt: $"局数が多いため {ruleLabel} のシミュレーションで近似します。試行回数を入力してください [200000]: ");
        }
    }

    /// <summary>
    /// 表示
    /// </summary>
    /// <param name="context"></param>
    static void PrintFinalStageModeContext(FinalStageModeSimulationContext context)
    {
        Console.WriteLine($"Apex / Innov の分け方: {FinalStageGroupingRule.GetLabel(context.GroupingMode)}\n");
        if (context.UsesFinalStageGrouping)
        {
            Console.WriteLine($"Apex: {context.ApexCount} 名");
            Console.WriteLine($"Innov: {context.InnovCount} 名\n");
            Console.WriteLine($"本戦不出場Apex: {context.AdditionalApexPlayers.Count} 名\n");
            Console.WriteLine($"本戦不出場Apexの扱い: {AdditionalApexPlacementRule.GetLabel(context.AdditionalApexPlacementMode)}\n");
            Console.WriteLine($"境界救済戦: {BoundaryRescueRule.GetLabel(context.BoundaryRescueMode)}\n");
        }
        else
        {
            Console.WriteLine($"対局者数: {context.Players.Count} 名\n");
        }

        PrintCommonSimulationContext(context, "本戦対局数");
        Console.WriteLine();
        PrintReferenceMatchesIfAny(context.Players, context.ReferenceMatches);
    }
}
