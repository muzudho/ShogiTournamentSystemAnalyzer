/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes;

using ShogiTournamentSystemAnalyzer.Application.Paths;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityReport;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Request.TournamentRule;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentQualityReport;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class TournamentQualityEvaluationOutputCoordinator
{
    internal static void PrintTournamentQualityEvaluationContext(
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
            ConsoleResultPrinter.PrintMatchesCsv(input.Players, input.ReferenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {input.ReferenceMatches.Count}");
            Console.WriteLine("参考対局は品質評価に含めません。\n");
        }
    }

    internal static TournamentQualityEvaluationOutputOptions ReadTournamentQualityReportOutputOptions(TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ConsoleInputReaders.ReadTournamentQualityEvaluationReportGroupingOptions();
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildQualitySummaryDefaultOutputPath(
            ruleDefinition.GroupingMode,
            ruleDefinition.AdditionalApexPlacementMode,
            ruleDefinition.BoundaryRescueMode,
            reportGroupingOptions,
            ruleDefinition.TournamentRuleSetMode);
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n品質評価サマリーCSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        var playerCsvPath = ReportOutputPathBuilder.BuildTournamentQualityPlayersOutputPathFromSummary(outputCsvPath);
        return new TournamentQualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath, playerCsvPath);
    }

    internal static TournamentQualityEvaluationOutputOptions ReadTournamentQualitySweepReportOutputOptions(TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ConsoleInputReaders.ReadTournamentQualityEvaluationReportGroupingOptions();
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildTournamentQualitySweepReportDefaultOutputPath(
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

    internal static void WriteTournamentQualityReportOutputs(
        TournamentQualityReportData tournamentQualityReportData,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        WriterHelper.WriteText(
            outputPath: outputOptions.OutputCsvPath,
            getLines: () => TournamentQualityReportDataFileWriter.CreateTournamentQualityReportSummaryCsv(tournamentQualityReportData.Summary, outputOptions.ReportGroupingOptions, tournamentQualityReportData.Suggestion));

        var playerCsvPath = outputOptions.PlayerCsvPath
            ?? CsvOutputHelpers.BuildSiblingOutputCsvPath(outputOptions.OutputCsvPath, "quality_players");
        WriterHelper.WriteText(
            outputPath: playerCsvPath,
            getLines: () => TournamentQualityReportDataFileWriter.CreateTournamentQualityReportPlayerCsv(tournamentQualityReportData.PlayerRows));

        var summaryMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputOptions.OutputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: summaryMarkdownPath,
            getLines: () => TournamentQualityReportDataFileWriter.CreateTournamentQualityReportSummaryMarkdown(
                summaryMarkdownPath,
                tournamentQualityReportData.PlayerRows,
                tournamentQualityReportData.Summary,
                tournamentQualityReportData.CalculationMode,
                outputOptions.OutputCsvPath,
                playerCsvPath,
                outputOptions.ReportGroupingOptions,
                tournamentQualityReportData.Suggestion));

        var requestInputLogPath = CsvOutputHelpers.ChangeOutputExtension(outputOptions.OutputCsvPath, ".stsa.txt");
        WriterHelper.WriteText(
            outputPath: requestInputLogPath,
            getLines: () => RequestInputLogFileWriter.CreateRequestInputLogLines(new
            {
                analysis_flow_mode = "QualityEvaluation",
                rule_profile_mode = "StandardOrFinalStage",
                execution_mode = tournamentQualityReportData.CalculationMode.Contains("スイープ", StringComparison.Ordinal) ? "Sweep" : "Single",
                tournament_rule_set_mode = (string?)null,
                first_player_win_rate_percent = (double?)null,
                simulation_count = (int?)null,
                sweep_start_percent = (double?)null,
                sweep_end_percent = (double?)null,
                sweep_step_percent = (double?)null,
                additional_apex_placement_mode = (string?)null,
                boundary_rescue_mode = (string?)null,
                variable_top8_mode = (string?)null,
                quality_innov_expected_rank_offset_mode = (string?)null,
                tournament_quality_evaluation_report_grouping = outputOptions.ReportGroupingOptions.IsEnabled ? outputOptions.ReportGroupingOptions.Outcome.ToString() : "Off",
                tournament_quality_evaluation_report_outcome = outputOptions.ReportGroupingOptions.IsEnabled ? outputOptions.ReportGroupingOptions.Outcome.ToString() : (string?)null,
                evaluation_memo = tournamentQualityReportData.CalculationMode,
                players_csv = string.Empty,
                group_map_csv = (string?)null,
                additional_apex_players_csv = (string?)null,
                matches_input = string.Empty,
                reference_matches_input = (string?)null,
                summary_output_path = outputOptions.OutputCsvPath,
                sweep_output_path = (string?)null,
                player_csv_path = outputOptions.PlayerCsvPath,
                summary_markdown_path = summaryMarkdownPath,
                sweep_markdown_path = (string?)null
            }));

        Console.WriteLine($"品質評価サマリーCSVを出力しました: {outputOptions.OutputCsvPath}");
        Console.WriteLine($"品質評価選手別CSVを出力しました: {playerCsvPath}");
        Console.WriteLine($"品質評価サマリーMarkdownを出力しました: {summaryMarkdownPath}");
        Console.WriteLine($"依頼ログSTSAを出力しました: {requestInputLogPath}");
    }

    internal static void WriteTournamentQualitySweepReportOutputs(
        TournamentQualitySweepReportData tournamentQualitySweepReportData,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        WriterHelper.WriteText(
            outputPath: outputOptions.OutputCsvPath,
            getLines: () => TournamentQualityReportDataFileWriter.CreateTournamentQualitySweepReportCsv(tournamentQualitySweepReportData.SweepRows, outputOptions.ReportGroupingOptions, tournamentQualitySweepReportData.Suggestion));

        var sweepMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputOptions.OutputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: sweepMarkdownPath,
            getLines: () => TournamentQualityReportDataFileWriter.CreateTournamentQualitySweepReportMarkdown(sweepMarkdownPath, tournamentQualitySweepReportData.SweepRows, outputOptions.OutputCsvPath, outputOptions.ReportGroupingOptions, tournamentQualitySweepReportData.StoppedByTimeout, tournamentQualitySweepReportData.Suggestion));

        Console.WriteLine($"n%スイープ結果CSVを出力しました: {outputOptions.OutputCsvPath}");
        Console.WriteLine($"n%スイープ結果Markdownを出力しました: {sweepMarkdownPath}");
    }

    internal static void PrintTournamentQualitySweepReportTable(IReadOnlyList<TournamentQualitySweepReportRow> sweepRows)
    {
        ConsoleResultPrinter.PrintTournamentQualitySweepReportTable(sweepRows);
    }
}
