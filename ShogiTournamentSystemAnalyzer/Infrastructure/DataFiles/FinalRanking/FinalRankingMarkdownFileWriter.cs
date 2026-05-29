/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Application.DataDefinitions;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using Scriban;
using System.Globalization;

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
    /// <typeparam name="TRow"></typeparam>
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
    internal IEnumerable<string> CreateResultMarkdownCore<TRow>(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
        where TRow : ISimulationResultRow
    {
        return resultRows switch
        {
            IReadOnlyList<StandardResultRow> standardRows => SplitMarkdownLines(RenderMarkdownTemplate(BuildStandardMarkdownTemplateModel(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                standardRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath))),
            IReadOnlyList<FinalStageResultRow> finalStageRows => SplitMarkdownLines(RenderMarkdownTemplate(BuildFinalStageMarkdownTemplateModel(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                finalStageRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath))),
            _ => throw new InvalidOperationException($"未対応の結果行型: {typeof(TRow).FullName}")
        };
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

    static TRow[] SelectTopRows<TRow>(
        IEnumerable<TRow> rows,
        Func<TRow, double> primaryDescending,
        Func<TRow, double> secondaryAscending,
        int takeCount)
        where TRow : ISimulationResultRow
    {
        return rows
            .OrderByDescending(primaryDescending)
            .ThenBy(secondaryAscending)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(takeCount)
            .ToArray();
    }

    static bool TrySelectBestRow<TRow>(
        IEnumerable<TRow> rows,
        Func<TRow, double> primaryDescending,
        Func<TRow, double> secondaryAscending,
        out TRow bestRow)
        where TRow : ISimulationResultRow
    {
        using var enumerator = rows
            .OrderByDescending(primaryDescending)
            .ThenBy(secondaryAscending)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .GetEnumerator();

        if (!enumerator.MoveNext())
        {
            bestRow = default!;
            return false;
        }

        bestRow = enumerator.Current;
        return true;
    }

    static TRow[] SelectTopRowsByGroup<TRow>(
        IEnumerable<TRow> rows,
        Func<TRow, bool> predicate,
        Func<TRow, double> primaryDescending,
        Func<TRow, double> secondaryAscending,
        int takeCount)
        where TRow : ISimulationResultRow
    {
        return SelectTopRows(rows.Where(predicate), primaryDescending, secondaryAscending, takeCount);
    }

    static IEnumerable<string> BuildMarkdownTableRows<TRow>(IEnumerable<TRow> rows, Func<TRow, string> formatter)
    {
        return rows.Select(formatter);
    }

    static string? BuildOptionalMarkdownLink(string markdownPath, string? targetPath)
    {
        return string.IsNullOrWhiteSpace(targetPath)
            ? null
            : MarkdownOutputHelpers.BuildMarkdownFileLink(markdownPath, targetPath);
    }

    sealed record class MarkdownTemplateModel
    {
        public string OutputCsvLink { get; init; } = string.Empty;
        public string EditionLabel { get; init; } = string.Empty;
        public string Mode { get; init; } = string.Empty;
        public string FirstPlayerWinRatePercent { get; init; } = string.Empty;
        public int PlayerCount { get; init; }
        public string? OverviewNote { get; init; }
        public string? RepresentativeRankingMarkdownLink { get; init; }
        public string? ReferenceMatchesCsvLink { get; init; }
        public IEnumerable<string> AttentionPoints { get; init; } = [];
        public IEnumerable<string> AutoComments { get; init; } = [];
        public string PrimaryTableHeader { get; init; } = string.Empty;
        public string PrimaryTableHeaderSeparator { get; init; } = string.Empty;
        public IEnumerable<string> PrimaryTableRows { get; init; } = [];
        public IEnumerable<MarkdownTemplateSection> TrailingSections { get; init; } = [];
        public IEnumerable<FinalRankingMarkdownChartSpec> Charts { get; init; } = [];
    }

    sealed record class MarkdownTemplateSection(string Title, string TableHeader, IEnumerable<string> Rows);

    static MarkdownTemplateModel BuildMarkdownTemplateModel<TRow>(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
        where TRow : ISimulationResultRow
    {
        return resultRows switch
        {
            IReadOnlyList<StandardResultRow> standardRows => BuildStandardMarkdownTemplateModel(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                standardRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath),
            IReadOnlyList<FinalStageResultRow> finalStageRows => BuildFinalStageMarkdownTemplateModel(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                finalStageRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath),
            _ => throw new InvalidOperationException($"未対応の結果行型: {typeof(TRow).FullName}")
        };
    }

    static MarkdownTemplateModel BuildStandardMarkdownTemplateModel(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<StandardResultRow> resultRows,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        var topChampionshipRows = SelectTopRows(resultRows, row => row.ChampionshipProbability, row => row.AveragePlace, takeCount: 8);
        var hasBestChampionshipRow = TrySelectBestRow(resultRows, row => row.ChampionshipProbability, row => row.AveragePlace, out var bestChampionshipRow);
        var hasBestAveragePlaceRow = TrySelectBestRow(resultRows, row => -row.AveragePlace, row => -row.ChampionshipProbability, out var bestAveragePlaceRow);
        var hasBiggestBoostRow = TrySelectBestRow(resultRows, row => row.RatingDelta, row => 0, out var biggestBoostRow);
        var hasBiggestDropRow = TrySelectBestRow(resultRows, row => -row.RatingDelta, row => 0, out var biggestDropRow);

        return BuildMarkdownTemplateBaseModel(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            resultRows.Count,
            editionLabel: "標準版",
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath) with
        {
            AttentionPoints =
            [
                $"優勝確率が最も高い選手: **{(hasBestChampionshipRow ? bestChampionshipRow.Name : "該当なし")}**（{((hasBestChampionshipRow ? bestChampionshipRow.ChampionshipProbability : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"平均順位が最も良い選手: **{(hasBestAveragePlaceRow ? bestAveragePlaceRow.Name : "該当なし")}**（{(hasBestAveragePlaceRow ? bestAveragePlaceRow.AveragePlace : 0).ToString("F3", CultureInfo.InvariantCulture)}）",
                $"実効Elo差分が最も大きくプラスの選手: **{(hasBiggestBoostRow ? biggestBoostRow.Name : "該当なし")}**（{SimulationRatingMath.FormatSignedRating(hasBiggestBoostRow ? biggestBoostRow.RatingDelta : 0)}）",
                $"実効Elo差分が最も大きくマイナスの選手: **{(hasBiggestDropRow ? biggestDropRow.Name : "該当なし")}**（{SimulationRatingMath.FormatSignedRating(hasBiggestDropRow ? biggestDropRow.RatingDelta : 0)}）"
            ],
            AutoComments =
            [
                $"優勝候補の強さ: {BuildTop1Comment(hasBestChampionshipRow ? bestChampionshipRow.ChampionshipProbability : 0)}",
                $"先頭の平均順位: {BuildAveragePlaceComment(hasBestAveragePlaceRow ? bestAveragePlaceRow.AveragePlace : 0)}",
                $"実効Eloの押し上げ: {BuildRatingDeltaComment(hasBiggestBoostRow ? biggestBoostRow.RatingDelta : 0, hasBiggestDropRow ? biggestDropRow.RatingDelta : 0)}"
            ],
            PrimaryTableHeader = "| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |",
            PrimaryTableHeaderSeparator = "| --- | ---: | ---: | ---: | ---: | ---: |",
            PrimaryTableRows = BuildMarkdownTableRows(topChampionshipRows, row =>
                $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} |"),
            TrailingSections = [],
            Charts = BuildChartSpecs(topChampionshipRows,
                ("上位候補の優勝確率", "優勝確率(%)", "0 --> 100", row => (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)),
                ("上位候補の平均順位", "平均順位", "1 --> " + Math.Max(2, resultRows.Count).ToString(CultureInfo.InvariantCulture), row => row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)))
        };
    }

    static MarkdownTemplateModel BuildFinalStageMarkdownTemplateModel(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<FinalStageResultRow> resultRows,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        var apexRows = SelectTopRowsByGroup(resultRows, row => string.Equals(row.Group, "Apex", StringComparison.OrdinalIgnoreCase), row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, takeCount: 4);
        var innovRows = SelectTopRowsByGroup(resultRows, row => string.Equals(row.Group, "Innov", StringComparison.OrdinalIgnoreCase), row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, takeCount: 4);
        var hasBestOverallRow = TrySelectBestRow(resultRows, row => row.OverallPlace1Probability, row => row.OverallPlaceAverage, out var bestOverallRow);
        var hasBestApexRow = TrySelectBestRow(apexRows, row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, out var bestApexRow);
        var hasBestInnovRow = TrySelectBestRow(innovRows, row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, out var bestInnovRow);

        var primaryRows = SelectTopRows(resultRows, row => row.OverallPlace1Probability, row => row.OverallPlaceAverage, takeCount: 8);

        var trailingSections = new List<MarkdownTemplateSection>();
        if (apexRows.Length > 0)
        {
            trailingSections.Add(new MarkdownTemplateSection(
                Title: "## Apex 注目候補",
                TableHeader: "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |\n| --- | ---: | ---: | ---: | ---: | ---: |",
                Rows: BuildMarkdownTableRows(apexRows, row =>
                    $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |")));
        }

        if (innovRows.Length > 0)
        {
            trailingSections.Add(new MarkdownTemplateSection(
                Title: "## Innov 注目候補",
                TableHeader: "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |\n| --- | ---: | ---: | ---: | ---: | ---: |",
                Rows: BuildMarkdownTableRows(innovRows, row =>
                    $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |")));
        }

        return BuildMarkdownTemplateBaseModel(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            resultRows.Count,
            editionLabel: "本戦版",
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath) with
        {
            AttentionPoints =
            [
                $"総合1位確率が最も高い選手: **{(hasBestOverallRow ? bestOverallRow.Name : "該当なし")}**（{((hasBestOverallRow ? bestOverallRow.OverallPlace1Probability : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"Apex で最も有力な選手: **{(hasBestApexRow ? bestApexRow.Name : "該当なし")}**（グループ1位確率 {((hasBestApexRow ? bestApexRow.GroupPlace1Probability : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"Innov で最も有力な選手: **{(hasBestInnovRow ? bestInnovRow.Name : "該当なし")}**（グループ1位確率 {((hasBestInnovRow ? bestInnovRow.GroupPlace1Probability : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）"
            ],
            AutoComments =
            [
                $"総合1位候補の強さ: {BuildTop1Comment(hasBestOverallRow ? bestOverallRow.OverallPlace1Probability : 0)}",
                $"Apex の先頭感: {BuildGroupLeadComment(hasBestApexRow ? bestApexRow.GroupPlace1Probability : 0, hasBestApexRow ? bestApexRow.GroupPlaceAverage : 0)}",
                $"Innov の先頭感: {BuildGroupLeadComment(hasBestInnovRow ? bestInnovRow.GroupPlace1Probability : 0, hasBestInnovRow ? bestInnovRow.GroupPlaceAverage : 0)}",
                $"Apex / Innov の先頭差: {BuildApexInnovGapComment(hasBestApexRow ? bestApexRow.GroupPlace1Probability : 0, hasBestInnovRow ? bestInnovRow.GroupPlace1Probability : 0)}"
            ],
            PrimaryTableHeader = "| 選手 | グループ | 元Elo | 実効Elo | 差分 | グループ1位確率 | 総合1位確率 | 総合平均順位 |",
            PrimaryTableHeaderSeparator = "| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |",
            PrimaryTableRows = BuildMarkdownTableRows(primaryRows, row =>
                $"| {row.Name} | {row.Group} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"),
            TrailingSections = trailingSections.ToArray(),
            Charts = BuildChartSpecs(primaryRows,
                ("上位候補の総合1位確率", "総合1位確率(%)", "0 --> 100", row => (row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)),
                ("上位候補のグループ1位確率", "グループ1位確率(%)", "0 --> 100", row => (row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)))
        };
    }

    static MarkdownTemplateModel BuildMarkdownTemplateBaseModel(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        int playerCount,
        string editionLabel,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        return new MarkdownTemplateModel
        {
            OutputCsvLink = MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath),
            EditionLabel = editionLabel,
            Mode = mode,
            FirstPlayerWinRatePercent = firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
            PlayerCount = playerCount,
            OverviewNote = overviewNote,
            RepresentativeRankingMarkdownLink = BuildOptionalMarkdownLink(outputMarkdownPath, representativeRankingMarkdownPath),
            ReferenceMatchesCsvLink = BuildOptionalMarkdownLink(outputMarkdownPath, referenceMatchesCsvPath),
            AttentionPoints = [],
            AutoComments = [],
            PrimaryTableHeader = string.Empty,
            PrimaryTableHeaderSeparator = string.Empty,
            PrimaryTableRows = [],
            TrailingSections = [],
            Charts = []
        };
    }

    /// <summary>
    /// ［最終順位という境界］のマークダウンの［概要］セクション各行を組立
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="playerCount"></param>
    /// <param name="editionLabel"></param>
    /// <param name="overviewNote"></param>
    /// <param name="representativeRankingMarkdownPath"></param>
    /// <param name="referenceMatchesCsvPath"></param>
    /// <returns></returns>
    protected static List<string> BuildFinalRankingMarkdownOverviewLines(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        int playerCount,
        string editionLabel,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        var lines = new List<string>
        {
            "## 概要",
            $"- 結果CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 版: {editionLabel}",
            $"- 計算モード: {mode}",
            $"- 同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%",
            $"- 対象選手数: {playerCount}"
        };

        if (!string.IsNullOrWhiteSpace(representativeRankingMarkdownPath))
        {
            lines.Add($"- representative順位表: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, representativeRankingMarkdownPath)}");
        }

        if (!string.IsNullOrWhiteSpace(referenceMatchesCsvPath))
        {
            lines.Add($"- 参考対局CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, referenceMatchesCsvPath)}");
        }

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            lines.Add($"- 注記: {overviewNote}");
        }

        return lines;
    }

    /// <summary>
    /// ［最終順位という境界］のマークダウン各行作成
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="playerCount"></param>
    /// <param name="editionLabel"></param>
    /// <param name="overviewNote"></param>
    /// <param name="representativeRankingMarkdownPath"></param>
    /// <param name="referenceMatchesCsvPath"></param>
    /// <returns></returns>
    static List<string> CreateFinalRankingMarkdownLines(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        int playerCount,
        string editionLabel,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        var lines = new List<string>
        {
            "# 最終順位結果レポート",
            string.Empty
        };

        lines.AddRange(BuildFinalRankingMarkdownOverviewLines(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            playerCount,
            editionLabel,
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath));

        return lines;
    }

    /// <summary>
    /// セクション追加
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="sections"></param>
    static void AddMarkdownSection(List<string> lines, params IEnumerable<string>[] sections)
    {
        foreach (var section in sections)
        {
            lines.Add(string.Empty);
            lines.AddRange(section);
        }
    }

    /// <summary>
    /// Mermaid のコード組立
    /// </summary>
    /// <param name="title"></param>
    /// <param name="categories"></param>
    /// <param name="yAxisLabel"></param>
    /// <param name="yAxisRange"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    static IReadOnlyList<string> BuildMermaidXychartLines(
        string title,
        IEnumerable<string> categories,
        string yAxisLabel,
        string yAxisRange,
        IEnumerable<string> values)
    {
        return
        [
            "```mermaid",
            "xychart-beta",
            $"    title \"{title}\"",
            "    x-axis [" + MarkdownOutputHelpers.BuildMermaidCategoryList(categories) + "]",
            $"    y-axis \"{yAxisLabel}\" {yAxisRange}",
            "    bar [" + string.Join(", ", values) + "]",
            "```"
        ];
    }

    readonly record struct FinalRankingMarkdownChartSpec(
        string Title,
        IEnumerable<string> Categories,
        string YAxisLabel,
        string YAxisRange,
        IEnumerable<string> Values);

    static FinalRankingMarkdownChartSpec[] BuildChartSpecs<TRow>(
        IReadOnlyList<TRow> rows,
        params (string Title, string YAxisLabel, string YAxisRange, Func<TRow, string> ValueSelector)[] chartDefinitions)
        where TRow : ISimulationResultRow
    {
        if (rows.Count == 0) return [];

        return chartDefinitions
            .Select(chart => new FinalRankingMarkdownChartSpec(
                chart.Title,
                rows.Select(row => row.Name),
                chart.YAxisLabel,
                chart.YAxisRange,
                rows.Select(chart.ValueSelector)))
            .ToArray();
    }

    static void AddMarkdownChartsSection(List<string> lines, params FinalRankingMarkdownChartSpec[] charts)
    {
        if (charts.Length == 0) return;

        lines.Add(string.Empty);
        lines.Add("## Mermaid 図");

        for (var index = 0; index < charts.Length; index++)
        {
            if (index > 0)
            {
                lines.Add(string.Empty);
            }

            var chart = charts[index];
            lines.AddRange(BuildMermaidXychartLines(
                chart.Title,
                chart.Categories,
                chart.YAxisLabel,
                chart.YAxisRange,
                chart.Values));
        }
    }

    static IEnumerable<string> CreateFinalRankingMarkdownReport(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        int playerCount,
        string editionLabel,
        IEnumerable<string>[] primarySections,
        IEnumerable<string> primaryTableRows,
        IEnumerable<string>[] trailingSections,
        FinalRankingMarkdownChartSpec[] charts,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        var lines = CreateFinalRankingMarkdownLines(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            playerCount,
            editionLabel,
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath);

        AddMarkdownSection(lines, primarySections);
        lines.AddRange(primaryTableRows);
        AddMarkdownSection(lines, trailingSections);
        AddMarkdownChartsSection(lines, charts);
        return lines;
    }

    #endregion

    #region ［順位］の［表型］

    const string RepresentativeExecutionRankTableTypeFileName = "RepresentativeExecutionRankTableType.json";

    static IReadOnlyList<string>? representativeExecutionRankFixedColumns;

    internal static IEnumerable<string> CreateRepresentativeExecutionRankCsv(
        TournamentRuleSetMode tournamentRuleSetMode,
        IReadOnlyList<RepresentativeExecutionRankRow> rows,
        string? overviewNote = null)
    {
        var specificHeaderColumns = GetRepresentativeExecutionRankFixedColumns().ToList();
        if (string.IsNullOrWhiteSpace(overviewNote)) specificHeaderColumns.Remove("note");

        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(specificHeaderColumns).Select(CsvOutputHelpers.EscapeCsv))
        };

        foreach (var row in rows)
        {
            var specificColumns = new List<string>
            {
                TournamentRuleSetRule.GetLabel(tournamentRuleSetMode),
                row.Name,
                row.Points.ToString(CultureInfo.InvariantCulture),
                row.RankLabel,
                row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture),
                (row.FirstPlaceProbability * 100).ToString("F2", CultureInfo.InvariantCulture)
            };

            if (!string.IsNullOrWhiteSpace(overviewNote))
            {
                specificColumns.Add(overviewNote);
            }

            var columns = CsvSchemaCommonColumns.BuildRowColumns(
                boundaryName: "FinalRanking",
                schemaName: "representativeExecutionRank",
                rowType: "data",
                specificColumns.ToArray());

            lines.Add(string.Join(",", columns.Select(CsvOutputHelpers.EscapeCsv)));
        }

        return lines;

        // ローカル関数

        static IReadOnlyList<string> GetRepresentativeExecutionRankFixedColumns()
        {
            return representativeExecutionRankFixedColumns ??= LoadFixedColumns(RepresentativeExecutionRankTableTypeFileName);
        }
    }

    internal static IEnumerable<string> CreateRepresentativeExecutionRankMarkdown(
        string outputMarkdownPath,
        string outputCsvPath,
        TournamentRuleSetMode tournamentRuleSetMode,
        IReadOnlyList<RepresentativeExecutionRankRow> rows,
        string? overviewNote = null,
        string? representativeMatchRecordsMarkdownPath = null)
    {
        var bestRow = rows
            .OrderBy(row => row.AveragePlace)
            .ThenByDescending(row => row.Points)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        var lines = new List<string>
        {
            "# representative順位表",
            string.Empty,
            "## 概要",
            $"- 結果CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 順位ルール: {TournamentRuleSetRule.GetLabel(tournamentRuleSetMode)}",
            $"- 対象選手数: {rows.Count}"
        };

        if (!string.IsNullOrWhiteSpace(representativeMatchRecordsMarkdownPath))
        {
            lines.Add($"- representative大会最終状態: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, representativeMatchRecordsMarkdownPath)}");
        }

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            lines.Add($"- 注記: {overviewNote}");
        }

        if (rows.Count > 0)
        {
            lines.AddRange(new[]
            {
                string.Empty,
                "## 注目ポイント",
                $"- representative 1位帯の先頭表示: **{bestRow.Name}**",
                $"- 勝点: **{bestRow.Points.ToString(CultureInfo.InvariantCulture)}**",
                $"- 順位帯: **{bestRow.RankLabel}**"
            });
        }

        lines.AddRange(new[]
        {
            string.Empty,
            "## 一覧表",
            "| 対局者 | 勝点 | 順位帯 | 平均順位 | 1位確率 |",
            "| --- | ---: | ---: | ---: | ---: |"
        });

        lines.AddRange(rows.Select(row =>
            $"| {row.Name} | {row.Points.ToString(CultureInfo.InvariantCulture)} | {row.RankLabel} | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} | {(row.FirstPlaceProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% |"));

        return lines;
    }

    #endregion

    #region ［固定列一覧］

    /// <summary>
    /// ［固定列一覧］読込
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    static IReadOnlyList<string> LoadFixedColumns(string fileName)
    {
        return TableTypeDefinitionReader.Load(fileName)
            .Data
            .Select(column => column.Name)
            .ToArray();
    }

    #endregion


    // ========================================
    // TODO: ［標準ルール版］と［本戦ルール版］の統合を進めてほしいものや、その他
    // ========================================

    #region ［数値の言語化］

    protected static string BuildTop1Comment(double top1Probability)
    {
        var percent = top1Probability * 100;
        return percent switch
        {
            >= 30.0 => "かなり強いです。",
            >= 20.0 => "そこそこ確保されています。",
            >= 10.0 => "やや弱めです。",
            _ => "かなり弱めです。",
        };
    }

    protected static string BuildAveragePlaceComment(double averagePlace)
    {
        return averagePlace switch
        {
            <= 2.0 => "かなり前寄りです。",
            <= 3.5 => "比較的前寄りです。",
            <= 5.0 => "中位上側です。",
            _ => "まだ混戦気味です。",
        };
    }

    protected static string BuildRatingDeltaComment(double biggestBoost, double biggestDrop)
    {
        var spread = biggestBoost - biggestDrop;
        return spread switch
        {
            >= 80.0 => "割り当てや対戦構成の影響がかなり大きいです。",
            >= 40.0 => "割り当てや対戦構成の影響が見えてきます。",
            >= 15.0 => "割り当てや対戦構成の影響は比較的小さめです。",
            _ => "割り当てや対戦構成の影響はかなり小さめです。",
        };
    }

    protected static string BuildGroupLeadComment(double groupPlace1Probability, double groupPlaceAverage)
    {
        var percent = groupPlace1Probability * 100;
        if (percent >= 35.0 && groupPlaceAverage <= 2.0) return "先頭がかなりはっきりしています。";
        if (percent >= 20.0 && groupPlaceAverage <= 3.0) return "先頭候補が見えています。";
        return "まだ横並び気味です。";
    }

    protected static string BuildApexInnovGapComment(double apexTopProbability, double innovTopProbability)
    {
        var gapPercent = (apexTopProbability - innovTopProbability) * 100;
        return gapPercent switch
        {
            >= 15.0 => "Apex 側の先頭がかなり優勢です。",
            >= 5.0 => "Apex 側の先頭がやや優勢です。",
            > -5.0 => "両グループの先頭感は近めです。",
            _ => "Innov 側の先頭感もかなり強いです。",
        };
    }

    #endregion

}
