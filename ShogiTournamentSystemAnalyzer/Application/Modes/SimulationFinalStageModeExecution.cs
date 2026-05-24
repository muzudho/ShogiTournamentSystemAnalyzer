/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;

internal static partial class Program
{
    static CalculationResult ExecuteTournamentFinalStateAndFinalRanking(
        FinalStageModeContext context,
        out IReadOnlyList<ResultRow>? standardResultRows,
        out IReadOnlyList<FinalStageResultRow>? finalStageResultRows)
    {
        standardResultRows = null;
        finalStageResultRows = null;

        if (context.GroupingMode == FinalStageGroupingMode.Off)
        {
            var result = ExecuteStandardMainlineForFinalStageMode(context);
            standardResultRows = RankingResultRowBuilder.BuildResultRows(context.Players, context.Matches, result, context.FirstPlayerWinRatePercent);
            ConsoleResultPrinter.PrintResult(context.Players.Count, result, context.FirstPlayerWinRatePercent, standardResultRows);
            if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
            {
                Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
            }

            return result;
        }

        var finalStageResult = ExecuteTournamentFinalStateForFinalStageMode(context);
        finalStageResultRows = RankingResultRowBuilder.BuildFinalStageResultRows(context.Players, context.Matches, finalStageResult, context.FirstPlayerWinRatePercent, context.GroupMap!, context.EffectiveAdditionalApexCount);
        ConsoleResultPrinter.PrintFinalStageResult(finalStageResult, context.FirstPlayerWinRatePercent, finalStageResultRows);
        if (finalStageResult.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        return finalStageResult;
    }

    static CalculationResult ExecuteStandardMainlineForFinalStageMode(FinalStageModeContext context)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine($"{TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)} の厳密計算を行います。\n");
            return CalculateExactly(context.Players, context.Matches, context.FirstPlayerWinRateRating, context.TournamentRuleSetMode);
        }

        const int defaultSimulationCount = 200_000;
        var simulationCount = ConsolePromptReaders.ReadIntWithDefault(
            $"局数が多いため {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)} のシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
            defaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return CalculateBySimulation(context.Players, context.Matches, context.FirstPlayerWinRateRating, simulationCount, context.TournamentRuleSetMode);
    }

    static CalculationResult ExecuteTournamentFinalStateForFinalStageMode(FinalStageModeContext context)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine("本戦専用の厳密計算を行います。\n");
            return CalculateFinalStageExactly(context.Players, context.Matches, context.GroupMap!, context.EffectiveAdditionalApexCount, context.BoundaryRescueMode, context.FirstPlayerWinRateRating);
        }

        const int finalStageDefaultSimulationCount = 200_000;
        var finalStageSimulationCount = ConsolePromptReaders.ReadIntWithDefault(
            $"局数が多いため本戦専用シミュレーションで近似します。試行回数を入力してください [{finalStageDefaultSimulationCount}]: ",
            finalStageDefaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var finalStageSimulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return CalculateFinalStageBySimulation(context.Players, context.Matches, context.GroupMap!, context.EffectiveAdditionalApexCount, context.BoundaryRescueMode, context.FirstPlayerWinRateRating, finalStageSimulationCount);
    }

    static void PrintFinalStageModeContext(FinalStageModeContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");
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

        ConsoleResultPrinter.PrintMatchesCsv(context.Players, context.Matches);
        Console.WriteLine($"本戦対局数: {context.Matches.Count}\n");
        if (context.ReferenceMatches.Count > 0)
        {
            ConsoleResultPrinter.PrintMatchesCsv(context.Players, context.ReferenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {context.ReferenceMatches.Count}");
            Console.WriteLine("参考対局は順位計算に含めません。\n");
        }
    }

    static void WriteFinalRankingOutputsForFinalStageMode(
        FinalStageModeContext context,
        CalculationResult result,
        IReadOnlyList<ResultRow>? standardResultRows,
        IReadOnlyList<FinalStageResultRow>? finalStageResultRows)
    {
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath($"final_stage_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        var referenceMatchesCsvPath = context.ReferenceMatches.Count > 0
            ? ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"reference_matches_{DateTime.Now:yyyyMMdd_HHmmss}.csv")
            : null;
        if (context.GroupingMode == FinalStageGroupingMode.On)
        {
            WriterHelper.WriteText(
                outputPath: outputCsvPath,
            getLines: () => FinalRankingDataFileWriter.CreateFinalStageResultCsv(outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, finalStageResultRows!));

            WriterHelper.WriteText(
                outputPath: outputMarkdownPath,
            getLines: () => FinalRankingDataFileWriter.CreateFinalStageResultMarkdown(outputMarkdownPath, outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, finalStageResultRows!, referenceMatchesCsvPath));
        }
        else
        {
            WriterHelper.WriteText(
                outputPath: outputCsvPath,
            getLines: () => FinalRankingDataFileWriter.CreateResultCsv(result.Mode, context.FirstPlayerWinRatePercent, standardResultRows!));

            WriterHelper.WriteText(
                outputPath: outputMarkdownPath,
            getLines: () => FinalRankingDataFileWriter.CreateResultMarkdown(outputMarkdownPath, outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, standardResultRows!));

        }
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");

        if (context.ReferenceMatches.Count > 0)
        {
            CsvOutputHelpers.WriteReferenceMatchCsv(referenceMatchesCsvPath!, context.Players, context.ReferenceMatches);
            Console.WriteLine($"参考対局CSVを出力しました: {referenceMatchesCsvPath}");
        }
    }
}

