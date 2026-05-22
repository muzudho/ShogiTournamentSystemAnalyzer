/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;

internal static partial class Program
{
    static CalculationResult ExecuteFinalStageMode(
        FinalStageModeContext context,
        out IReadOnlyList<ResultRow>? standardResultRows,
        out IReadOnlyList<FinalStageResultRow>? finalStageResultRows)
    {
        standardResultRows = null;
        finalStageResultRows = null;

        if (context.GroupingMode == FinalStageGroupingMode.Off)
        {
            CalculationResult result;
            if (context.Matches.Count <= 20)
            {
                Console.WriteLine($"{TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)} の厳密計算を行います。\n");
                result = CalculateExactly(context.Participants, context.Matches, context.FirstPlayerWinRateRating, context.TournamentRuleSetMode);
            }
            else
            {
                const int defaultSimulationCount = 200_000;
                var simulationCount = ConsolePromptReaders.ReadIntWithDefault(
                    $"局数が多いため {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)} のシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                    defaultSimulationCount,
                    min: 1);

                Console.WriteLine();
                using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
                result = CalculateBySimulation(context.Participants, context.Matches, context.FirstPlayerWinRateRating, simulationCount, context.TournamentRuleSetMode);
            }

            standardResultRows = RankingResultRowBuilder.BuildResultRows(context.Participants, context.Matches, result, context.FirstPlayerWinRatePercent);
            PrintResult(context.Participants.Count, result, context.FirstPlayerWinRatePercent, standardResultRows);
            if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
            {
                Console.WriteLine($"シミュレーションは時間上限 {Program.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
            }

            return result;
        }

        if (context.Matches.Count <= 20)
        {
            Console.WriteLine("本戦専用の厳密計算を行います。\n");
            var result = CalculateFinalStageExactly(context.Participants, context.Matches, context.GroupMap!, context.EffectiveAdditionalApexCount, context.BoundaryRescueMode, context.FirstPlayerWinRateRating);
            finalStageResultRows = RankingResultRowBuilder.BuildFinalStageResultRows(context.Participants, context.Matches, result, context.FirstPlayerWinRatePercent, context.GroupMap!, context.EffectiveAdditionalApexCount);
            PrintFinalStageResult(result, context.FirstPlayerWinRatePercent, finalStageResultRows);
            if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
            {
                Console.WriteLine($"シミュレーションは時間上限 {Program.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
            }

            return result;
        }

        const int finalStageDefaultSimulationCount = 200_000;
        var finalStageSimulationCount = ConsolePromptReaders.ReadIntWithDefault(
            $"局数が多いため本戦専用シミュレーションで近似します。試行回数を入力してください [{finalStageDefaultSimulationCount}]: ",
            finalStageDefaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var finalStageSimulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        var finalStageSimulationResult = CalculateFinalStageBySimulation(context.Participants, context.Matches, context.GroupMap!, context.EffectiveAdditionalApexCount, context.BoundaryRescueMode, context.FirstPlayerWinRateRating, finalStageSimulationCount);

        finalStageResultRows = RankingResultRowBuilder.BuildFinalStageResultRows(context.Participants, context.Matches, finalStageSimulationResult, context.FirstPlayerWinRatePercent, context.GroupMap!, context.EffectiveAdditionalApexCount);
        PrintFinalStageResult(finalStageSimulationResult, context.FirstPlayerWinRatePercent, finalStageResultRows);
        if (finalStageSimulationResult.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {Program.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        return finalStageSimulationResult;
    }

    static void PrintFinalStageModeContext(FinalStageModeContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");
        Console.WriteLine($"Apex / Innov の分け方: {FinalStageGroupingRule.GetLabel(context.GroupingMode)}\n");
        if (context.UsesFinalStageGrouping)
        {
            Console.WriteLine($"Apex: {context.ApexCount} 名");
            Console.WriteLine($"Innov: {context.InnovCount} 名\n");
            Console.WriteLine($"本戦不出場Apex: {context.AdditionalApexParticipants.Count} 名\n");
            Console.WriteLine($"本戦不出場Apexの扱い: {AdditionalApexPlacementRule.GetLabel(context.AdditionalApexPlacementMode)}\n");
            Console.WriteLine($"境界救済戦: {BoundaryRescueRule.GetLabel(context.BoundaryRescueMode)}\n");
        }
        else
        {
            Console.WriteLine($"対局者数: {context.Participants.Count} 名\n");
        }

        PrintMatchesCsv(context.Participants, context.Matches);
        Console.WriteLine($"本戦対局数: {context.Matches.Count}\n");
        if (context.ReferenceMatches.Count > 0)
        {
            PrintMatchesCsv(context.Participants, context.ReferenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {context.ReferenceMatches.Count}");
            Console.WriteLine("参考対局は順位計算に含めません。\n");
        }
    }

    static void WriteFinalStageModeOutputs(
        FinalStageModeContext context,
        CalculationResult result,
        IReadOnlyList<ResultRow>? standardResultRows,
        IReadOnlyList<FinalStageResultRow>? finalStageResultRows)
    {
        var defaultOutputCsvPath = Path.GetFullPath($"final_stage_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        var outputMarkdownPath = ChangeOutputExtension(outputCsvPath, ".md");
        var referenceMatchesCsvPath = context.ReferenceMatches.Count > 0
            ? BuildSiblingOutputCsvPath(outputCsvPath, "reference_matches")
            : null;
        if (context.GroupingMode == FinalStageGroupingMode.On)
        {
            WriterHelper.WriteText(
                outputPath: outputCsvPath,
                getLines: () => ResultCsvWriter.CreateFinalStageResultCsv(outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, finalStageResultRows!));

            WriterHelper.WriteText(
                outputPath: outputMarkdownPath,
                getLines: () => ResultCsvWriter.CreateFinalStageResultMarkdown(outputMarkdownPath, outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, finalStageResultRows!, referenceMatchesCsvPath));
        }
        else
        {
            WriterHelper.WriteText(
                outputPath: outputCsvPath,
                getLines: () => ResultCsvWriter.CreateResultCsv(result.Mode, context.FirstPlayerWinRatePercent, standardResultRows!));

            WriterHelper.WriteText(
                outputPath: outputMarkdownPath,
                getLines: () => ResultCsvWriter.CreateResultMarkdown(outputMarkdownPath, outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, standardResultRows!));

        }
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");

        if (context.ReferenceMatches.Count > 0)
        {
            WriteReferenceMatchCsv(referenceMatchesCsvPath!, context.Participants, context.ReferenceMatches);
            Console.WriteLine($"参考対局CSVを出力しました: {referenceMatchesCsvPath}");
        }
    }
}

