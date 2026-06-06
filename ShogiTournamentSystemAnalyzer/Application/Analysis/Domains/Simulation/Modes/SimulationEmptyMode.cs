/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class SimulationEmptyMode
{
    internal static void Run()
    {
        Console.WriteLine("対局シミュレーション / 空ルール: ペアリングを一切行わず、大会最終状態 0 件の最小結果を出力します。\n");
        ConsoleSamplePrinter.PrintSimulationEmptyOverview();
        RunMainlineToEmptyTournamentFinalState(null);
    }

    internal static void Run(string? outputPathOverride)
    {
        Console.WriteLine("対局シミュレーション / 空ルール: ペアリングを一切行わず、大会最終状態 0 件の最小結果を出力します。\n");
        RunMainlineToEmptyTournamentFinalState(outputPathOverride);
    }

    static void RunMainlineToEmptyTournamentFinalState(string? outputPathOverride)
    {
        ExecuteEmptyMode(outputPathOverride);
    }

    static void ExecuteEmptyMode(string? outputPathOverride)
    {
        const string mode = "空ルール / ペアリング0回 / 大会最終状態0件";
        const int pairingCount = 0;
        const int tournamentMatchRecordCount = 0;
        const string note = "このモードではペアリングを行わず、大会最終状態も 0 件です。";

        Console.WriteLine($"計算方法: {mode}\n");
        Console.WriteLine($"総ペアリング数: {pairingCount}");
        Console.WriteLine($"大会最終状態件数: {tournamentMatchRecordCount}\n");

        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath($"empty_rule_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var requestedOutputPath = string.IsNullOrWhiteSpace(outputPathOverride)
            ? ConsolePromptReaders.ReadTextWithDefault(
                $"空ルール結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
                defaultOutputCsvPath)
            : outputPathOverride!;
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(requestedOutputPath);
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        var tournamentMatchRecordsCsvPath = ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"empty_tournament_final_state_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var tournamentMatchRecordsMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(tournamentMatchRecordsCsvPath, ".md");

        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () => CreateEmptyRuleResultCsv(mode, pairingCount, tournamentMatchRecordCount, note));

        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () => CreateEmptyRuleResultMarkdown(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                pairingCount,
                tournamentMatchRecordCount,
                note,
                tournamentMatchRecordsMarkdownPath));

        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsCsvPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordCsv(
                Array.Empty<StageEntry>(),
                Array.Empty<PlayerEntry>(),
                Array.Empty<TournamentMatchRecord>(),
                overviewNote: note));

        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsMarkdownPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordMarkdown(
                tournamentMatchRecordsMarkdownPath,
                tournamentMatchRecordsCsvPath,
                Array.Empty<StageEntry>(),
                Array.Empty<PlayerEntry>(),
                Array.Empty<TournamentMatchRecord>(),
                overviewNote: note));

        Console.WriteLine($"空ルール結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"空ルール結果Markdownを出力しました: {outputMarkdownPath}");
        Console.WriteLine($"空ルール大会最終状態CSVを出力しました: {tournamentMatchRecordsCsvPath}");
        Console.WriteLine($"空ルール大会最終状態Markdownを出力しました: {tournamentMatchRecordsMarkdownPath}");
    }

    static IEnumerable<string> CreateEmptyRuleResultCsv(string mode, int pairingCount, int tournamentMatchRecordCount, string note)
    {
        return new[]
        {
            "calculationMode,pairingCount,tournamentMatchRecordCount,note",
            string.Join(",",
                CsvOutputHelpers.EscapeCsv(mode),
                pairingCount.ToString(),
                tournamentMatchRecordCount.ToString(),
                CsvOutputHelpers.EscapeCsv(note))
        };
    }

    static IEnumerable<string> CreateEmptyRuleResultMarkdown(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        int pairingCount,
        int tournamentMatchRecordCount,
        string note,
        string tournamentMatchRecordsMarkdownPath)
    {
        return new[]
        {
            "# 空ルール結果レポート",
            string.Empty,
            "## 概要",
            $"- 結果CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 計算モード: {mode}",
            $"- 総ペアリング数: {pairingCount}",
            $"- 大会最終状態件数: {tournamentMatchRecordCount}",
            $"- 大会最終状態Markdown: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, tournamentMatchRecordsMarkdownPath)}",
            $"- 注記: {note}",
            string.Empty,
            "## 説明",
            "- このモードでは対局を1件も組まないため、順位表は作りません。",
            "- 大会最終状態テーブルも 0 件のまま出力します。"
        };
    }
}
