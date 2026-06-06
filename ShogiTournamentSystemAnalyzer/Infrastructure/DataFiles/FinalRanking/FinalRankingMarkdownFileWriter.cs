/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries.FinalRanking.DataDefinitions;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
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
        return SplitMarkdownLines(RenderMarkdownTemplate(BuildGeneralMarkdownTemplateModel(
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

    static GeneralSimulationResultRow[] SelectTopRows(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, double> primaryDescending,
        Func<GeneralSimulationResultRow, double> secondaryAscending,
        int takeCount,
        Func<GeneralSimulationResultRow, string> nameSelector)
    {
        return rows
            .OrderByDescending(primaryDescending)
            .ThenBy(secondaryAscending)
            .ThenBy(nameSelector, StringComparer.OrdinalIgnoreCase)
            .Take(takeCount)
            .ToArray();
    }

    static bool TrySelectBestRow(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, double> primaryDescending,
        Func<GeneralSimulationResultRow, double> secondaryAscending,
        Func<GeneralSimulationResultRow, string> nameSelector,
        out GeneralSimulationResultRow bestRow)
    {
        using var enumerator = rows
            .OrderByDescending(primaryDescending)
            .ThenBy(secondaryAscending)
            .ThenBy(nameSelector, StringComparer.OrdinalIgnoreCase)
            .GetEnumerator();

        if (!enumerator.MoveNext())
        {
            bestRow = default!;
            return false;
        }

        bestRow = enumerator.Current;
        return true;
    }

    static GeneralSimulationResultRow[] SelectTopRowsByGroup(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, bool> predicate,
        Func<GeneralSimulationResultRow, double> primaryDescending,
        Func<GeneralSimulationResultRow, double> secondaryAscending,
        int takeCount,
        Func<GeneralSimulationResultRow, string> nameSelector)
    {
        return SelectTopRows(rows.Where(predicate), primaryDescending, secondaryAscending, takeCount, nameSelector);
    }

    static bool HasMetrics(IReadOnlyList<GeneralSimulationResultRow> rows, params string[] metricKeys)
    {
        return rows.Count > 0 && rows.All(row => metricKeys.All(row.Metrics.ContainsKey));
    }

    static double GetMetric(GeneralSimulationResultRow row, string key)
    {
        if (row.Metrics.TryGetValue(key, out var metric))
        {
            return metric.Value;
        }

        throw new InvalidOperationException($"シミュレーション結果行に必要な metric がありません: {key}");
    }

    static string GetFreeColumn(GeneralSimulationResultRow row, string key)
    {
        var column = row.FreeColumns.FirstOrDefault(column => string.Equals(column.Key, key, StringComparison.Ordinal));
        if (!string.IsNullOrEmpty(column.Key))
        {
            return column.DisplayValue;
        }

        throw new InvalidOperationException($"シミュレーション結果行に必要な自由形式列がありません: {key}");
    }
    static IEnumerable<string> BuildMarkdownTableRows(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, string> formatter)
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

    static MarkdownTemplateModel BuildGeneralMarkdownTemplateModel(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        if (HasMetrics(resultRows, "championshipProbability", "averagePlace"))
        {
            return BuildChampionshipMarkdownTemplateModel(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                resultRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath);
        }

        if (HasMetrics(resultRows, "groupPlace1Probability", "groupPlaceAverage", "overallPlace1Probability", "overallPlaceAverage"))
        {
            return BuildGroupedOverallMarkdownTemplateModel(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                resultRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath);
        }

        throw new InvalidOperationException("Markdown出力に必要な metric の組み合わせが見つかりません。");
    }

    static MarkdownTemplateModel BuildChampionshipMarkdownTemplateModel(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        var topChampionshipRows = SelectTopRows(resultRows, row => GetMetric(row, "championshipProbability"), row => GetMetric(row, "averagePlace"), takeCount: 8, row => row.CommonData.Name);
        var hasBestChampionshipRow = TrySelectBestRow(resultRows, row => GetMetric(row, "championshipProbability"), row => GetMetric(row, "averagePlace"), row => row.CommonData.Name, out var bestChampionshipRow);
        var hasBestAveragePlaceRow = TrySelectBestRow(resultRows, row => -GetMetric(row, "averagePlace"), row => -GetMetric(row, "championshipProbability"), row => row.CommonData.Name, out var bestAveragePlaceRow);
        var hasBiggestBoostRow = TrySelectBestRow(resultRows, row => row.CommonData.RatingDelta, row => 0, row => row.CommonData.Name, out var biggestBoostRow);
        var hasBiggestDropRow = TrySelectBestRow(resultRows, row => -row.CommonData.RatingDelta, row => 0, row => row.CommonData.Name, out var biggestDropRow);

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
                $"優勝確率が最も高い選手: **{(hasBestChampionshipRow ? bestChampionshipRow.CommonData.Name : "該当なし")}**（{((hasBestChampionshipRow ? GetMetric(bestChampionshipRow, "championshipProbability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"平均順位が最も良い選手: **{(hasBestAveragePlaceRow ? bestAveragePlaceRow.CommonData.Name : "該当なし")}**（{(hasBestAveragePlaceRow ? GetMetric(bestAveragePlaceRow, "averagePlace") : 0).ToString("F3", CultureInfo.InvariantCulture)}）",
                $"実効Elo差分が最も大きくプラスの選手: **{(hasBiggestBoostRow ? biggestBoostRow.CommonData.Name : "該当なし")}**（{SimulationRatingMath.FormatSignedRating(hasBiggestBoostRow ? biggestBoostRow.CommonData.RatingDelta : 0)}）",
                $"実効Elo差分が最も大きくマイナスの選手: **{(hasBiggestDropRow ? biggestDropRow.CommonData.Name : "該当なし")}**（{SimulationRatingMath.FormatSignedRating(hasBiggestDropRow ? biggestDropRow.CommonData.RatingDelta : 0)}）"
            ],
            AutoComments =
            [
                $"優勝候補の強さ: {BuildTop1Comment(hasBestChampionshipRow ? GetMetric(bestChampionshipRow, "championshipProbability") : 0)}",
                $"先頭の平均順位: {BuildAveragePlaceComment(hasBestAveragePlaceRow ? GetMetric(bestAveragePlaceRow, "averagePlace") : 0)}",
                $"実効Eloの押し上げ: {BuildRatingDeltaComment(hasBiggestBoostRow ? biggestBoostRow.CommonData.RatingDelta : 0, hasBiggestDropRow ? biggestDropRow.CommonData.RatingDelta : 0)}"
            ],
            PrimaryTableHeader = "| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |",
            PrimaryTableHeaderSeparator = "| --- | ---: | ---: | ---: | ---: | ---: |",
            PrimaryTableRows = BuildMarkdownTableRows(topChampionshipRows, row =>
                $"| {row.CommonData.Name} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.CommonData.RatingDelta)} | {(GetMetric(row, "championshipProbability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "averagePlace").ToString("F3", CultureInfo.InvariantCulture)} |"),
            TrailingSections = [],
            Charts = BuildChartSpecs(topChampionshipRows,
                row => row.CommonData.Name,
                ("上位候補の優勝確率", "優勝確率(%)", "0 --> 100", row => (GetMetric(row, "championshipProbability") * 100).ToString("F2", CultureInfo.InvariantCulture)),
                ("上位候補の平均順位", "平均順位", "1 --> " + Math.Max(2, resultRows.Count).ToString(CultureInfo.InvariantCulture), row => GetMetric(row, "averagePlace").ToString("F3", CultureInfo.InvariantCulture)))
        };
    }

    static MarkdownTemplateModel BuildGroupedOverallMarkdownTemplateModel(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        var apexRows = SelectTopRowsByGroup(resultRows, row => string.Equals(GetFreeColumn(row, "group"), "Apex", StringComparison.OrdinalIgnoreCase), row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), takeCount: 4, row => row.CommonData.Name);
        var innovRows = SelectTopRowsByGroup(resultRows, row => string.Equals(GetFreeColumn(row, "group"), "Innov", StringComparison.OrdinalIgnoreCase), row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), takeCount: 4, row => row.CommonData.Name);
        var hasBestOverallRow = TrySelectBestRow(resultRows, row => GetMetric(row, "overallPlace1Probability"), row => GetMetric(row, "overallPlaceAverage"), row => row.CommonData.Name, out var bestOverallRow);
        var hasBestApexRow = TrySelectBestRow(apexRows, row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), row => row.CommonData.Name, out var bestApexRow);
        var hasBestInnovRow = TrySelectBestRow(innovRows, row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), row => row.CommonData.Name, out var bestInnovRow);

        var primaryRows = SelectTopRows(resultRows, row => GetMetric(row, "overallPlace1Probability"), row => GetMetric(row, "overallPlaceAverage"), takeCount: 8, row => row.CommonData.Name);

        var trailingSections = new List<MarkdownTemplateSection>();
        if (apexRows.Length > 0)
        {
            trailingSections.Add(new MarkdownTemplateSection(
                Title: "## Apex 注目候補",
                TableHeader: "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |\n| --- | ---: | ---: | ---: | ---: | ---: |",
                Rows: BuildMarkdownTableRows(apexRows, row =>
                    $"| {row.CommonData.Name} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {(GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "groupPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} | {GetMetric(row, "overallPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} |")));
        }

        if (innovRows.Length > 0)
        {
            trailingSections.Add(new MarkdownTemplateSection(
                Title: "## Innov 注目候補",
                TableHeader: "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |\n| --- | ---: | ---: | ---: | ---: | ---: |",
                Rows: BuildMarkdownTableRows(innovRows, row =>
                    $"| {row.CommonData.Name} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {(GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "groupPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} | {GetMetric(row, "overallPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} |")));
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
                $"総合1位確率が最も高い選手: **{(hasBestOverallRow ? bestOverallRow.CommonData.Name : "該当なし")}**（{((hasBestOverallRow ? GetMetric(bestOverallRow, "overallPlace1Probability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"Apex で最も有力な選手: **{(hasBestApexRow ? bestApexRow.CommonData.Name : "該当なし")}**（グループ1位確率 {((hasBestApexRow ? GetMetric(bestApexRow, "groupPlace1Probability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"Innov で最も有力な選手: **{(hasBestInnovRow ? bestInnovRow.CommonData.Name : "該当なし")}**（グループ1位確率 {((hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlace1Probability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）"
            ],
            AutoComments =
            [
                $"総合1位候補の強さ: {BuildTop1Comment(hasBestOverallRow ? GetMetric(bestOverallRow, "overallPlace1Probability") : 0)}",
                $"Apex の先頭感: {BuildGroupLeadComment(hasBestApexRow ? GetMetric(bestApexRow, "groupPlace1Probability") : 0, hasBestApexRow ? GetMetric(bestApexRow, "groupPlaceAverage") : 0)}",
                $"Innov の先頭感: {BuildGroupLeadComment(hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlace1Probability") : 0, hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlaceAverage") : 0)}",
                $"Apex / Innov の先頭差: {BuildApexInnovGapComment(hasBestApexRow ? GetMetric(bestApexRow, "groupPlace1Probability") : 0, hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlace1Probability") : 0)}"
            ],
            PrimaryTableHeader = "| 選手 | グループ | 元Elo | 実効Elo | 差分 | グループ1位確率 | 総合1位確率 | 総合平均順位 |",
            PrimaryTableHeaderSeparator = "| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |",
            PrimaryTableRows = BuildMarkdownTableRows(primaryRows, row =>
                $"| {row.CommonData.Name} | {GetFreeColumn(row, "group")} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.CommonData.RatingDelta)} | {(GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(GetMetric(row, "overallPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "overallPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} |"),
            TrailingSections = trailingSections.ToArray(),
            Charts = BuildChartSpecs(primaryRows,
                row => row.CommonData.Name,
                ("上位候補の総合1位確率", "総合1位確率(%)", "0 --> 100", row => (GetMetric(row, "overallPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)),
                ("上位候補のグループ1位確率", "グループ1位確率(%)", "0 --> 100", row => (GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)))
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

    readonly record struct FinalRankingMarkdownChartSpec(
        string Title,
        IEnumerable<string> Categories,
        string YAxisLabel,
        string YAxisRange,
        IEnumerable<string> Values);

    static FinalRankingMarkdownChartSpec[] BuildChartSpecs(
        IReadOnlyList<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, string> categorySelector,
        params (string Title, string YAxisLabel, string YAxisRange, Func<GeneralSimulationResultRow, string> ValueSelector)[] chartDefinitions)
    {
        if (rows.Count == 0) return [];

        return chartDefinitions
            .Select(chart => new FinalRankingMarkdownChartSpec(
                chart.Title,
                rows.Select(categorySelector),
                chart.YAxisLabel,
                chart.YAxisRange,
                rows.Select(chart.ValueSelector)))
            .ToArray();
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
