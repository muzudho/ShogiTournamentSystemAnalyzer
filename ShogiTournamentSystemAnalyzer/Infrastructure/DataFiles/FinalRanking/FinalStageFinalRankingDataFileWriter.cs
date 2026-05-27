/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal sealed class FinalStageFinalRankingDataFileWriter
    : AbstractFinalRankingDataFileWriter
{
    static readonly FinalStageFinalRankingDataFileWriter Instance = new();

    internal static IEnumerable<string> CreateResultCsv(
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<FinalStageResultRow> resultRows,
        string? overviewNote = null)
    {
        return Instance.CreateFinalStageResultCsvCore(outputCsvPath, mode, firstPlayerWinRatePercent, resultRows);
    }

    internal static IEnumerable<string> CreateResultMarkdown(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<FinalStageResultRow> resultRows,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        return Instance.CreateFinalStageResultMarkdownCore(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            resultRows,
            referenceMatchesCsvPath);
    }
}
