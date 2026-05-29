/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

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
///     <pre>
/// ［シミュレーション　＞　本戦モード］の主フロー
/// 
/// TODO: 今は［標準版］とか［本戦版］とかに分かれているが、将来的には［ルールを一覧から選択する］ということを撤廃して、処理を主線１本に統合して、DSL ファイルを入力するようにしたいぜ（＾▽＾）！
///     </pre>
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
        if (executionResult is SimulationMainlineExecutionResult<ResultRow> standardExecutionResult)
        {
            ConsoleResultPrinter.PrintResult(finalStageContext.Players.Count, standardExecutionResult.Result, finalStageContext.FirstPlayerWinRatePercent, standardExecutionResult.ResultRows);
            return;
        }

        var finalStageExecutionResult = (SimulationMainlineExecutionResult<FinalStageResultRow>)executionResult;
        ConsoleResultPrinter.PrintFinalStageResult(finalStageExecutionResult.Result, finalStageContext.FirstPlayerWinRatePercent, finalStageExecutionResult.ResultRows);
    }

    protected override void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
        FinalRankingDataFileWriter finalRankingDataFileWriter = new(RuleProfileMode.FinalStage);

        var finalStageContext = (FinalStageModeSimulationContext)context;
        if (executionResult is SimulationMainlineExecutionResult<ResultRow> standardExecutionResult)
        {
            WriteFinalRankingOutputsForFinalStageMode(finalRankingDataFileWriter, finalStageContext, standardExecutionResult);
            return;
        }

        WriteFinalRankingOutputsForFinalStageMode(finalRankingDataFileWriter, finalStageContext, (SimulationMainlineExecutionResult<FinalStageResultRow>)executionResult);
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
            return new SimulationMainlineExecutionResult<ResultRow>(result, standardResultRows);
        }

        var finalStageResult = FinalStageSimulationExecutor.Execute(context);
        var finalStageResultRows = RankingResultRowBuilder.BuildFinalStageResultRows(context.Players, context.Matches, finalStageResult, context.FirstPlayerWinRatePercent, context.GroupMap!, context.EffectiveAdditionalApexCount);
        return new SimulationMainlineExecutionResult<FinalStageResultRow>(finalStageResult, finalStageResultRows);

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

    /// <summary>
    /// 出力
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <param name="standardResultRows"></param>
    /// <param name="finalStageResultRows"></param>
    static void WriteFinalRankingOutputsForFinalStageMode(
        FinalRankingDataFileWriter finalRankingDataFileWriter,
        FinalStageModeSimulationContext context,
        SimulationMainlineExecutionResult<ResultRow> executionResult)
    {
        var (outputCsvPath, outputMarkdownPath, referenceMatchesCsvPath) = PrepareFinalStageOutputPaths(context);

        WriteFinalRankingOutputs(finalRankingDataFileWriter, outputCsvPath, outputMarkdownPath, executionResult.Result, context.FirstPlayerWinRatePercent, executionResult.ResultRows);

        CompleteFinalStageOutputs(context, outputCsvPath, outputMarkdownPath, referenceMatchesCsvPath);
    }

    static void WriteFinalRankingOutputsForFinalStageMode(
        FinalRankingDataFileWriter finalRankingDataFileWriter,
        FinalStageModeSimulationContext context,
        SimulationMainlineExecutionResult<FinalStageResultRow> executionResult)
    {
        var (outputCsvPath, outputMarkdownPath, referenceMatchesCsvPath) = PrepareFinalStageOutputPaths(context);
        WriteFinalRankingOutputs(
            finalRankingDataFileWriter,
            outputCsvPath,
            outputMarkdownPath,
            executionResult.Result,
            context.FirstPlayerWinRatePercent,
            executionResult.ResultRows,
            referenceMatchesCsvPath: referenceMatchesCsvPath);
        CompleteFinalStageOutputs(context, outputCsvPath, outputMarkdownPath, referenceMatchesCsvPath);
    }

    static (string OutputCsvPath, string OutputMarkdownPath, string? ReferenceMatchesCsvPath) PrepareFinalStageOutputPaths(FinalStageModeSimulationContext context)
    {
        var (outputCsvPath, outputMarkdownPath) = ResolveFinalRankingOutputPaths($"final_stage_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var referenceMatchesCsvPath = context.ReferenceMatches.Count > 0
            ? ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"reference_matches_{DateTime.Now:yyyyMMdd_HHmmss}.csv")
            : null;
        return (outputCsvPath, outputMarkdownPath, referenceMatchesCsvPath);
    }

    static void CompleteFinalStageOutputs(
        FinalStageModeSimulationContext context,
        string outputCsvPath,
        string outputMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        PrintFinalRankingOutputCompleted(outputCsvPath, outputMarkdownPath);

        if (context.ReferenceMatches.Count == 0) return;

        CsvOutputHelpers.WriteReferenceMatchCsv(referenceMatchesCsvPath!, context.Players, context.ReferenceMatches);
        Console.WriteLine($"参考対局CSVを出力しました: {referenceMatchesCsvPath}");
    }
}

