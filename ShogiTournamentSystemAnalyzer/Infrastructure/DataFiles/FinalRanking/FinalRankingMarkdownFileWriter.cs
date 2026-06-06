/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using Scriban;

/// <summary>
/// ［最終順位］境界のMarkdown形式データファイル書き出し処理
/// </summary>
internal class FinalRankingMarkdownFileWriter
{


    // ========================================
    // 生成
    // ========================================


    public FinalRankingMarkdownFileWriter(FinalRankingDataFileWriterSettings settings)
    {
        this.Settings = settings;
    }


    // ========================================
    // 構成
    // ========================================


    #region ［最終順位という境界］の設定

    /// <summary>
    /// ［最終順位という境界］の設定
    /// </summary>
    internal FinalRankingDataFileWriterSettings Settings { get; init; }

    #endregion

    #region ［最終順位という境界］のMarkdown形式データ

    /// <summary>
    /// ［最終順位という境界］のMarkdown形式データを作成する。
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
    /// <exception cref="InvalidOperationException"></exception>
    internal IEnumerable<string> CreateResultMarkdownCore(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        return SplitMarkdownLines(RenderMarkdownTemplate(FinalRankingMarkdownTemplateModelBuilder.Build(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            resultRows,
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath)));
    }

    static string RenderMarkdownTemplate(MarkdownTemplateModel model)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "DataFiles", "FinalRanking", "FinalRankingMarkdownTemplate.sbn.md");
        var templateText = File.ReadAllText(templatePath);
        var template = Template.Parse(templateText);
        if (template.HasErrors)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, template.Messages));
        }

        return template.Render(model);
    }

    static IEnumerable<string> SplitMarkdownLines(string markdownText)
    {
        using var reader = new StringReader(markdownText);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }

    #endregion
}
