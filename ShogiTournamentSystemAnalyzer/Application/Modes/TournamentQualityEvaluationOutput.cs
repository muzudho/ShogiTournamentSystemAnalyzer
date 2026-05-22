/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;
using System.Globalization;

internal static partial class Program
{
    /// <summary>
    /// ［大会品質評価フロー］
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ruleDefinition"></param>
    static void PrintTournamentQualityEvaluationContext(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(ruleDefinition.TournamentRuleSetMode)}\n");
        Console.WriteLine($"Apex / Innov の分け方: {FinalStageGroupingRule.GetLabel(ruleDefinition.GroupingMode)}\n");
        if (ruleDefinition.UsesFinalStageGrouping)
        {
            Console.WriteLine($"本戦不出場Apexの扱い: {AdditionalApexPlacementRule.GetLabel(ruleDefinition.AdditionalApexPlacementMode)}\n");
            Console.WriteLine($"境界救済戦: {BoundaryRescueRule.GetLabel(ruleDefinition.BoundaryRescueMode)}\n");
            Console.WriteLine($"可変定員8ルール: {VariableTop8Rule.GetLabel(ruleDefinition.VariableTop8Mode)}\n");
            Console.WriteLine($"品質評価の Innov 比較基準順位補正: {TournamentQualityEvaluationInnovExpectedRankOffsetRule.GetLabel(input.InnovExpectedRankOffsetMode)}\n");
        }

        if (input.ReferenceMatches.Count > 0)
        {
            ConsoleResultPrinter.PrintMatchesCsv(input.Participants, input.ReferenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {input.ReferenceMatches.Count}");
            Console.WriteLine("参考対局は品質評価に含めません。\n");
        }
    }

    /// <summary>
    /// ［大会品質レポート］
    /// </summary>
    /// <param name="ruleDefinition"></param>
    /// <returns></returns>
    static TournamentQualityEvaluationOutputOptions ReadTournamentQualityReportOutputOptions(TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ConsoleInputReaders.ReadTournamentQualityEvaluationReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySummaryDefaultOutputPath(
            ruleDefinition.GroupingMode,
            ruleDefinition.AdditionalApexPlacementMode,
            ruleDefinition.BoundaryRescueMode,
            reportGroupingOptions,
            ruleDefinition.TournamentRuleSetMode);
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n品質評価サマリーCSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        return new TournamentQualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath);
    }

    /// <summary>
    /// ［大会品質レポート］
    /// </summary>
    /// <param name="ruleDefinition"></param>
    /// <returns></returns>
    static TournamentQualityEvaluationOutputOptions ReadTournamentQualitySweepReportOutputOptions(TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ConsoleInputReaders.ReadTournamentQualityEvaluationReportGroupingOptions();
        var defaultOutputCsvPath = BuildTournamentQualitySweepReportDefaultOutputPath(
            ruleDefinition.GroupingMode,
            ruleDefinition.AdditionalApexPlacementMode,
            ruleDefinition.BoundaryRescueMode,
            reportGroupingOptions,
            ruleDefinition.TournamentRuleSetMode);
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\nn%スイープ結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        return new TournamentQualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath);
    }

    /// <summary>
    /// ［大会品質レポート］
    /// </summary>
    /// <param name="tournamentQualityReportData"></param>
    /// <param name="outputOptions"></param>
    static void WriteTournamentQualityReportOutputs(
        TournamentQualityReportData tournamentQualityReportData,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        WriterHelper.WriteText(
            outputPath: outputOptions.OutputCsvPath,
            getLines: () => ResultCsvWriter.CreateTournamentQualityReportSummaryCsv(tournamentQualityReportData.Summary, outputOptions.ReportGroupingOptions));

        var playerCsvPath = CsvOutputHelpers.BuildSiblingOutputCsvPath(outputOptions.OutputCsvPath, "quality_players");
        WriterHelper.WriteText(
            outputPath: playerCsvPath,
            getLines: () => ResultCsvWriter.CreateTournamentQualityReportPlayerCsv(tournamentQualityReportData.PlayerRows));

        var summaryMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputOptions.OutputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: summaryMarkdownPath,
            getLines: () => ResultCsvWriter.CreateTournamentQualityReportSummaryMarkdown(
                summaryMarkdownPath,
                tournamentQualityReportData.PlayerRows,
                tournamentQualityReportData.Summary,
                tournamentQualityReportData.CalculationMode,
                outputOptions.OutputCsvPath,
                playerCsvPath,
                outputOptions.ReportGroupingOptions));

        Console.WriteLine($"品質評価サマリーCSVを出力しました: {outputOptions.OutputCsvPath}");
        Console.WriteLine($"品質評価選手別CSVを出力しました: {playerCsvPath}");
        Console.WriteLine($"品質評価サマリーMarkdownを出力しました: {summaryMarkdownPath}");
    }

    /// <summary>
    /// ［大会品質レポート］
    /// </summary>
    /// <param name="tournamentQualitySweepReportData"></param>
    /// <param name="outputOptions"></param>
    static void WriteTournamentQualitySweepReportOutputs(
        TournamentQualitySweepReportData tournamentQualitySweepReportData,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        WriterHelper.WriteText(
            outputPath: outputOptions.OutputCsvPath,
            getLines: () => ResultCsvWriter.CreateTournamentQualitySweepReportCsv(tournamentQualitySweepReportData.SweepRows, outputOptions.ReportGroupingOptions));

        var sweepMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputOptions.OutputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: sweepMarkdownPath,
            getLines: () => ResultCsvWriter.CreateTournamentQualitySweepReportMarkdown(sweepMarkdownPath, tournamentQualitySweepReportData.SweepRows, outputOptions.OutputCsvPath, outputOptions.ReportGroupingOptions));

        Console.WriteLine($"n%スイープ結果CSVを出力しました: {outputOptions.OutputCsvPath}");
        Console.WriteLine($"n%スイープ結果Markdownを出力しました: {sweepMarkdownPath}");
    }

    /// <summary>
    /// ［大会品質レポート］
    /// </summary>
    /// <param name="sweepRows"></param>
    static void PrintTournamentQualitySweepReportTable(IReadOnlyList<TournamentQualitySweepReportRow> sweepRows)
    {
        ConsoleResultPrinter.PrintTournamentQualitySweepReportTable(sweepRows);
    }
}

