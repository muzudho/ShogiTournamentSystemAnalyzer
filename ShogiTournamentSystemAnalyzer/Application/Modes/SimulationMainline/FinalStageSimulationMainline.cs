/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Paths;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［シミュレーション　＞　本戦モード］の主フロー
/// </summary>
internal class FinalStageSimulationMainline
    : AbstractSimulationMainline
{
    protected override SimulationMainlineExecutionResult ExecuteSimulation(AbstractSimulationContext context)
    {
        var finalStageContext = (FinalStageModeSimulationContext)context;
        return ExecuteTournamentFinalStateAndFinalRanking(finalStageContext);
    }

    protected override void AfterExecuteSimulationContext(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        PrintFinalStageModeContext((FinalStageModeSimulationContext)context);
    }

    protected override void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        var finalStageContext = (FinalStageModeSimulationContext)context;
        if (finalStageContext.GroupingMode == FinalStageGroupingMode.Off)
        {
            ConsoleResultPrinter.PrintResult(finalStageContext.Players.Count, executionResult.Result, finalStageContext.FirstPlayerWinRatePercent, executionResult.StandardResultRows!);
            return;
        }

        ConsoleResultPrinter.PrintFinalStageResult(executionResult.Result, finalStageContext.FirstPlayerWinRatePercent, executionResult.FinalStageResultRows!);
    }

    protected override void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        WriteFinalRankingOutputsForFinalStageMode(
            (FinalStageModeSimulationContext)context,
            executionResult.Result,
            executionResult.StandardResultRows,
            executionResult.FinalStageResultRows);
    }

    /// <summary>
    /// シミュレーションして、最終順位付け。
    /// </summary>
    /// <param name="context"></param>
    /// <param name="standardResultRows"></param>
    /// <param name="finalStageResultRows"></param>
    /// <returns></returns>
    static SimulationMainlineExecutionResult ExecuteTournamentFinalStateAndFinalRanking(FinalStageModeSimulationContext context)
    {
        if (context.GroupingMode == FinalStageGroupingMode.Off)
        {
            var result = ExecuteStandardMainline();
            var standardResultRows = BuildStandardResultRows(context, result);
            return new SimulationMainlineExecutionResult(result, standardResultRows);
        }

        var finalStageResult = ExecuteFinalStageMainline();
        var finalStageResultRows = RankingResultRowBuilder.BuildFinalStageResultRows(context.Players, context.Matches, finalStageResult, context.FirstPlayerWinRatePercent, context.GroupMap!, context.EffectiveAdditionalApexCount);
        return new SimulationMainlineExecutionResult(finalStageResult, FinalStageResultRows: finalStageResultRows);

        // 以下、ローカル関数

        CalculationResult ExecuteStandardMainline()
        {
            var ruleLabel = TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode);
            return ExecuteStandardTournamentFinalState(
                context,
                exactCalculationMessage: $"{ruleLabel} の厳密計算を行います。\n",
                simulationPrompt: $"局数が多いため {ruleLabel} のシミュレーションで近似します。試行回数を入力してください [200000]: ");
        }

        CalculationResult ExecuteFinalStageMainline()
        {
            if (context.Matches.Count <= 20)
            {
                Console.WriteLine("本戦専用の厳密計算を行います。\n");
                return FinalStageCalculationEngine.CalculateFinalStageExactly(context.Players, context.Matches, context.GroupMap!, context.EffectiveAdditionalApexCount, context.BoundaryRescueMode, context.FirstPlayerWinRateRating);
            }

            const int finalStageDefaultSimulationCount = 200_000;
            var finalStageSimulationCount = ConsolePromptReaders.ReadIntWithDefault(
                $"局数が多いため本戦専用シミュレーションで近似します。試行回数を入力してください [{finalStageDefaultSimulationCount}]: ",
                finalStageDefaultSimulationCount,
                min: 1);

            Console.WriteLine();
            using var finalStageSimulationBudget = SimulationTimeBudget.BeginSimulationBudget();
            return FinalStageCalculationEngine.CalculateFinalStageBySimulation(context.Players, context.Matches, context.GroupMap!, context.EffectiveAdditionalApexCount, context.BoundaryRescueMode, context.FirstPlayerWinRateRating, finalStageSimulationCount);
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

    /// <summary>
    /// 出力
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <param name="standardResultRows"></param>
    /// <param name="finalStageResultRows"></param>
    static void WriteFinalRankingOutputsForFinalStageMode(
        FinalStageModeSimulationContext context,
        CalculationResult result,
        IReadOnlyList<ResultRow>? standardResultRows,
        IReadOnlyList<FinalStageResultRow>? finalStageResultRows)
    {
        var (outputCsvPath, outputMarkdownPath) = ResolveFinalRankingOutputPaths($"final_stage_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var referenceMatchesCsvPath = context.ReferenceMatches.Count > 0
            ? ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"reference_matches_{DateTime.Now:yyyyMMdd_HHmmss}.csv")
            : null;
        if (context.GroupingMode == FinalStageGroupingMode.On)
        {
            WriteFinalStageFinalRankingOutputs(outputCsvPath, outputMarkdownPath, result, context.FirstPlayerWinRatePercent, finalStageResultRows!, referenceMatchesCsvPath);
        }
        else
        {
            WriteStandardFinalRankingOutputs(outputCsvPath, outputMarkdownPath, result, context.FirstPlayerWinRatePercent, standardResultRows!);
        }
        PrintFinalRankingOutputCompleted(outputCsvPath, outputMarkdownPath);

        if (context.ReferenceMatches.Count > 0)
        {
            CsvOutputHelpers.WriteReferenceMatchCsv(referenceMatchesCsvPath!, context.Players, context.ReferenceMatches);
            Console.WriteLine($"参考対局CSVを出力しました: {referenceMatchesCsvPath}");
        }
    }
}

