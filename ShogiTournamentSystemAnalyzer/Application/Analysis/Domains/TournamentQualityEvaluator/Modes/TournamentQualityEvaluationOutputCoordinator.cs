/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
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
        var summaryMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        var requestInputLogPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".stsa.txt");
        var outputOptions = new TournamentQualityEvaluationOutputOptions(
            reportGroupingOptions,
            outputCsvPath,
            playerCsvPath,
            requestInputLogPath,
            ResolveOutputProfile(ruleDefinition));

        return outputOptions;
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
        return new TournamentQualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath, OutputProfile: ResolveOutputProfile(ruleDefinition));
    }


    static TournamentQualityEvaluationOutputProfile ResolveOutputProfile(TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        return ruleDefinition.UsesFinalStageGrouping
            ? TournamentQualityEvaluationOutputProfile.FinalStage
            : TournamentQualityEvaluationOutputProfile.Standard;
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

        WriteTournamentQualityReportRequestInputLog(outputOptions, summaryMarkdownPath);
        if (outputOptions.RequestInputLogPath is not null)
        {
            Console.WriteLine($"依頼ログSTSAを出力しました: {outputOptions.RequestInputLogPath}");
        }

        Console.WriteLine($"品質評価サマリーCSVを出力しました: {outputOptions.OutputCsvPath}");
        Console.WriteLine($"品質評価選手別CSVを出力しました: {playerCsvPath}");
        Console.WriteLine($"品質評価サマリーMarkdownを出力しました: {summaryMarkdownPath}");
    }


    static void WriteTournamentQualityReportRequestInputLog(
        TournamentQualityEvaluationOutputOptions outputOptions,
        string summaryMarkdownPath)
    {
        if (outputOptions.RequestInputLogPath is null) return;

        var ruleProfileAttributes = outputOptions.GetRuleProfileAttributes();
        WriterHelper.WriteText(
            outputPath: outputOptions.RequestInputLogPath,
            getLines: () => RequestInputLogFileWriter.CreateRequestInputLogLines(new
            {
                analysis_flow_steps = "QualityEvaluation",
                simulation_shape = ruleProfileAttributes.SimulationShape.ToString(),
                uses_final_stage_grouping = FormatOnOff(ruleProfileAttributes.UsesFinalStageGrouping),
                uses_additional_apex_placement = FormatOnOff(ruleProfileAttributes.UsesAdditionalApexPlacement),
                uses_boundary_rescue = FormatOnOff(ruleProfileAttributes.UsesBoundaryRescue),
                uses_variable_top8 = FormatOnOff(ruleProfileAttributes.UsesVariableTop8),
                ranking_rule_set_mode = ruleProfileAttributes.RankingRuleSetMode.ToString(),
                has_reference_matches = FormatOnOff(ruleProfileAttributes.HasReferenceMatches),
                pairing_source = ruleProfileAttributes.PairingSource.ToString(),
                execution_mode = "Single",
                tournament_rule_set_mode = ruleProfileAttributes.RankingRuleSetMode.ToString(),
                first_player_win_rate_percent = (double?)null,
                simulation_count = (int?)null,
                sweep_start_percent = (double?)null,
                sweep_end_percent = (double?)null,
                sweep_step_percent = (double?)null,
                additional_apex_placement_mode = (string?)null,
                boundary_rescue_mode = (string?)null,
                variable_top8_mode = (string?)null,
                quality_innov_expected_rank_offset_mode = (string?)null,
                tournament_quality_evaluation_report_grouping = outputOptions.ReportGroupingOptions.IsEnabled ? "On" : "Off",
                tournament_quality_evaluation_report_outcome = outputOptions.ReportGroupingOptions.IsEnabled ? outputOptions.ReportGroupingOptions.Outcome.ToString() : (string?)null,
                evaluation_memo = string.IsNullOrWhiteSpace(outputOptions.ReportGroupingOptions.EvaluationMemo) ? (string?)null : outputOptions.ReportGroupingOptions.EvaluationMemo,
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
    }

    static string FormatOnOff(bool value)
    {
        return value ? "On" : "Off";
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
