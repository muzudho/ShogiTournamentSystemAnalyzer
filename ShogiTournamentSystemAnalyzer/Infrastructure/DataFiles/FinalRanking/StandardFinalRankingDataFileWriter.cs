/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// TODO: 今は［標準版］とか［本戦版］とかに分かれているが、将来的には［ルールを一覧から選択する］ということを撤廃して、処理を主線１本に統合して、DSL ファイルを入力するようにしたいぜ（＾▽＾）！
/// </summary>
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
        return Instance.CreateResultCsvCore(outputCsvPath, mode, firstPlayerWinRatePercent, resultRows, overviewNote);
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
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath);
    }
}
