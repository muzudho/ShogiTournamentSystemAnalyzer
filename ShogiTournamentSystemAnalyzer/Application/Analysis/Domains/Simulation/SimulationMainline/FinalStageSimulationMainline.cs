/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
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
    string? outputPathOverride;
    int? simulationCountOverride;

    internal SimulationMainlineResult Run(FinalStageModeSimulationContext context, string? outputPathOverride)
    {
        return Run(context, outputPathOverride, simulationCountOverride: null);
    }

    internal SimulationMainlineResult Run(FinalStageModeSimulationContext context, string? outputPathOverride, int? simulationCountOverride)
    {
        this.outputPathOverride = outputPathOverride;
        this.simulationCountOverride = simulationCountOverride;
        try
        {
            return Run(context);
        }
        finally
        {
            this.outputPathOverride = null;
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

    protected override void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
    {
        FinalRankingMarkdownFileWriter finalRankingDataFileWriter = new(new FinalRankingDataFileWriterSettings(RuleProfileMode.FinalStage));

        var finalStageContext = (FinalStageModeSimulationContext)context;
        WriteFinalRankingOutputsForFinalStageMode(finalRankingDataFileWriter, finalStageContext, mainlineResult);
    }

    /// <summary>
    /// シミュレーションして、最終順位付け。
    /// </summary>
    /// <param name="context"></param>
    /// <param name="standardResultRows"></param>
    /// <param name="finalStageResultRows"></param>
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
        var finalStageResultRows = RankingResultRowBuilder.BuildFinalStageGeneralResultRows(context.Players, context.Matches, finalStageResult, context.FirstPlayerWinRatePercent, context.GroupMap!, context.EffectiveAdditionalApexCount);
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

    /// <summary>
    /// 出力
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <param name="standardResultRows"></param>
    /// <param name="finalStageResultRows"></param>
    void WriteFinalRankingOutputsForFinalStageMode(
        FinalRankingMarkdownFileWriter finalRankingDataFileWriter,
        FinalStageModeSimulationContext context,
        SimulationMainlineResult mainlineResult)
    {
        var (outputCsvPath, outputMarkdownPath, referenceMatchesCsvPath) = PrepareFinalStageOutputPaths(context);
        var markdownReferenceMatchesCsvPath = mainlineResult.Presentation == SimulationMainlineResultPresentation.GroupedOverall
            ? referenceMatchesCsvPath
            : null;

        FinalRankingDomain.WriteOutputs(
            finalRankingDataFileWriter,
            outputCsvPath,
            outputMarkdownPath,
            mainlineResult.SimulationResult.TournamentFinalState,
            context.FirstPlayerWinRatePercent,
            mainlineResult.FinalRankingResult.Rows,
            referenceMatchesCsvPath: markdownReferenceMatchesCsvPath);
        CompleteFinalStageOutputs(context, outputCsvPath, outputMarkdownPath, referenceMatchesCsvPath);
    }

    (string OutputCsvPath, string OutputMarkdownPath, string? ReferenceMatchesCsvPath) PrepareFinalStageOutputPaths(FinalStageModeSimulationContext context)
    {
        var (outputCsvPath, outputMarkdownPath) = FinalRankingDomain.ResolveOutputPaths(
            $"final_stage_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            outputPathOverride);
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
        FinalRankingDomain.PrintOutputCompleted(outputCsvPath, outputMarkdownPath);

        if (context.ReferenceMatches.Count == 0) return;

        CsvOutputHelpers.WriteReferenceMatchCsv(referenceMatchesCsvPath!, context.Players, context.ReferenceMatches);
        Console.WriteLine($"参考対局CSVを出力しました: {referenceMatchesCsvPath}");
    }
}

