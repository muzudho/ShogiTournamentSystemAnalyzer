using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;
using System.Globalization;

internal static partial class Program
{
    static void PrintQualityEvaluationContext(
        QualityEvaluationInput input,
        QualityEvaluationRuleDefinition ruleDefinition)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(ruleDefinition.TournamentRuleSetMode)}\n");
        Console.WriteLine($"Apex / Innov の分け方: {FinalStageGroupingRule.GetLabel(ruleDefinition.GroupingMode)}\n");
        if (ruleDefinition.UsesFinalStageGrouping)
        {
            Console.WriteLine($"本戦不出場Apexの扱い: {AdditionalApexPlacementRule.GetLabel(ruleDefinition.AdditionalApexPlacementMode)}\n");
            Console.WriteLine($"境界救済戦: {BoundaryRescueRule.GetLabel(ruleDefinition.BoundaryRescueMode)}\n");
            Console.WriteLine($"可変定員8ルール: {VariableTop8Rule.GetLabel(ruleDefinition.VariableTop8Mode)}\n");
            Console.WriteLine($"品質評価の Innov 比較基準順位補正: {QualityInnovExpectedRankOffsetRule.GetLabel(input.InnovExpectedRankOffsetMode)}\n");
        }

        if (input.ReferenceMatches.Count > 0)
        {
            PrintMatchesCsv(input.Participants, input.ReferenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {input.ReferenceMatches.Count}");
            Console.WriteLine("参考対局は品質評価に含めません。\n");
        }
    }

    static QualityEvaluationOutputOptions ReadQualitySummaryOutputOptions(QualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ReadExperimentalReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySummaryDefaultOutputPath(
            ruleDefinition.GroupingMode,
            ruleDefinition.AdditionalApexPlacementMode,
            ruleDefinition.BoundaryRescueMode,
            reportGroupingOptions,
            ruleDefinition.TournamentRuleSetMode);
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n品質評価サマリーCSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        return new QualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath);
    }

    static QualityEvaluationOutputOptions ReadQualitySweepOutputOptions(QualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ReadExperimentalReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySweepDefaultOutputPath(
            ruleDefinition.GroupingMode,
            ruleDefinition.AdditionalApexPlacementMode,
            ruleDefinition.BoundaryRescueMode,
            reportGroupingOptions,
            ruleDefinition.TournamentRuleSetMode);
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\nn%スイープ結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        return new QualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath);
    }

    static void WriteQualityEvaluationOutputs(
        TournamentQualityReportData tournamentQualityReportData,
        QualityEvaluationOutputOptions outputOptions)
    {
        WriterHelper.WriteText(
            outputPath: outputOptions.OutputCsvPath,
            getLines: () => CreateQualitySummaryCsv(tournamentQualityReportData.EvaluationRun.Summary, outputOptions.ReportGroupingOptions));

        var playerCsvPath = BuildSiblingOutputCsvPath(outputOptions.OutputCsvPath, "quality_players");
        WriterHelper.WriteText(
            outputPath: playerCsvPath,
            getLines: () => CreateQualityPlayerCsv(tournamentQualityReportData.EvaluationRun.PlayerRows));

        var summaryMarkdownPath = ChangeOutputExtension(outputOptions.OutputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: summaryMarkdownPath,
            getLines: () => CreateQualitySummaryMarkdown(summaryMarkdownPath, tournamentQualityReportData.EvaluationRun, outputOptions.OutputCsvPath, playerCsvPath, outputOptions.ReportGroupingOptions));

        Console.WriteLine($"品質評価サマリーCSVを出力しました: {outputOptions.OutputCsvPath}");
        Console.WriteLine($"品質評価選手別CSVを出力しました: {playerCsvPath}");
        Console.WriteLine($"品質評価サマリーMarkdownを出力しました: {summaryMarkdownPath}");
    }

    static void PrintQualitySweepRows(IReadOnlyList<QualitySweepRow> sweepRows)
    {
        Console.WriteLine("n%スイープ結果:");
        Console.WriteLine("先手勝率    Spearman   平均順位ずれ   上位8残留   Elo1位総合1位");

        foreach (var row in sweepRows)
        {
            Console.WriteLine(
                row.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture).PadLeft(8)
                + "%"
                + row.SpearmanCorrelation.ToString("F4", CultureInfo.InvariantCulture).PadLeft(12)
                + row.MeanAbsoluteRankError.ToString("F3", CultureInfo.InvariantCulture).PadLeft(14)
                + row.AverageTop8Retention.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
                + ((row.EloTop1OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture) + "%").PadLeft(16));
        }

        Console.WriteLine();
    }
}

