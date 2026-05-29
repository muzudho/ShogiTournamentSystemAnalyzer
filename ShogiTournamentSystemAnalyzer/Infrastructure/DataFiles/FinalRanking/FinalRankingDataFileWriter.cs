/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Application.DataDefinitions;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System.Globalization;

/// <summary>
/// ［最終順位］境界のデータファイル writer の共通処理。
/// </summary>
internal class FinalRankingDataFileWriter
{
    // ========================================
    // 生成
    // ========================================


    public FinalRankingDataFileWriter(FinalRankingDataFileWriterSettings settings)
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

    #region ［列名の一覧］

    /// <summary>
    /// ［列名の一覧］取得
    /// </summary>
    /// <returns></returns>
    protected IReadOnlyList<string> GetFinalRankingFixedColumns()
    {
        return finalRankingFixedColumns ??= LoadFixedColumns(this.Settings.GetFinalRankingTableTypeFileName());
    }
    static IReadOnlyList<string>? finalRankingFixedColumns;

    #endregion


    #region ［最終順位という境界］のCSV形式データ

    /// <summary>
    /// ［最終順位という境界］のCSV形式データを作成する。
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <param name="outputCsvPath">TODO: この引数、使ってるのかだぜ（＾～＾）？</param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="overviewNote"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal IEnumerable<string> CreateResultCsvCore<TRow>(
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        string? overviewNote = null)
        where TRow : ISimulationResultRow
    {
        _ = outputCsvPath;
        return BuildFinalRankingResultCsv(mode, firstPlayerWinRatePercent, resultRows, overviewNote);
    }

    /// <summary>
    /// ヘッダー行の各列
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <param name="fixedColumns"></param>
    /// <param name="resultRows"></param>
    /// <param name="overviewNote"></param>
    /// <returns></returns>
    static List<string> BuildFinalRankingSpecificHeaderColumns<TRow>(
        IReadOnlyList<string> fixedColumns,
        IReadOnlyList<TRow> resultRows,
        string? overviewNote)
        where TRow : ISimulationResultRow
    {
        var specificHeaderColumns = fixedColumns.ToList();
        if (string.IsNullOrWhiteSpace(overviewNote)) specificHeaderColumns.Remove("note");

        if (resultRows.Count > 0)
        {
            AppendPlaceHeaderColumns(specificHeaderColumns, resultRows[0]);
        }

        return specificHeaderColumns;
    }

    static void AppendPlaceHeaderColumns(List<string> specificHeaderColumns, ISimulationResultRow row)
    {
        for (var place = 0; place < row.PlaceProbabilities.Length; place++)
        {
            specificHeaderColumns.Add($"place{place + 1}Percent");
            if (row.PlaceCounts is not null)
            {
                specificHeaderColumns.Add($"place{place + 1}Count");
            }
        }
    }

    /// <summary>
    /// データ行の各列
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="row"></param>
    /// <param name="overviewNote"></param>
    /// <returns></returns>
    static List<string> BuildFinalRankingSpecificColumns(
        string mode,
        double firstPlayerWinRatePercent,
        ISimulationResultRow row,
        string? overviewNote)
    {
        var specificColumns = new List<string>
        {
            mode,
            firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)
        };

