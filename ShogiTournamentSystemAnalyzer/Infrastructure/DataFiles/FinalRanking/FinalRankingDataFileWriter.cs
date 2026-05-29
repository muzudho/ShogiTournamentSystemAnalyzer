/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Application.DataDefinitions;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
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


    public FinalRankingDataFileWriter(RuleProfileMode ruleProfileMode)
    {
        this.RuleProfileMode = ruleProfileMode;
    }


    // ========================================
    // 窓口データメンバー
    // ========================================


    #region ［大会ルール］の種類（ TODO: ここをDSLに外出ししたい ）

    /// <summary>
    /// ［大会ルール］の種類
    /// </summary>
    internal RuleProfileMode RuleProfileMode { get; init; }

    internal string GetFinalRankingTableTypeFileName()
    {
        switch (this.RuleProfileMode)
        {
            case RuleProfileMode.Standard:
                return "FinalRankingStandardTableType.json";

            case RuleProfileMode.FinalStage:
                return "FinalRankingFinalStageTableType.json";

            default:
                throw(new InvalidOperationException($"未対応のルールプロファイルモード: {this.RuleProfileMode}"));
        }
    }

    internal string GetSchemaName()
    {
        switch (this.RuleProfileMode)
        {
            case RuleProfileMode.Standard:
                return "standardFinalRanking";

            case RuleProfileMode.FinalStage:
                return "finalStageFinalRanking";

            //case RuleProfileMode.TournamentFramework:
            //    return "";

            default:
                throw (new InvalidOperationException($"未対応のルールプロファイルモード: {this.RuleProfileMode}"));
        }
    }

    #endregion

    #region ［列名の一覧］

    /// <summary>
    /// ［列名の一覧］取得
    /// </summary>
    /// <returns></returns>
    protected IReadOnlyList<string> GetFinalRankingFixedColumns()
    {
        return finalRankingFixedColumns ??= LoadFixedColumns(this.GetFinalRankingTableTypeFileName());
    }
    static IReadOnlyList<string>? finalRankingFixedColumns;

    #endregion


    // ========================================
    // 窓口メソッド
    // ========================================


    /// <summary>
    /// ［最終順位という境界］のCSV形式データを作成する。
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <param name="outputCsvPath"></param>
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
        return resultRows switch
        {
            IReadOnlyList<ResultRow> standardRows => CreateStandardResultCsvCore(
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                standardRows,
                overviewNote),
            IReadOnlyList<FinalStageResultRow> finalStageRows => CreateFinalStageResultCsvCore(
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                finalStageRows,
                overviewNote),
            _ => throw new InvalidOperationException($"未対応の結果行型: {typeof(TRow).FullName}")
        };
    }

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
            IReadOnlyList<ResultRow> standardRows => CreateStandardResultMarkdownCore(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                standardRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath),
            IReadOnlyList<FinalStageResultRow> finalStageRows => CreateFinalStageResultMarkdownCore(
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


    // ========================================
    // その他
    // ========================================


    const string RepresentativeExecutionRankTableTypeFileName = "RepresentativeExecutionRankTableType.json";

    static IReadOnlyList<string>? representativeExecutionRankFixedColumns;


    protected static IReadOnlyList<string> GetRepresentativeExecutionRankFixedColumns()
    {
        return representativeExecutionRankFixedColumns ??= LoadFixedColumns(RepresentativeExecutionRankTableTypeFileName);
    }

    static IReadOnlyList<string> LoadFixedColumns(string fileName)
    {
        return TableTypeDefinitionReader.Load(fileName)
            .Data
            .Select(column => column.Name)
            .ToArray();
    }

    /// <summary>
    /// ［本戦用］
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
                schemaName: GetSchemaName(),
                rowType: "data",
                specificColumns.ToArray());

            lines.Add(string.Join(",", columns.Select(CsvOutputHelpers.EscapeCsv)));
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

    #region ［結果CSV］

    /// <summary>
    /// これは［標準版］。
    /// 
    /// TODO: おー、メソッドのシグニチャが揃ってきたな（＾▽＾） `CreateFinalStageResultCsvCore` メソッドと統合できる感じかだぜ（＾▽＾）？
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="overviewNote">TODO: これ［本戦］に無いの（＾～＾）？</param>
    /// <returns></returns>
    internal IEnumerable<string> CreateStandardResultCsvCore(
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<ResultRow> resultRows,
        string? overviewNote = null)
    {
        _ = outputCsvPath;
        return BuildFinalRankingResultCsv(mode, firstPlayerWinRatePercent, resultRows, overviewNote);
    }

    /// <summary>
    /// これは［本戦版］。
    /// 
    /// TODO: おー、メソッドのシグニチャが揃ってきたな（＾▽＾） `CreateResultCsvCore` メソッドと統合できる感じかだぜ（＾▽＾）？
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <returns></returns>
    internal IEnumerable<string> CreateFinalStageResultCsvCore(
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<FinalStageResultRow> resultRows,  // TODO: `ResultRow` と `FinalStageResultRow` を一本にできないの（＾～＾）？
        string? overviewNote = null)
    {
        _ = outputCsvPath;
        return BuildFinalRankingResultCsv(mode, firstPlayerWinRatePercent, resultRows, overviewNote);
    }

    #endregion

    #region ［結果マークダウン］

    /// <summary>
    /// これは［標準版］。
    /// 
    /// TODO: おー、メソッドのシグニチャが揃ってきたな（＾▽＾） `CreateFinalStageResultMarkdownCore` メソッドと統合できる感じかだぜ（＾▽＾）？
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
    internal IEnumerable<string> CreateStandardResultMarkdownCore(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<ResultRow> resultRows,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        _ = referenceMatchesCsvPath;

        var topChampionshipRows = resultRows
            .OrderByDescending(row => row.ChampionshipProbability)
            .ThenBy(row => row.AveragePlace)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();
        var bestChampionshipRow = resultRows
            .OrderByDescending(row => row.ChampionshipProbability)
            .ThenBy(row => row.AveragePlace)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        var bestAveragePlaceRow = resultRows
            .OrderBy(row => row.AveragePlace)
            .ThenByDescending(row => row.ChampionshipProbability)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        var biggestBoostRow = resultRows
            .OrderByDescending(row => row.RatingDelta)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        var biggestDropRow = resultRows
            .OrderBy(row => row.RatingDelta)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        var bestChampionshipRowName = resultRows.Count > 0 ? bestChampionshipRow.Name : "該当なし";
        var bestAveragePlaceRowName = resultRows.Count > 0 ? bestAveragePlaceRow.Name : "該当なし";
        var biggestBoostRowName = resultRows.Count > 0 ? biggestBoostRow.Name : "該当なし";
        var biggestDropRowName = resultRows.Count > 0 ? biggestDropRow.Name : "該当なし";
        var bestChampionshipProbability = resultRows.Count > 0 ? bestChampionshipRow.ChampionshipProbability : 0;
        var bestAveragePlace = resultRows.Count > 0 ? bestAveragePlaceRow.AveragePlace : 0;
        var biggestBoost = resultRows.Count > 0 ? biggestBoostRow.RatingDelta : 0;
        var biggestDrop = resultRows.Count > 0 ? biggestDropRow.RatingDelta : 0;

        var lines = CreateFinalRankingMarkdownLines(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            playerCount: resultRows.Count,
            editionLabel: "標準版",
            overviewNote: overviewNote,
            representativeRankingMarkdownPath: representativeRankingMarkdownPath);

        AddMarkdownSection(lines,
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
        ]);

        lines.AddRange(topChampionshipRows.Select(row =>
            $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} |"));

        if (topChampionshipRows.Length > 0)
        {
            AddMarkdownSection(lines,
            ["## Mermaid 図"],
            BuildMermaidXychartLines(
                title: "上位候補の優勝確率",
                categories: topChampionshipRows.Select(row => row.Name),
                yAxisLabel: "優勝確率(%)",
                yAxisRange: "0 --> 100",
                values: topChampionshipRows.Select(row => (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture))),
            BuildMermaidXychartLines(
                title: "上位候補の平均順位",
                categories: topChampionshipRows.Select(row => row.Name),
                yAxisLabel: "平均順位",
                yAxisRange: "1 --> " + Math.Max(2, resultRows.Count).ToString(CultureInfo.InvariantCulture),
                values: topChampionshipRows.Select(row => row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture))));
        }

        return lines;
    }

    /// <summary>
    /// これは［本戦版］。
    /// 
    /// TODO: おー、メソッドのシグニチャが揃ってきたな（＾▽＾） `CreateResultMarkdownCore` メソッドと統合できる感じかだぜ（＾▽＾）？
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="referenceMatchesCsvPath">参考対局CSVファイルへのパス</param>
    /// <returns></returns>
    internal IEnumerable<string> CreateFinalStageResultMarkdownCore(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<FinalStageResultRow> resultRows,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null,
        string? referenceMatchesCsvPath = null)
    {
        var topRows = resultRows
            .OrderByDescending(row => row.OverallPlace1Probability)
            .ThenBy(row => row.OverallPlaceAverage)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();
        var apexRows = resultRows
            .Where(row => string.Equals(row.Group, "Apex", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.GroupPlace1Probability)
            .ThenBy(row => row.GroupPlaceAverage)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();
        var innovRows = resultRows
            .Where(row => string.Equals(row.Group, "Innov", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.GroupPlace1Probability)
            .ThenBy(row => row.GroupPlaceAverage)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();
        var bestOverallRow = resultRows
            .OrderByDescending(row => row.OverallPlace1Probability)
            .ThenBy(row => row.OverallPlaceAverage)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        var bestApexRow = apexRows.FirstOrDefault();
        var bestInnovRow = innovRows.FirstOrDefault();
        var bestOverallRowName = resultRows.Count > 0 ? bestOverallRow.Name : "該当なし";
        var bestApexRowName = apexRows.Length > 0 ? bestApexRow.Name : "該当なし";
        var bestInnovRowName = innovRows.Length > 0 ? bestInnovRow.Name : "該当なし";
        var bestOverallProbability = resultRows.Count > 0 ? bestOverallRow.OverallPlace1Probability : 0;
        var bestApexProbability = apexRows.Length > 0 ? bestApexRow.GroupPlace1Probability : 0;
        var bestInnovProbability = innovRows.Length > 0 ? bestInnovRow.GroupPlace1Probability : 0;
        var bestApexAverage = apexRows.Length > 0 ? bestApexRow.GroupPlaceAverage : 0;
        var bestInnovAverage = innovRows.Length > 0 ? bestInnovRow.GroupPlaceAverage : 0;

        var lines = CreateFinalRankingMarkdownLines(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            playerCount: resultRows.Count,
            editionLabel: "本戦版",
            overviewNote: overviewNote,
            representativeRankingMarkdownPath: representativeRankingMarkdownPath,
            referenceMatchesCsvPath: referenceMatchesCsvPath);

        AddMarkdownSection(lines,
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
        ]);

        lines.AddRange(topRows.Select(row =>
            $"| {row.Name} | {row.Group} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));

        if (apexRows.Length > 0)
        {
            AddMarkdownSection(lines,
            [
                "## Apex 注目候補",
                "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |",
                "| --- | ---: | ---: | ---: | ---: | ---: |"
            ]);

            lines.AddRange(apexRows.Select(row =>
                $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
        }

        if (innovRows.Length > 0)
        {
            AddMarkdownSection(lines,
            [
                "## Innov 注目候補",
                "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |",
                "| --- | ---: | ---: | ---: | ---: | ---: |"
            ]);

            lines.AddRange(innovRows.Select(row =>
                $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
        }

        if (topRows.Length > 0)
        {
            AddMarkdownSection(lines,
            ["## Mermaid 図"],
            BuildMermaidXychartLines(
                title: "上位候補の総合1位確率",
                categories: topRows.Select(row => row.Name),
                yAxisLabel: "総合1位確率(%)",
                yAxisRange: "0 --> 100",
                values: topRows.Select(row => (row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))),
            BuildMermaidXychartLines(
                title: "上位候補のグループ1位確率",
                categories: topRows.Select(row => row.Name),
                yAxisLabel: "グループ1位確率(%)",
                yAxisRange: "0 --> 100",
                values: topRows.Select(row => (row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))));
        }

        return lines;
    }

    #endregion

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
}
