using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;
using System.Globalization;
using System.Text;

internal static partial class Program
{
    /// <summary>
    /// ［品質評価］サマリーCSVを出力する
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="summary"></param>
    /// <param name="options"></param>
    static void WriteQualitySummaryCsv(string outputCsvPath, QualitySummary summary, ExperimentalReportGroupingOptions options)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () =>
            {
                // CSVの内容を作成する
                var lines = new List<string>
                {
                    "metricName,metricValue,note",
                    $"spearmanCorrelation,{summary.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture)},Elo順位と期待総合順位の相関",
                    $"meanAbsoluteRankError,{summary.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture)},期待総合順位とElo順位のずれの絶対値平均",
                    $"averageTop8Retention,{summary.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture)},Elo上位8名が総合上位8位に残る人数の期待値",
                    $"eloTop1OverallTop1Probability,{(summary.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture)},Elo1位が総合1位になる確率(%)",
                    $"mostPenalizedPlayerDelta,{summary.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture)},{EscapeCsv(summary.MostPenalizedPlayerName)}",
                    $"mostAdvantagedPlayerDelta,{summary.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture)},{EscapeCsv(summary.MostAdvantagedPlayerName)}"
                };

                // 評価メモがある場合はCSVの最後に追加する
                if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
                {
                    lines.Add($"evaluationMemo,,{EscapeCsv(options.EvaluationMemo)}");
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="sweepRows"></param>
    /// <param name="sweepCsvPath"></param>
    /// <param name="options"></param>
    static void WriteQualitySweepMarkdown(string outputMarkdownPath, IReadOnlyList<QualitySweepRow> sweepRows, string sweepCsvPath, ExperimentalReportGroupingOptions options)
    {
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () =>
            {
                var bestSpearmanRow = sweepRows
                    .OrderByDescending(row => row.SpearmanCorrelation)
                    .ThenBy(row => row.MeanAbsoluteRankError)
                    .ThenBy(row => row.FirstPlayerWinRatePercent)
                    .FirstOrDefault();
                var bestMaeRow = sweepRows
                    .OrderBy(row => row.MeanAbsoluteRankError)
                    .ThenBy(row => row.FirstPlayerWinRatePercent)
                    .FirstOrDefault();
                var bestTop1Row = sweepRows
                    .OrderByDescending(row => row.EloTop1OverallTop1Probability)
                    .ThenBy(row => row.FirstPlayerWinRatePercent)
                    .FirstOrDefault();

                var lines = new List<string>
                {
                    "# n% スイープ結果レポート",
                    string.Empty,
                    "## 概要",
                    $"- 評価点数: {sweepRows.Count}",
                    $"- 出力CSV: {BuildMarkdownFileLink(outputMarkdownPath, sweepCsvPath)}"
                };

                if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
                {
                    lines.Add($"- 評価メモ: {options.EvaluationMemo}");
                }

                if (sweepRows.Count > 0)
                {
                    var recommendedRows = sweepRows
                        .Where(row => row.SpearmanCorrelation >= bestSpearmanRow.SpearmanCorrelation - 0.001
                            && row.MeanAbsoluteRankError <= bestMaeRow.MeanAbsoluteRankError + 0.05)
                        .OrderBy(row => row.FirstPlayerWinRatePercent)
                        .ToArray();

                    lines.AddRange(new[]
                    {
                        string.Empty,
                        "## 注目ポイント",
                        $"- Spearman 相関が最良の点: **{bestSpearmanRow.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%**（{bestSpearmanRow.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture)}）",
                        $"- 平均順位ずれが最良の点: **{bestMaeRow.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%**（{bestMaeRow.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture)}）",
                        $"- Elo1位の総合1位確率が最良の点: **{bestTop1Row.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%**（{(bestTop1Row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture)}%）",
                        $"- 自動おすすめ帯: **{BuildRecommendedSweepBandText(recommendedRows, bestMaeRow.FirstPlayerWinRatePercent)}**",
                        string.Empty,
                        "## 一覧表",
                        "| 先手勝率 | Spearman 相関 | 平均順位ずれ | Elo上位8名残留 | Elo1位の総合1位確率 | 最大不利益 | 最大利益 |",
                        "| ---: | ---: | ---: | ---: | ---: | --- | --- |"
                    });

                    lines.AddRange(sweepRows.Select(row =>
                        $"| {row.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}% | {row.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture)} | {row.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture)} | {row.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture)} | {(row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture)}% | {row.MostPenalizedPlayerName} ({FormatSignedDelta(row.MostPenalizedDelta)}) | {row.MostAdvantagedPlayerName} ({FormatSignedDelta(row.MostAdvantagedDelta)}) |"));

                    lines.AddRange(new[]
                    {
                        string.Empty,
                        "## 推移図",
                        "```mermaid",
                        "xychart-beta",
                        "    title \"n%スイープの主要指標\"",
                        "    x-axis \"先手勝率(%)\" [" + string.Join(", ", sweepRows.Select(row => row.FirstPlayerWinRatePercent.ToString("F0", CultureInfo.InvariantCulture))) + "]",
                        "    y-axis \"値\" 0 --> 100",
                        "    line \"Elo1位の総合1位確率(%)\" [" + string.Join(", ", sweepRows.Select(row => (row.EloTop1OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                        "    line \"Elo上位8名残留\" [" + string.Join(", ", sweepRows.Select(row => row.AverageTop8Retention.ToString("F2", CultureInfo.InvariantCulture))) + "]",
                        "```"
                    });
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="qualityEvaluationRun"></param>
    /// <param name="summaryCsvPath"></param>
    /// <param name="playerCsvPath"></param>
    /// <param name="options"></param>
    static void WriteQualitySummaryMarkdown(
        string outputMarkdownPath,
        QualityEvaluationRun qualityEvaluationRun,
        string summaryCsvPath,
        string playerCsvPath,
        ExperimentalReportGroupingOptions options)
    {
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () =>
            {
                var summary = qualityEvaluationRun.Summary;
                var topPenalizedPlayers = qualityEvaluationRun.PlayerRows
                    .OrderByDescending(row => row.OverallPlaceDeltaFromEloRank)
                    .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToArray();
                var topAdvantagedPlayers = qualityEvaluationRun.PlayerRows
                    .OrderBy(row => row.OverallPlaceDeltaFromEloRank)
                    .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToArray();
                var bestTop1Row = qualityEvaluationRun.PlayerRows
                    .OrderByDescending(row => row.OverallTop1Probability)
                    .ThenBy(row => row.ExpectedOverallPlace)
                    .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                var lines = new List<string>
                {
                    "# 品質評価サマリーレポート",
                    string.Empty,
                    "## 概要",
                    $"- 計算モード: {qualityEvaluationRun.CalculationMode}",
                    $"- 対象選手数: {qualityEvaluationRun.PlayerRows.Count}",
                    $"- サマリーCSV: {BuildMarkdownFileLink(outputMarkdownPath, summaryCsvPath)}",
                    $"- 選手別CSV: {BuildMarkdownFileLink(outputMarkdownPath, playerCsvPath)}"
                };

                if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
                {
                    lines.Add($"- 評価メモ: {options.EvaluationMemo}");
                }

                lines.AddRange(new[]
                {
                    string.Empty,
                    "## 指標サマリー",
                    "| 指標 | 値 | 意味 |",
                    "| --- | ---: | --- |",
                    $"| Spearman 相関 | {summary.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture)} | Elo順位と期待総合順位の相関 |",
                    $"| 平均順位ずれ | {summary.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture)} | 期待総合順位とElo順位のずれの絶対値平均 |",
                    $"| Elo上位8名の総合上位8位残留人数 | {summary.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture)} | Elo上位8名が総合上位8位に残る人数の期待値 |",
                    $"| Elo1位の総合1位確率 | {(summary.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture)}% | Elo1位が総合1位になる確率 |",
                    string.Empty,
                    "## 着目選手",
                    $"- 最大不利益: **{summary.MostPenalizedPlayerName}** ({FormatSignedDelta(summary.MostPenalizedDelta)})",
                    $"- 最大利益: **{summary.MostAdvantagedPlayerName}** ({FormatSignedDelta(summary.MostAdvantagedDelta)})",
                    $"- 総合1位確率が最も高い選手: **{bestTop1Row.Name}**（{(bestTop1Row.OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                    string.Empty,
                    "## 自動コメント",
                    $"- 実力順の並び: {BuildSpearmanComment(summary.SpearmanCorrelation)}",
                    $"- 平均順位の安定感: {BuildMeanAbsoluteRankErrorComment(summary.MeanAbsoluteRankError)}",
                    $"- 上位8名の残留: {BuildTop8RetentionComment(summary.AverageTop8Retention)}",
                    $"- 最強者の押し上げ: {BuildTop1Comment(summary.EloTop1OverallTop1Probability)}",
                    string.Empty,
                    "### 不利益が大きい選手",
                    "| 選手 | Elo順位 | 期待総合順位 | ずれ | 総合1位確率 | 総合上位8位確率 |",
                    "| --- | ---: | ---: | ---: | ---: | ---: |"
                });

                lines.AddRange(topPenalizedPlayers.Select(BuildQualityPlayerMarkdownRow));

                lines.AddRange(new[]
                {
                    string.Empty,
                    "### 利益が大きい選手",
                    "| 選手 | Elo順位 | 期待総合順位 | ずれ | 総合1位確率 | 総合上位8位確率 |",
                    "| --- | ---: | ---: | ---: | ---: | ---: |"
                });

                lines.AddRange(topAdvantagedPlayers.Select(BuildQualityPlayerMarkdownRow));

                if (qualityEvaluationRun.PlayerRows.Count > 0)
                {
                    var chartRows = qualityEvaluationRun.PlayerRows
                        .OrderBy(row => row.EloRank)
                        .Take(8)
                        .ToArray();

                    lines.AddRange(new[]
                    {
                        string.Empty,
                        "## Mermaid 図",
                        "```mermaid",
                        "xychart-beta",
                        "    title \"Elo上位8名の期待総合順位\"",
                        "    x-axis [" + BuildMermaidCategoryList(chartRows.Select(row => row.Name)) + "]",
                        "    y-axis \"期待総合順位\" 1 --> " + Math.Max(8, qualityEvaluationRun.PlayerRows.Count).ToString(CultureInfo.InvariantCulture),
                        "    bar [" + string.Join(", ", chartRows.Select(row => row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture))) + "]",
                        "```",
                        string.Empty,
                        "```mermaid",
                        "xychart-beta",
                        "    title \"Elo上位8名の総合1位確率\"",
                        "    x-axis [" + BuildMermaidCategoryList(chartRows.Select(row => row.Name)) + "]",
                        "    y-axis \"総合1位確率(%)\" 0 --> 100",
                        "    bar [" + string.Join(", ", chartRows.Select(row => (row.OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                        "```"
                    });
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="sweepRows"></param>
    /// <param name="options"></param>
    static void WriteQualitySweepCsv(string outputCsvPath, IReadOnlyList<QualitySweepRow> sweepRows, ExperimentalReportGroupingOptions options)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () =>
            {
                var lines = new List<string>
                {
                    "firstPlayerWinRatePercent,spearmanCorrelation,meanAbsoluteRankError,averageTop8Retention,eloTop1OverallTop1ProbabilityPercent,mostPenalizedPlayer,mostPenalizedDelta,mostAdvantagedPlayer,mostAdvantagedDelta"
                };

                lines.AddRange(sweepRows.Select(row => string.Join(",",
                    row.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
                    row.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture),
                    row.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture),
                    row.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture),
                    (row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture),
                    EscapeCsv(row.MostPenalizedPlayerName),
                    row.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture),
                    EscapeCsv(row.MostAdvantagedPlayerName),
                    row.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture))));

                if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
                {
                    lines.Add($"evaluationMemo,,,,,,,{EscapeCsv(options.EvaluationMemo)},");
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    static string BuildQualityPlayerMarkdownRow(QualityPlayerRow row)
    {
        return string.Join(" | ",
            "|",
            row.Name,
            row.EloRank.ToString(CultureInfo.InvariantCulture),
            row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture),
            FormatSignedDelta(row.OverallPlaceDeltaFromEloRank),
            ((row.OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture) + "%"),
            ((row.OverallTop8Probability * 100).ToString("F2", CultureInfo.InvariantCulture) + "%"),
            string.Empty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    static string FormatSignedDelta(double value)
    {
        return value >= 0
            ? "+" + value.ToString("F6", CultureInfo.InvariantCulture)
            : value.ToString("F6", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spearmanCorrelation"></param>
    /// <returns></returns>
    static string BuildSpearmanComment(double spearmanCorrelation)
    {
        return spearmanCorrelation switch
        {
            >= 0.999 => "かなり強く保たれています。",
            >= 0.99 => "概ね保たれています。",
            >= 0.95 => "少し崩れ始めています。",
            _ => "はっきり崩れています。",
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="meanAbsoluteRankError"></param>
    /// <returns></returns>
    static string BuildMeanAbsoluteRankErrorComment(double meanAbsoluteRankError)
    {
        return meanAbsoluteRankError switch
        {
            <= 1.2 => "かなり小さめです。",
            <= 1.6 => "比較的おだやかです。",
            <= 2.0 => "やや大きめです。",
            _ => "大きめです。",
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="averageTop8Retention"></param>
    /// <returns></returns>
    static string BuildTop8RetentionComment(double averageTop8Retention)
    {
        return averageTop8Retention switch
        {
            >= 7.95 => "ほぼ完全に保たれています。",
            >= 7.5 => "かなり保たれています。",
            >= 7.0 => "少し崩れています。",
            _ => "はっきり崩れています。",
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="top1Probability"></param>
    /// <returns></returns>
    static string BuildTop1Comment(double top1Probability)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="averagePlace"></param>
    /// <returns></returns>
    static string BuildAveragePlaceComment(double averagePlace)
    {
        return averagePlace switch
        {
            <= 2.0 => "かなり前寄りです。",
            <= 3.5 => "比較的前寄りです。",
            <= 5.0 => "中位上側です。",
            _ => "まだ混戦気味です。",
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="biggestBoost"></param>
    /// <param name="biggestDrop"></param>
    /// <returns></returns>
    static string BuildRatingDeltaComment(double biggestBoost, double biggestDrop)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="groupPlace1Probability"></param>
    /// <param name="groupPlaceAverage"></param>
    /// <returns></returns>
    static string BuildGroupLeadComment(double groupPlace1Probability, double groupPlaceAverage)
    {
        var percent = groupPlace1Probability * 100;
        if (percent >= 35.0 && groupPlaceAverage <= 2.0) return "先頭がかなりはっきりしています。";
        if (percent >= 20.0 && groupPlaceAverage <= 3.0) return "先頭候補が見えています。";
        return "まだ横並び気味です。";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apexTopProbability"></param>
    /// <param name="innovTopProbability"></param>
    /// <returns></returns>
    static string BuildApexInnovGapComment(double apexTopProbability, double innovTopProbability)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="recommendedRows"></param>
    /// <param name="fallbackPercent"></param>
    /// <returns></returns>
    static string BuildRecommendedSweepBandText(IReadOnlyList<QualitySweepRow> recommendedRows, double fallbackPercent)
    {
        if (recommendedRows.Count == 0) return fallbackPercent.ToString("F2", CultureInfo.InvariantCulture) + "% 付近";

        var start = recommendedRows[0].FirstPlayerWinRatePercent;
        var end = recommendedRows[^1].FirstPlayerWinRatePercent;
        return Math.Abs(start - end) < 0.000001
            ? start.ToString("F2", CultureInfo.InvariantCulture) + "% 付近"
            : start.ToString("F2", CultureInfo.InvariantCulture) + "%〜" + end.ToString("F2", CultureInfo.InvariantCulture) + "%";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="playerRows"></param>
    static void WriteQualityPlayerCsv(string outputCsvPath, IReadOnlyList<QualityPlayerRow> playerRows)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () =>
            {
                var lines = new List<string>
                {
                    "playerName,group,originalElo,eloRank,expectedOverallPlace,overallPlaceDeltaFromEloRank,overallTop1ProbabilityPercent,overallTop8ProbabilityPercent"
                };

                lines.AddRange(playerRows.Select(row => string.Join(",",
                    EscapeCsv(row.Name),
                    EscapeCsv(row.Group),
                    FormatRating(row.OriginalRating),
                    row.EloRank.ToString(CultureInfo.InvariantCulture),
                    row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture),
                    row.OverallPlaceDeltaFromEloRank.ToString("F3", CultureInfo.InvariantCulture),
                    (row.OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
                    (row.OverallTop8Probability * 100).ToString("F2", CultureInfo.InvariantCulture))));

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    static void WriteResultCsv(string outputCsvPath, string mode, double firstPlayerWinRatePercent, IReadOnlyList<ResultRow> resultRows)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () =>
            {
                var lines = new List<string>();
                var headerColumns = new List<string>
                {
                    "calculationMode",
                    "firstPlayerWinRatePercent",
                    "playerName",
                    "originalElo",
                    "effectiveElo",
                    "eloDelta",
                    "firstPlayerCount",
                    "secondPlayerCount",
                    "firstPlayerWinRatePercent",
                    "secondPlayerWinRatePercent",
                    "championshipProbabilityPercent",
                    "averagePlace"
                };

                if (resultRows.Count > 0)
                {
                    for (var place = 0; place < resultRows[0].PlaceProbabilities.Length; place++)
                    {
                        headerColumns.Add($"place{place + 1}Percent");
                        if (resultRows[0].PlaceCounts is not null)
                        {
                            headerColumns.Add($"place{place + 1}Count");
                        }
                    }
                }

                lines.Add(string.Join(",", headerColumns.Select(EscapeCsv)));

                foreach (var row in resultRows)
                {
                    var columns = new List<string>
                    {
                        mode,
                        firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
                        row.Name,
                        FormatRating(row.OriginalRating),
                        FormatRating(row.EffectiveRating),
                        FormatSignedRating(row.RatingDelta),
                        row.FirstPlayerCount.ToString(CultureInfo.InvariantCulture),
                        row.SecondPlayerCount.ToString(CultureInfo.InvariantCulture),
                        FormatOptionalPercentValue(row.FirstPlayerWinRate),
                        FormatOptionalPercentValue(row.SecondPlayerWinRate),
                        (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture),
                        row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)
                    };

                    for (var place = 0; place < row.PlaceProbabilities.Length; place++)
                    {
                        columns.Add((row.PlaceProbabilities[place] * 100).ToString("F2", CultureInfo.InvariantCulture));
                        if (row.PlaceCounts is not null)
                        {
                            columns.Add(row.PlaceCounts[place].ToString("F3", CultureInfo.InvariantCulture));
                        }
                    }

                    lines.Add(string.Join(",", columns.Select(EscapeCsv)));
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    static void WriteResultMarkdown(string outputMarkdownPath, string outputCsvPath, string mode, double firstPlayerWinRatePercent, IReadOnlyList<ResultRow> resultRows)
    {
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () =>
            {
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

                var lines = new List<string>
                {
                    "# 通常モード結果レポート",
                    string.Empty,
                    "## 概要",
                    $"- 結果CSV: {BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
                    $"- 計算モード: {mode}",
                    $"- 同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%",
                    $"- 対象選手数: {resultRows.Count}",
                    string.Empty,
                    "## 注目ポイント",
                    $"- 優勝確率が最も高い選手: **{bestChampionshipRow.Name}**（{(bestChampionshipRow.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                    $"- 平均順位が最も良い選手: **{bestAveragePlaceRow.Name}**（{bestAveragePlaceRow.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)}）",
                    $"- 実効Elo差分が最も大きくプラスの選手: **{biggestBoostRow.Name}**（{FormatSignedRating(biggestBoostRow.RatingDelta)}）",
                    $"- 実効Elo差分が最も大きくマイナスの選手: **{biggestDropRow.Name}**（{FormatSignedRating(biggestDropRow.RatingDelta)}）",
                    string.Empty,
                    "## 自動コメント",
                    $"- 優勝候補の強さ: {BuildTop1Comment(bestChampionshipRow.ChampionshipProbability)}",
                    $"- 先頭の平均順位: {BuildAveragePlaceComment(bestAveragePlaceRow.AveragePlace)}",
                    $"- 実効Eloの押し上げ: {BuildRatingDeltaComment(biggestBoostRow.RatingDelta, biggestDropRow.RatingDelta)}",
                    string.Empty,
                    "## 上位候補一覧",
                    "| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |",
                    "| --- | ---: | ---: | ---: | ---: | ---: |"
                };

                lines.AddRange(topChampionshipRows.Select(row =>
                    $"| {row.Name} | {FormatRating(row.OriginalRating)} | {FormatRating(row.EffectiveRating)} | {FormatSignedRating(row.RatingDelta)} | {(row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} |"));

                if (topChampionshipRows.Length > 0)
                {
                    lines.AddRange(new[]
                    {
                        string.Empty,
                        "## Mermaid 図",
                        "```mermaid",
                        "xychart-beta",
                        "    title \"上位候補の優勝確率\"",
                        "    x-axis [" + BuildMermaidCategoryList(topChampionshipRows.Select(row => row.Name)) + "]",
                        "    y-axis \"優勝確率(%)\" 0 --> 100",
                        "    bar [" + string.Join(", ", topChampionshipRows.Select(row => (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                        "```",
                        string.Empty,
                        "```mermaid",
                        "xychart-beta",
                        "    title \"上位候補の平均順位\"",
                        "    x-axis [" + BuildMermaidCategoryList(topChampionshipRows.Select(row => row.Name)) + "]",
                        "    y-axis \"平均順位\" 1 --> " + Math.Max(2, resultRows.Count).ToString(CultureInfo.InvariantCulture),
                        "    bar [" + string.Join(", ", topChampionshipRows.Select(row => row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture))) + "]",
                        "```"
                    });
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    static void WriteFinalStageResultCsv(string outputCsvPath, string mode, double firstPlayerWinRatePercent, IReadOnlyList<FinalStageResultRow> resultRows)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () =>
            {
                var lines = new List<string>();
                var headerColumns = new List<string>
                {
                    "calculationMode",
                    "firstPlayerWinRatePercent",
                    "playerName",
                    "group",
                    "originalElo",
                    "effectiveElo",
                    "eloDelta",
                    "firstPlayerCount",
                    "secondPlayerCount",
                    "firstPlayerWinRatePercent",
                    "secondPlayerWinRatePercent",
                    "groupPlace1ProbabilityPercent",
                    "groupPlaceAverage",
                    "overallPlace1ProbabilityPercent",
                    "overallPlaceAverage"
                };

                if (resultRows.Count > 0)
                {
                    for (var place = 0; place < resultRows[0].PlaceProbabilities.Length; place++)
                    {
                        headerColumns.Add($"place{place + 1}Percent");
                        if (resultRows[0].PlaceCounts is not null)
                        {
                            headerColumns.Add($"place{place + 1}Count");
                        }
                    }
                }

                lines.Add(string.Join(",", headerColumns.Select(EscapeCsv)));

                foreach (var row in resultRows)
                {
                    var columns = new List<string>
                    {
                        mode,
                        firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
                        row.Name,
                        row.Group,
                        FormatRating(row.OriginalRating),
                        FormatRating(row.EffectiveRating),
                        FormatSignedRating(row.RatingDelta),
                        row.FirstPlayerCount.ToString(CultureInfo.InvariantCulture),
                        row.SecondPlayerCount.ToString(CultureInfo.InvariantCulture),
                        FormatOptionalPercentValue(row.FirstPlayerWinRate),
                        FormatOptionalPercentValue(row.SecondPlayerWinRate),
                        (row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
                        row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture),
                        (row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
                        row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)
                    };

                    for (var place = 0; place < row.PlaceProbabilities.Length; place++)
                    {
                        columns.Add((row.PlaceProbabilities[place] * 100).ToString("F2", CultureInfo.InvariantCulture));
                        if (row.PlaceCounts is not null)
                        {
                            columns.Add(row.PlaceCounts[place].ToString("F3", CultureInfo.InvariantCulture));
                        }
                    }

                    lines.Add(string.Join(",", columns.Select(EscapeCsv)));
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="outputCsvPath"></param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="referenceMatchesCsvPath"></param>
    static void WriteFinalStageResultMarkdown(string outputMarkdownPath, string outputCsvPath, string mode, double firstPlayerWinRatePercent, IReadOnlyList<FinalStageResultRow> resultRows, string? referenceMatchesCsvPath = null)
    {
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () =>
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

                var lines = new List<string>
                {
                    "# 本戦モード結果レポート",
                    string.Empty,
                    "## 概要",
                    $"- 結果CSV: {BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
                    $"- 計算モード: {mode}",
                    $"- 同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%",
                    $"- 対象選手数: {resultRows.Count}",
                    string.Empty,
                    "## 注目ポイント",
                    $"- 総合1位確率が最も高い選手: **{bestOverallRow.Name}**（{(bestOverallRow.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                    $"- Apex で最も有力な選手: **{bestApexRow.Name}**（グループ1位確率 {(bestApexRow.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                    $"- Innov で最も有力な選手: **{bestInnovRow.Name}**（グループ1位確率 {(bestInnovRow.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                    string.Empty,
                    "## 自動コメント",
                    $"- 総合1位候補の強さ: {BuildTop1Comment(bestOverallRow.OverallPlace1Probability)}",
                    $"- Apex の先頭感: {BuildGroupLeadComment(bestApexRow.GroupPlace1Probability, bestApexRow.GroupPlaceAverage)}",
                    $"- Innov の先頭感: {BuildGroupLeadComment(bestInnovRow.GroupPlace1Probability, bestInnovRow.GroupPlaceAverage)}",
                    $"- Apex / Innov の先頭差: {BuildApexInnovGapComment(bestApexRow.GroupPlace1Probability, bestInnovRow.GroupPlace1Probability)}",
                    string.Empty,
                    "## 上位候補一覧",
                    "| 選手 | グループ | 元Elo | 実効Elo | 差分 | グループ1位確率 | 総合1位確率 | 総合平均順位 |",
                    "| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |"
                };

                if (!string.IsNullOrWhiteSpace(referenceMatchesCsvPath))
                {
                    lines.Insert(4, $"- 参考対局CSV: {BuildMarkdownFileLink(outputMarkdownPath, referenceMatchesCsvPath)}");
                }

                lines.AddRange(topRows.Select(row =>
                    $"| {row.Name} | {row.Group} | {FormatRating(row.OriginalRating)} | {FormatRating(row.EffectiveRating)} | {FormatSignedRating(row.RatingDelta)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));

                if (apexRows.Length > 0)
                {
                    lines.AddRange(new[]
                    {
                        string.Empty,
                        "## Apex 注目候補",
                        "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |",
                        "| --- | ---: | ---: | ---: | ---: | ---: |"
                    });

                    lines.AddRange(apexRows.Select(row =>
                        $"| {row.Name} | {FormatRating(row.OriginalRating)} | {FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
                }

                if (innovRows.Length > 0)
                {
                    lines.AddRange(new[]
                    {
                        string.Empty,
                        "## Innov 注目候補",
                        "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |",
                        "| --- | ---: | ---: | ---: | ---: | ---: |"
                    });

                    lines.AddRange(innovRows.Select(row =>
                        $"| {row.Name} | {FormatRating(row.OriginalRating)} | {FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
                }

                if (topRows.Length > 0)
                {
                    lines.AddRange(new[]
                    {
                        string.Empty,
                        "## Mermaid 図",
                        "```mermaid",
                        "xychart-beta",
                        "    title \"上位候補の総合1位確率\"",
                        "    x-axis [" + BuildMermaidCategoryList(topRows.Select(row => row.Name)) + "]",
                        "    y-axis \"総合1位確率(%)\" 0 --> 100",
                        "    bar [" + string.Join(", ", topRows.Select(row => (row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                        "```",
                        string.Empty,
                        "```mermaid",
                        "xychart-beta",
                        "    title \"上位候補のグループ1位確率\"",
                        "    x-axis [" + BuildMermaidCategoryList(topRows.Select(row => row.Name)) + "]",
                        "    y-axis \"グループ1位確率(%)\" 0 --> 100",
                        "    bar [" + string.Join(", ", topRows.Select(row => (row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                        "```"
                    });
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="labels"></param>
    /// <returns></returns>
    static string BuildMermaidCategoryList(IEnumerable<string> labels)
    {
        return string.Join(", ", labels.Select(EscapeMermaidLabel));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label"></param>
    /// <returns></returns>
    static string EscapeMermaidLabel(string label)
    {
        return "\"" + label.Replace("\"", "'") + "\"";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="markdownPath"></param>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    static string BuildMarkdownFileLink(string markdownPath, string targetPath)
    {
        var markdownDirectory = Path.GetDirectoryName(Path.GetFullPath(markdownPath)) ?? Path.GetFullPath(".");
        var fullTargetPath = Path.GetFullPath(targetPath);
        var relativePath = Path.GetRelativePath(markdownDirectory, fullTargetPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
        var fileName = Path.GetFileName(targetPath);
        return $"[{fileName}]({relativePath})";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputCsvPath"></param>
    /// <param name="stages"></param>
    /// <param name="players"></param>
    /// <param name="matchRecords"></param>
    static void WriteTournamentMatchRecordCsv(string outputCsvPath, IReadOnlyList<StageEntry> stages, IReadOnlyList<PlayerEntry> players, IReadOnlyList<TournamentMatchRecord> matchRecords)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () =>
            {
                var stageNameById = stages.ToDictionary(stage => stage.StageId, stage => stage.StageName);
                var playerNameById = players.ToDictionary(player => player.PlayerId, player => player.Name);
                var lines = new List<string>
                {
                    "matchId,stageId,stageName,firstPlayerId,firstPlayerName,secondPlayerId,secondPlayerName,startTime,endTime,status,resultType,roundNo"
                };

                foreach (var match in matchRecords.OrderBy(match => match.StartTime).ThenBy(match => match.MatchId))
                {
                    var stageName = stageNameById.TryGetValue(match.StageId, out var stage) ? stage : string.Empty;
                    var firstPlayerName = playerNameById.TryGetValue(match.FirstPlayerId, out var firstPlayer) ? firstPlayer : string.Empty;
                    var secondPlayerName = playerNameById.TryGetValue(match.SecondPlayerId, out var secondPlayer) ? secondPlayer : string.Empty;

                    var columns = new[]
                    {
                        match.MatchId.ToString(CultureInfo.InvariantCulture),
                        match.StageId.ToString(CultureInfo.InvariantCulture),
                        stageName,
                        match.FirstPlayerId.ToString(CultureInfo.InvariantCulture),
                        firstPlayerName,
                        match.SecondPlayerId.ToString(CultureInfo.InvariantCulture),
                        secondPlayerName,
                        match.StartTime.ToString(CultureInfo.InvariantCulture),
                        match.EndTime.ToString(CultureInfo.InvariantCulture),
                        match.Status.ToString(),
                        match.ResultType.ToString(),
                        match.RoundNo?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    };

                    lines.Add(string.Join(",", columns.Select(EscapeCsv)));
                }

                return lines;
            });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputMarkdownPath"></param>
    /// <param name="outputCsvPath"></param>
    /// <param name="stages"></param>
    /// <param name="players"></param>
    /// <param name="matchRecords"></param>
    static void WriteTournamentMatchRecordMarkdown(string outputMarkdownPath, string outputCsvPath, IReadOnlyList<StageEntry> stages, IReadOnlyList<PlayerEntry> players, IReadOnlyList<TournamentMatchRecord> matchRecords)
    {
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () =>
            {
                var stageNameById = stages.ToDictionary(stage => stage.StageId, stage => stage.StageName);
                var playerNameById = players.ToDictionary(player => player.PlayerId, player => player.Name);
                var orderedMatches = matchRecords
                    .OrderBy(match => match.StartTime)
                    .ThenBy(match => match.MatchId)
                    .ToArray();
                var finishedCount = orderedMatches.Count(match => match.Status == MatchStatus.Finished);
                var cancelledCount = orderedMatches.Count(match => match.Status == MatchStatus.Cancelled);

                var lines = new List<string>
                {
                    "# 大会結果テーブル",
                    string.Empty,
                    "## 概要",
                    $"- 結果CSV: {BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
                    $"- 総対局数: {orderedMatches.Length}",
                    $"- ステージ数: {stages.Count}",
                    $"- 完了対局数: {finishedCount}",
                    $"- 中止対局数: {cancelledCount}",
                    string.Empty,
                    "## 一覧表",
                    "| 対局番号 | ステージ | 先手 | 後手 | 開始 | 終了 | 状態 | 結果 | ラウンド |",
                    "| ---: | --- | --- | --- | ---: | ---: | --- | --- | ---: |"
                };

                foreach (var match in orderedMatches)
                {
                    var stageName = stageNameById.TryGetValue(match.StageId, out var stage) ? stage : match.StageId.ToString(CultureInfo.InvariantCulture);
                    var firstPlayerName = playerNameById.TryGetValue(match.FirstPlayerId, out var firstPlayer) ? firstPlayer : match.FirstPlayerId.ToString(CultureInfo.InvariantCulture);
                    var secondPlayerName = playerNameById.TryGetValue(match.SecondPlayerId, out var secondPlayer) ? secondPlayer : match.SecondPlayerId.ToString(CultureInfo.InvariantCulture);
                    var roundText = match.RoundNo?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                    lines.Add($"| {match.MatchId.ToString(CultureInfo.InvariantCulture)} | {stageName} | {firstPlayerName} | {secondPlayerName} | {match.StartTime.ToString(CultureInfo.InvariantCulture)} | {match.EndTime.ToString(CultureInfo.InvariantCulture)} | {match.Status} | {match.ResultType} | {roundText} |");
                }

                return lines;
            });
    }
}

