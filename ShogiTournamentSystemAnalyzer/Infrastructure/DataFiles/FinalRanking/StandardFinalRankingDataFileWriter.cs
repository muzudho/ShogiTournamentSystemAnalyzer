/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal sealed class StandardFinalRankingDataFileWriter
    : AbstractFinalRankingDataFileWriter
{
    static readonly StandardFinalRankingDataFileWriter Instance = new();

    internal static IEnumerable<string> CreateResultCsv(
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<ResultRow> resultRows,
        string? overviewNote = null)
    {
        return Instance.CreateResultCsvCore(mode, firstPlayerWinRatePercent, resultRows, overviewNote);
    }

    internal static IEnumerable<string> CreateResultMarkdown(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<ResultRow> resultRows,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        return Instance.CreateResultMarkdownCore(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            resultRows,
            overviewNote,
            representativeRankingMarkdownPath);
    }
}
