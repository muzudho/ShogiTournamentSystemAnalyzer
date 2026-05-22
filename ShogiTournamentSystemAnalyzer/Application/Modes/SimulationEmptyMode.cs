/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;

internal static partial class Program
{
    static void RunEmptyMode()
    {
        Console.WriteLine("対局シミュレーション / 空ルール: ペアリングを一切行わず、大会結果 0 件の最小結果を出力します。\n");
        ConsoleSamplePrinter.PrintSimulationEmptyOverview();
        ExecuteEmptyMode();
    }

    static void ExecuteEmptyMode()
    {
        const string mode = "空ルール / ペアリング0回 / 大会結果0件";
        const int pairingCount = 0;
        const int tournamentMatchRecordCount = 0;
        const string note = "このモードではペアリングを行わず、大会結果も 0 件です。";

        Console.WriteLine($"計算方法: {mode}\n");
        Console.WriteLine($"総ペアリング数: {pairingCount}");
        Console.WriteLine($"大会結果件数: {tournamentMatchRecordCount}\n");

        var defaultOutputCsvPath = Path.GetFullPath($"empty_rule_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"空ルール結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        var outputMarkdownPath = ChangeOutputExtension(outputCsvPath, ".md");
        var tournamentMatchRecordsCsvPath = BuildSiblingOutputCsvPath(outputCsvPath, "tournament_match_records_empty");
        var tournamentMatchRecordsMarkdownPath = ChangeOutputExtension(tournamentMatchRecordsCsvPath, ".md");

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
            getLines: () => ResultCsvWriter.CreateTournamentMatchRecordCsv(
                Array.Empty<StageEntry>(),
                Array.Empty<PlayerEntry>(),
                Array.Empty<TournamentMatchRecord>(),
                overviewNote: note));

        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsMarkdownPath,
            getLines: () => ResultCsvWriter.CreateTournamentMatchRecordMarkdown(
                tournamentMatchRecordsMarkdownPath,
                tournamentMatchRecordsCsvPath,
                Array.Empty<StageEntry>(),
                Array.Empty<PlayerEntry>(),
                Array.Empty<TournamentMatchRecord>(),
                overviewNote: note));

        Console.WriteLine($"空ルール結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"空ルール結果Markdownを出力しました: {outputMarkdownPath}");
        Console.WriteLine($"空ルール大会結果CSVを出力しました: {tournamentMatchRecordsCsvPath}");
        Console.WriteLine($"空ルール大会結果Markdownを出力しました: {tournamentMatchRecordsMarkdownPath}");
    }

    static IEnumerable<string> CreateEmptyRuleResultCsv(string mode, int pairingCount, int tournamentMatchRecordCount, string note)
    {
        return new[]
        {
            "calculationMode,pairingCount,tournamentMatchRecordCount,note",
            string.Join(",",
                EscapeCsv(mode),
                pairingCount.ToString(),
                tournamentMatchRecordCount.ToString(),
                EscapeCsv(note))
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
            $"- 結果CSV: {ResultCsvWriter.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 計算モード: {mode}",
            $"- 総ペアリング数: {pairingCount}",
            $"- 大会結果件数: {tournamentMatchRecordCount}",
            $"- 大会結果Markdown: {ResultCsvWriter.BuildMarkdownFileLink(outputMarkdownPath, tournamentMatchRecordsMarkdownPath)}",
            $"- 注記: {note}",
            string.Empty,
            "## 説明",
            "- このモードでは対局を1件も組まないため、順位表は作りません。",
            "- 大会結果テーブルも 0 件のまま出力します。"
        };
    }
}
