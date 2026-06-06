/*
 * ［アプリケーション　＞　ユースケース　＞　最終順位付け域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class FinalRankingDomain
{
    internal static (string OutputCsvPath, string OutputMarkdownPath) ResolveOutputPaths(string defaultFileName)
    {
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath(defaultFileName);
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        return (outputCsvPath, outputMarkdownPath);
    }

    internal static (string OutputCsvPath, string OutputMarkdownPath) ResolveOutputPaths(string defaultFileName, string? outputPathOverride)
    {
        return string.IsNullOrWhiteSpace(outputPathOverride)
            ? ResolveOutputPaths(defaultFileName)
            : ResolveOutputPathsFromOverride(outputPathOverride);
    }

    internal static (string OutputCsvPath, string OutputMarkdownPath) ResolveOutputPathsFromOverride(string outputPath)
    {
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(outputPath);
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        return (outputCsvPath, outputMarkdownPath);
    }

    internal static void PrintOutputCompleted(string outputCsvPath, string outputMarkdownPath)
    {
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");
    }

    internal static void WriteOutputFiles(
        string outputCsvPath,
        string outputMarkdownPath,
        Func<IEnumerable<string>> createCsvLines,
        Func<IEnumerable<string>> createMarkdownLines)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: createCsvLines);

        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: createMarkdownLines);
    }

    internal static void WriteOutputs(
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        Func<string, string, double, IReadOnlyList<GeneralSimulationResultRow>, IEnumerable<string>> createCsvLines,
        Func<string, string, string, double, IReadOnlyList<GeneralSimulationResultRow>, IEnumerable<string>> createMarkdownLines)
    {
        WriteOutputFiles(
            outputCsvPath,
            outputMarkdownPath,
            createCsvLines: () => createCsvLines(outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows),
            createMarkdownLines: () => createMarkdownLines(outputMarkdownPath, outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows));
    }

    internal static void WriteOutputs(
        FinalRankingMarkdownFileWriter finalRankingDataFileWriter,
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? referenceMatchesCsvPath = null)
    {
        WriteOutputs(
            outputCsvPath,
            outputMarkdownPath,
            result,
            firstPlayerWinRatePercent,
            resultRows,
            createCsvLines: (outputCsvPath, mode, firstPlayerWinRatePercent, resultRows) => new FinalRankingCsvFileWriter(finalRankingDataFileWriter.Settings).CreateResultCsvLines(
                mode,
                firstPlayerWinRatePercent,
                resultRows),
            createMarkdownLines: (outputMarkdownPath, outputCsvPath, mode, firstPlayerWinRatePercent, resultRows) => finalRankingDataFileWriter.CreateResultMarkdownCore(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                resultRows,
                referenceMatchesCsvPath: referenceMatchesCsvPath));
    }
}