        specificColumns.AddRange(row.GetFinalRankingCsvSpecificColumns());

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            specificColumns.Add(overviewNote);
        }

        AppendPlaceColumns(specificColumns, row);
        return specificColumns;
    }

    static void AppendPlaceColumns(List<string> specificColumns, ISimulationResultRow row)
    {
        for (var place = 0; place < row.PlaceProbabilities.Length; place++)
        {
            specificColumns.Add((row.PlaceProbabilities[place] * 100).ToString("F2", CultureInfo.InvariantCulture));
            if (row.PlaceCounts is not null)
            {
                specificColumns.Add(row.PlaceCounts[place].ToString("F3", CultureInfo.InvariantCulture));
            }
        }
    }

    IEnumerable<string> BuildFinalRankingResultCsv<TRow>(
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        string? overviewNote = null)
        where TRow : ISimulationResultRow
    {
        var specificHeaderColumns = BuildFinalRankingSpecificHeaderColumns(GetFinalRankingFixedColumns(), resultRows, overviewNote);
        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(specificHeaderColumns).Select(CsvOutputHelpers.EscapeCsv))
        };

        foreach (var row in resultRows)
        {
            var specificColumns = BuildFinalRankingSpecificColumns(mode, firstPlayerWinRatePercent, row, overviewNote);
            var columns = CsvSchemaCommonColumns.BuildRowColumns(
                boundaryName: "FinalRanking",
                schemaName: this.Settings.GetSchemaName(),
                rowType: "data",
                specificColumns.ToArray());

            lines.Add(string.Join(",", columns.Select(CsvOutputHelpers.EscapeCsv)));
        }

        return lines;
    }

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
        // TODO: ここの分岐も、将来的には、外部の DSL ファイルに出したいぜ（＾～＾）
        return resultRows switch
        {
            IReadOnlyList<ResultRow> standardRows => CreateFinalRankingMarkdownReport(
                outputMarkdownPath: outputMarkdownPath,
                outputCsvPath: outputCsvPath,
                mode: mode,
                firstPlayerWinRatePercent: firstPlayerWinRatePercent,
                playerCount: standardRows.Count,
                editionLabel: "標準版",
                primarySections: BuildStandardPrimarySections(standardRows),
                primaryTableRows: BuildStandardPrimaryTableRows(standardRows),
                trailingSections: [],
                charts: BuildStandardCharts(standardRows),
                overviewNote: overviewNote,
                representativeRankingMarkdownPath: representativeRankingMarkdownPath,
                referenceMatchesCsvPath: referenceMatchesCsvPath),
            IReadOnlyList<FinalStageResultRow> finalStageRows => CreateFinalRankingMarkdownReport(
                outputMarkdownPath: outputMarkdownPath,
                outputCsvPath: outputCsvPath,
                mode: mode,
                firstPlayerWinRatePercent: firstPlayerWinRatePercent,
                playerCount: finalStageRows.Count,
                editionLabel: "本戦版",
                primarySections: BuildFinalStagePrimarySections(finalStageRows),
                primaryTableRows: BuildFinalStagePrimaryTableRows(finalStageRows),
                trailingSections: BuildFinalStageTrailingSections(finalStageRows),
                charts: BuildFinalStageCharts(finalStageRows),
                overviewNote: overviewNote,
                representativeRankingMarkdownPath: representativeRankingMarkdownPath,
                referenceMatchesCsvPath: referenceMatchesCsvPath),
            _ => throw new InvalidOperationException($"未対応の結果行型: {typeof(TRow).FullName}")
        };
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

    static IEnumerable<string>[] BuildStandardPrimarySections(IReadOnlyList<ResultRow> resultRows)
    {
        var topChampionshipRows = SelectTopRows(resultRows, row => row.ChampionshipProbability, row => row.AveragePlace, takeCount: 8);
        var hasBestChampionshipRow = TrySelectBestRow(resultRows, row => row.ChampionshipProbability, row => row.AveragePlace, out var bestChampionshipRow);
        var hasBestAveragePlaceRow = TrySelectBestRow(resultRows, row => -row.AveragePlace, row => -row.ChampionshipProbability, out var bestAveragePlaceRow);
        var hasBiggestBoostRow = TrySelectBestRow(resultRows, row => row.RatingDelta, row => 0, out var biggestBoostRow);
        var hasBiggestDropRow = TrySelectBestRow(resultRows, row => -row.RatingDelta, row => 0, out var biggestDropRow);
        var bestChampionshipRowName = hasBestChampionshipRow ? bestChampionshipRow.Name : "該当なし";
        var bestAveragePlaceRowName = hasBestAveragePlaceRow ? bestAveragePlaceRow.Name : "該当なし";
        var biggestBoostRowName = hasBiggestBoostRow ? biggestBoostRow.Name : "該当なし";
        var biggestDropRowName = hasBiggestDropRow ? biggestDropRow.Name : "該当なし";
        var bestChampionshipProbability = hasBestChampionshipRow ? bestChampionshipRow.ChampionshipProbability : 0;
        var bestAveragePlace = hasBestAveragePlaceRow ? bestAveragePlaceRow.AveragePlace : 0;
        var biggestBoost = hasBiggestBoostRow ? biggestBoostRow.RatingDelta : 0;
        var biggestDrop = hasBiggestDropRow ? biggestDropRow.RatingDelta : 0;

        return
        [
            [
                "## 注目ポイント",
                $"- 優勝確率が最も高い選手: **{bestChampionshipRowName}**（{(bestChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"- 平均順位が最も良い選手: **{bestAveragePlaceRowName}**（{bestAveragePlace.ToString("F3", CultureInfo.InvariantCulture)}）",
                $"- 実効Elo差分が最も大きくプラスの選手: **{biggestBoostRowName}**（{SimulationRatingMath.FormatSignedRating(biggestBoost)}）",
                $"- 実効Elo差分が最も大きくマイナスの選手: **{biggestDropRowName}**（{SimulationRatingMath.FormatSignedRating(biggestDrop)}）"
            ],
            [
                "## 自動コメント",
                $"- 優勝候補の強さ: {BuildTop1Comment(bestChampionshipProbability)}",
                $"- 先頭の平均順位: {BuildAveragePlaceComment(bestAveragePlace)}",
                $"- 実効Eloの押し上げ: {BuildRatingDeltaComment(biggestBoost, biggestDrop)}"
            ],
            [
                "## 上位候補一覧",
                "| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |",
                "| --- | ---: | ---: | ---: | ---: | ---: |"
            ]
        ];
    }

    static IEnumerable<string> BuildStandardPrimaryTableRows(IReadOnlyList<ResultRow> resultRows)
    {
        var topChampionshipRows = SelectTopRows(resultRows, row => row.ChampionshipProbability, row => row.AveragePlace, takeCount: 8);
        return BuildMarkdownTableRows(topChampionshipRows, row =>
            $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} |");
    }

    static FinalRankingMarkdownChartSpec[] BuildStandardCharts(IReadOnlyList<ResultRow> resultRows)
    {
        var topChampionshipRows = SelectTopRows(resultRows, row => row.ChampionshipProbability, row => row.AveragePlace, takeCount: 8);
        if (topChampionshipRows.Length == 0) return [];

        return
        [
            new(
                "上位候補の優勝確率",
                topChampionshipRows.Select(row => row.Name),
                "優勝確率(%)",
                "0 --> 100",
                topChampionshipRows.Select(row => (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture))),
            new(
                "上位候補の平均順位",
                topChampionshipRows.Select(row => row.Name),
                "平均順位",
                "1 --> " + Math.Max(2, resultRows.Count).ToString(CultureInfo.InvariantCulture),
                topChampionshipRows.Select(row => row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)))
        ];
    }

    static IEnumerable<string>[] BuildFinalStagePrimarySections(IReadOnlyList<FinalStageResultRow> resultRows)
    {
        var apexRows = SelectTopRowsByGroup(resultRows, row => string.Equals(row.Group, "Apex", StringComparison.OrdinalIgnoreCase), row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, takeCount: 4);
        var innovRows = SelectTopRowsByGroup(resultRows, row => string.Equals(row.Group, "Innov", StringComparison.OrdinalIgnoreCase), row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, takeCount: 4);
        var hasBestOverallRow = TrySelectBestRow(resultRows, row => row.OverallPlace1Probability, row => row.OverallPlaceAverage, out var bestOverallRow);
        var hasBestApexRow = TrySelectBestRow(apexRows, row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, out var bestApexRow);
        var hasBestInnovRow = TrySelectBestRow(innovRows, row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, out var bestInnovRow);
        var bestOverallRowName = hasBestOverallRow ? bestOverallRow.Name : "該当なし";
        var bestApexRowName = hasBestApexRow ? bestApexRow.Name : "該当なし";
        var bestInnovRowName = hasBestInnovRow ? bestInnovRow.Name : "該当なし";
        var bestOverallProbability = hasBestOverallRow ? bestOverallRow.OverallPlace1Probability : 0;
        var bestApexProbability = hasBestApexRow ? bestApexRow.GroupPlace1Probability : 0;
        var bestInnovProbability = hasBestInnovRow ? bestInnovRow.GroupPlace1Probability : 0;
        var bestApexAverage = hasBestApexRow ? bestApexRow.GroupPlaceAverage : 0;
        var bestInnovAverage = hasBestInnovRow ? bestInnovRow.GroupPlaceAverage : 0;

        return
        [
            [
                "## 注目ポイント",
                $"- 総合1位確率が最も高い選手: **{bestOverallRowName}**（{(bestOverallProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"- Apex で最も有力な選手: **{bestApexRowName}**（グループ1位確率 {(bestApexProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"- Innov で最も有力な選手: **{bestInnovRowName}**（グループ1位確率 {(bestInnovProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）"
            ],
            [
                "## 自動コメント",
                $"- 総合1位候補の強さ: {BuildTop1Comment(bestOverallProbability)}",
                $"- Apex の先頭感: {BuildGroupLeadComment(bestApexProbability, bestApexAverage)}",
                $"- Innov の先頭感: {BuildGroupLeadComment(bestInnovProbability, bestInnovAverage)}",
                $"- Apex / Innov の先頭差: {BuildApexInnovGapComment(bestApexProbability, bestInnovProbability)}"
            ],
            [
                "## 上位候補一覧",
                "| 選手 | グループ | 元Elo | 実効Elo | 差分 | グループ1位確率 | 総合1位確率 | 総合平均順位 |",
                "| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |"
            ]
        ];
    }

    static IEnumerable<string> BuildFinalStagePrimaryTableRows(IReadOnlyList<FinalStageResultRow> resultRows)
    {
        var topRows = SelectTopRows(resultRows, row => row.OverallPlace1Probability, row => row.OverallPlaceAverage, takeCount: 8);
        return BuildMarkdownTableRows(topRows, row =>
            $"| {row.Name} | {row.Group} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |");
    }

    static IEnumerable<string>[] BuildFinalStageTrailingSections(IReadOnlyList<FinalStageResultRow> resultRows)
    {
        var sections = new List<IEnumerable<string>>();
        var apexRows = SelectTopRowsByGroup(resultRows, row => string.Equals(row.Group, "Apex", StringComparison.OrdinalIgnoreCase), row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, takeCount: 4);
        var innovRows = SelectTopRowsByGroup(resultRows, row => string.Equals(row.Group, "Innov", StringComparison.OrdinalIgnoreCase), row => row.GroupPlace1Probability, row => row.GroupPlaceAverage, takeCount: 4);

        if (apexRows.Length > 0)
        {
            sections.Add(
            [
                "## Apex 注目候補",
                "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |",
                "| --- | ---: | ---: | ---: | ---: | ---: |"
            ]);
            sections.Add(BuildMarkdownTableRows(apexRows, row =>
                $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
        }

        if (innovRows.Length > 0)
        {
            sections.Add(
            [
                "## Innov 注目候補",
                "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |",
                "| --- | ---: | ---: | ---: | ---: | ---: |"
            ]);
            sections.Add(BuildMarkdownTableRows(innovRows, row =>
                $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
        }

        return sections.ToArray();
    }

    static FinalRankingMarkdownChartSpec[] BuildFinalStageCharts(IReadOnlyList<FinalStageResultRow> resultRows)
    {
        var topRows = SelectTopRows(resultRows, row => row.OverallPlace1Probability, row => row.OverallPlaceAverage, takeCount: 8);
        if (topRows.Length == 0) return [];

        return
        [
            new(
                "上位候補の総合1位確率",
                topRows.Select(row => row.Name),
                "総合1位確率(%)",
                "0 --> 100",
                topRows.Select(row => (row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))),
            new(
                "上位候補のグループ1位確率",
                topRows.Select(row => row.Name),
                "グループ1位確率(%)",
                "0 --> 100",
                topRows.Select(row => (row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)))
        ];
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

    protected static IReadOnlyList<string> GetRepresentativeExecutionRankFixedColumns()
    {
        return representativeExecutionRankFixedColumns ??= LoadFixedColumns(RepresentativeExecutionRankTableTypeFileName);
    }

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
