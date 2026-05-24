/*
 * ［大会品質評価レポートという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentQualityReport;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System.Globalization;

/// <summary>
/// ［大会品質評価レポート］のデータファイルを作成するクラスだぜ（＾▽＾）！
/// </summary>
internal static class TournamentQualityReportDataFileWriter
{
    static string EscapeCsv(string value) => CsvOutputHelpers.EscapeCsv(value);

    static IEnumerable<string> BuildAdjustmentCycleAdviceLines(bool timedOut, bool zeroResults, int completedCount, string subjectLabel)
    {
        if (!timedOut && !zeroResults) return Array.Empty<string>();

        return new[]
        {
            string.Empty,
            "## 次回調整のヒント",
            $"- 今回の {subjectLabel} は {(zeroResults ? "0件" : "途中まで")} で止まりました。",
            $"- 今回最後まで計算できた件数: {completedCount}",
            "- 次回は次のどれかを短くすると回しやすいです。",
            "  - 先手勝率の終了値を手前にする",
            "  - 刻み幅を大きくする",
            "  - シミュレーション試行回数を減らす",
            "  - 対局数や対象条件を絞る"
        };
    }

    static IEnumerable<string> BuildNextCycleSuggestionMarkdownLines(TournamentQualityNextCycleSuggestion suggestion)
    {
        if (string.IsNullOrWhiteSpace(suggestion.Title)) return Array.Empty<string>();

        var lines = new List<string>
        {
            string.Empty,
            "## 次回の具体設定案",
            $"- {suggestion.Title}"
        };

        lines.AddRange(suggestion.SuggestedSettings.Select(setting => $"  - {setting}"));
        if (!string.IsNullOrWhiteSpace(suggestion.Reason))
        {
            lines.Add($"- 理由: {suggestion.Reason}");
        }

        return lines;
    }

    static IEnumerable<string> BuildNextCycleSuggestionCsvLines(TournamentQualityNextCycleSuggestion suggestion)
    {
        if (string.IsNullOrWhiteSpace(suggestion.Title)) return Array.Empty<string>();

        var lines = new List<string>
        {
            $"nextCycleSuggestionTitle,,{EscapeCsv(suggestion.Title)}"
        };

        lines.AddRange(suggestion.SuggestedSettings.Select(setting => $"nextCycleSuggestedSetting,,{EscapeCsv(setting)}"));
        if (!string.IsNullOrWhiteSpace(suggestion.Reason))
        {
            lines.Add($"nextCycleSuggestionReason,,{EscapeCsv(suggestion.Reason)}");
        }

        return lines;
    }

    internal static IEnumerable<string> CreateTournamentQualityReportSummaryCsv(TournamentQualityReportSummary summary, TournamentQualityEvaluationReportGroupingOptions options, TournamentQualityNextCycleSuggestion suggestion)
    {
        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(new[] { "metricName", "metricValue", "note" }).Select(EscapeCsv))
        };

        lines.AddRange(new[]
        {
            string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "metric", "spearmanCorrelation", summary.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture), "Elo順位と期待総合順位の相関").Select(EscapeCsv)),
            string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "metric", "meanAbsoluteRankError", summary.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture), "期待総合順位とElo順位のずれの絶対値平均").Select(EscapeCsv)),
            string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "metric", "averageTop8Retention", summary.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture), "Elo上位8名が総合上位8位に残る人数の期待値").Select(EscapeCsv)),
            string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "metric", "eloTop1OverallTop1Probability", (summary.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture), "Elo1位が総合1位になる確率(%)").Select(EscapeCsv)),
            string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "metric", "mostPenalizedPlayerDelta", summary.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture), summary.MostPenalizedPlayerName).Select(EscapeCsv)),
            string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "metric", "mostAdvantagedPlayerDelta", summary.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture), summary.MostAdvantagedPlayerName).Select(EscapeCsv))
        });

        if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
        {
            lines.Add(string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "meta", "evaluationMemo", string.Empty, options.EvaluationMemo).Select(EscapeCsv)));
        }

        if (string.IsNullOrWhiteSpace(summary.MostPenalizedPlayerName) && string.IsNullOrWhiteSpace(summary.MostAdvantagedPlayerName))
        {
            lines.Add(string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "summaryMetrics", "meta", "adjustmentHint", string.Empty, "結果が0件なら先手勝率範囲・刻み幅・試行回数・対局条件を短くして再試行してください").Select(EscapeCsv)));
        }

        lines.AddRange(BuildNextCycleSuggestionCsvLines(suggestion));
        return lines;
    }

    internal static IEnumerable<string> CreateTournamentQualitySweepReportMarkdown(string outputMarkdownPath, IReadOnlyList<TournamentQualitySweepReportRow> sweepRows, string sweepCsvPath, TournamentQualityEvaluationReportGroupingOptions options, bool stoppedByTimeout, TournamentQualityNextCycleSuggestion suggestion)
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
            $"- 出力CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, sweepCsvPath)}"
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

        lines.AddRange(BuildAdjustmentCycleAdviceLines(
            timedOut: stoppedByTimeout,
            zeroResults: sweepRows.Count == 0,
            completedCount: sweepRows.Count,
            subjectLabel: "n%スイープ"));
        lines.AddRange(BuildNextCycleSuggestionMarkdownLines(suggestion));

        return lines;
    }

    internal static IEnumerable<string> CreateTournamentQualityReportSummaryMarkdown(
        string outputMarkdownPath,
        IReadOnlyList<TournamentQualityReportPlayerRow> playerRows,
        TournamentQualityReportSummary summary,
        string calculationMode,
        string summaryCsvPath,
        string playerCsvPath,
        TournamentQualityEvaluationReportGroupingOptions options,
        TournamentQualityNextCycleSuggestion suggestion)
    {
        var topPenalizedPlayers = playerRows
            .OrderByDescending(row => row.OverallPlaceDeltaFromEloRank)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToArray();
        var topAdvantagedPlayers = playerRows
            .OrderBy(row => row.OverallPlaceDeltaFromEloRank)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToArray();
        var bestTop1Rows = playerRows
            .OrderByDescending(row => row.OverallTop1Probability)
            .ThenBy(row => row.ExpectedOverallPlace)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(1)
            .ToArray();

        var lines = new List<string>
        {
            "# 品質評価サマリーレポート",
            string.Empty,
            "## 概要",
            $"- 計算モード: {calculationMode}",
            $"- 対象選手数: {playerRows.Count}",
            $"- サマリーCSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, summaryCsvPath)}",
            $"- 選手別CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, playerCsvPath)}"
        };

        var bestTop1PlayerName = bestTop1Rows.Length == 0 ? "該当なし" : bestTop1Rows[0].Name;
        var bestTop1ProbabilityText = bestTop1Rows.Length == 0
            ? "0.00"
            : (bestTop1Rows[0].OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture);

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
            $"- 総合1位確率が最も高い選手: **{bestTop1PlayerName}**（{bestTop1ProbabilityText}%）",
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

        lines.AddRange(topPenalizedPlayers.Select(BuildTournamentQualityReportPlayerMarkdownRow));

        lines.AddRange(new[]
        {
            string.Empty,
            "### 利益が大きい選手",
            "| 選手 | Elo順位 | 期待総合順位 | ずれ | 総合1位確率 | 総合上位8位確率 |",
            "| --- | ---: | ---: | ---: | ---: | ---: |"
        });

        lines.AddRange(topAdvantagedPlayers.Select(BuildTournamentQualityReportPlayerMarkdownRow));

        if (playerRows.Count > 0)
        {
            var chartRows = playerRows
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
                "    x-axis [" + MarkdownOutputHelpers.BuildMermaidCategoryList(chartRows.Select(row => row.Name)) + "]",
                "    y-axis \"期待総合順位\" 1 --> " + Math.Max(8, playerRows.Count).ToString(CultureInfo.InvariantCulture),
                "    bar [" + string.Join(", ", chartRows.Select(row => row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture))) + "]",
                "```",
                string.Empty,
                "```mermaid",
                "xychart-beta",
                "    title \"Elo上位8名の総合1位確率\"",
                "    x-axis [" + MarkdownOutputHelpers.BuildMermaidCategoryList(chartRows.Select(row => row.Name)) + "]",
                "    y-axis \"総合1位確率(%)\" 0 --> 100",
                "    bar [" + string.Join(", ", chartRows.Select(row => (row.OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                "```"
            });
        }

        lines.AddRange(BuildAdjustmentCycleAdviceLines(
            timedOut: calculationMode.Contains("時間切れ", StringComparison.Ordinal),
            zeroResults: playerRows.Count == 0,
            completedCount: playerRows.Count,
            subjectLabel: "品質評価"));
        lines.AddRange(BuildNextCycleSuggestionMarkdownLines(suggestion));

        return lines;
    }

    internal static IEnumerable<string> CreateTournamentQualitySweepReportCsv(IReadOnlyList<TournamentQualitySweepReportRow> sweepRows, TournamentQualityEvaluationReportGroupingOptions options, TournamentQualityNextCycleSuggestion suggestion)
    {
        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(new[] { "firstPlayerWinRatePercent", "spearmanCorrelation", "meanAbsoluteRankError", "averageTop8Retention", "eloTop1OverallTop1ProbabilityPercent", "mostPenalizedPlayer", "mostPenalizedDelta", "mostAdvantagedPlayer", "mostAdvantagedDelta" }).Select(EscapeCsv))
        };

        lines.AddRange(sweepRows.Select(row => string.Join(",", CsvSchemaCommonColumns.BuildRowColumns(
            "TournamentQualityReport",
            "sweepReport",
            "data",
            row.FirstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
            row.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture),
            row.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture),
            row.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture),
            (row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture),
            row.MostPenalizedPlayerName,
            row.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture),
            row.MostAdvantagedPlayerName,
            row.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture)).Select(EscapeCsv))));

        if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
        {
            lines.Add(string.Join(",", CsvSchemaCommonColumns.BuildRowColumns("TournamentQualityReport", "sweepReport", "meta", "evaluationMemo", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, options.EvaluationMemo, string.Empty).Select(EscapeCsv)));
        }

        lines.AddRange(BuildNextCycleSuggestionCsvLines(suggestion));
        return lines;
    }

    internal static IEnumerable<string> CreateTournamentQualityReportPlayerCsv(IReadOnlyList<TournamentQualityReportPlayerRow> playerRows)
    {
        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(new[] { "playerName", "group", "originalElo", "eloRank", "expectedOverallPlace", "overallPlaceDeltaFromEloRank", "overallTop1ProbabilityPercent", "overallTop8ProbabilityPercent" }).Select(EscapeCsv))
        };

        lines.AddRange(playerRows.Select(row => string.Join(",", CsvSchemaCommonColumns.BuildRowColumns(
            "TournamentQualityReport",
            "playerReport",
            "data",
            row.Name,
            row.Group,
            row.OriginalRating.ToString(CultureInfo.InvariantCulture),
            row.EloRank.ToString(CultureInfo.InvariantCulture),
            row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture),
            row.OverallPlaceDeltaFromEloRank.ToString("F3", CultureInfo.InvariantCulture),
            (row.OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture),
            (row.OverallTop8Probability * 100).ToString("F2", CultureInfo.InvariantCulture)).Select(EscapeCsv))));

        return lines;
    }

    static string BuildRecommendedSweepBandText(IReadOnlyList<TournamentQualitySweepReportRow> recommendedRows, double fallbackPercent)
    {
        if (recommendedRows.Count == 0) return fallbackPercent.ToString("F2", CultureInfo.InvariantCulture) + "% 付近";

        var start = recommendedRows[0].FirstPlayerWinRatePercent;
        var end = recommendedRows[^1].FirstPlayerWinRatePercent;
        return Math.Abs(start - end) < 0.000001
            ? start.ToString("F2", CultureInfo.InvariantCulture) + "% 付近"
            : start.ToString("F2", CultureInfo.InvariantCulture) + "%〜" + end.ToString("F2", CultureInfo.InvariantCulture) + "%";
    }

    static string BuildTournamentQualityReportPlayerMarkdownRow(TournamentQualityReportPlayerRow row)
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

    static string FormatSignedDelta(double value)
    {
        return value >= 0
            ? "+" + value.ToString("F6", CultureInfo.InvariantCulture)
            : value.ToString("F6", CultureInfo.InvariantCulture);
    }

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
}
