/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// TODO: 今は［標準版］とか［本戦版］とかに分かれているが、将来的には［ルールを一覧から選択する］ということを撤廃して、処理を主線１本に統合して、DSL ファイルを入力するようにしたいぜ（＾▽＾）！
/// </summary>
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
        return Instance.CreateFinalStageResultCsvCore(outputCsvPath, mode, firstPlayerWinRatePercent, resultRows, overviewNote);
    }

    /// <summary>
    /// TODO: これラッパー（＾～＾）？　剥がせないのかだぜ（＾～＾）？
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="overviewNote"></param>
    /// <param name="representativeRankingMarkdownPath"></param>
    /// <param name="referenceMatchesCsvPath"></param>
    /// <returns></returns>
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
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath);
    }
}
