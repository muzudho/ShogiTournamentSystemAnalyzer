using System.Globalization;
using System.Text;

internal static partial class Program
{
    static void WriteQualitySummaryCsv(string outputCsvPath, QualitySummary summary, ExperimentalReportGroupingOptions options)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

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

        if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
        {
            lines.Add($"evaluationMemo,,{EscapeCsv(options.EvaluationMemo)}");
        }

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }

    static void WriteQualitySweepMarkdown(string outputMarkdownPath, IReadOnlyList<QualitySweepRow> sweepRows, string sweepCsvPath, ExperimentalReportGroupingOptions options)
    {
        var directoryPath = Path.GetDirectoryName(outputMarkdownPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var bestSpearmanRow = sweepRows
            .OrderByDescending(row => row.SpearmanCorrelation)
            .ThenBy(row => row.MeanAbsoluteRankError)
            .ThenBy(row => row.BlackAdvantagePercent)
            .FirstOrDefault();
        var bestMaeRow = sweepRows
            .OrderBy(row => row.MeanAbsoluteRankError)
            .ThenBy(row => row.BlackAdvantagePercent)
            .FirstOrDefault();
        var bestTop1Row = sweepRows
            .OrderByDescending(row => row.EloTop1OverallTop1Probability)
            .ThenBy(row => row.BlackAdvantagePercent)
            .FirstOrDefault();

        var lines = new List<string>
        {
            "# n% スイープ結果レポート",
            string.Empty,
            "## 概要",
            $"- 評価点数: {sweepRows.Count}",
            $"- 出力CSV: `{sweepCsvPath}`"
        };

        if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
        {
            lines.Add($"- 評価メモ: {options.EvaluationMemo}");
        }

        if (sweepRows.Count > 0)
        {
            lines.AddRange(new[]
            {
                string.Empty,
                "## 注目ポイント",
                $"- Spearman 相関が最良の点: **{bestSpearmanRow.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%**（{bestSpearmanRow.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture)}）",
                $"- 平均順位ずれが最良の点: **{bestMaeRow.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%**（{bestMaeRow.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture)}）",
                $"- Elo1位の総合1位確率が最良の点: **{bestTop1Row.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%**（{(bestTop1Row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture)}%）",
                string.Empty,
                "## 一覧表",
                "| 先手勝率 | Spearman 相関 | 平均順位ずれ | Elo上位8名残留 | Elo1位の総合1位確率 | 最大不利益 | 最大利益 |",
                "| ---: | ---: | ---: | ---: | ---: | --- | --- |"
            });

            lines.AddRange(sweepRows.Select(row =>
                $"| {row.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}% | {row.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture)} | {row.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture)} | {row.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture)} | {(row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture)}% | {row.MostPenalizedPlayerName} ({FormatSignedDelta(row.MostPenalizedDelta)}) | {row.MostAdvantagedPlayerName} ({FormatSignedDelta(row.MostAdvantagedDelta)}) |"));

            lines.AddRange(new[]
            {
                string.Empty,
                "## 推移図",
                "```mermaid",
                "xychart-beta",
                "    title \"n%スイープの主要指標\"",
                "    x-axis \"先手勝率(%)\" [" + string.Join(", ", sweepRows.Select(row => row.BlackAdvantagePercent.ToString("F0", CultureInfo.InvariantCulture))) + "]",
                "    y-axis \"値\" 0 --> 100",
                "    line \"Elo1位の総合1位確率(%)\" [" + string.Join(", ", sweepRows.Select(row => (row.EloTop1OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                "    line \"Elo上位8名残留\" [" + string.Join(", ", sweepRows.Select(row => row.AverageTop8Retention.ToString("F2", CultureInfo.InvariantCulture))) + "]",
                "```"
            });
        }

        File.WriteAllLines(outputMarkdownPath, lines, new UTF8Encoding(false));
    }

    static void WriteQualitySummaryMarkdown(
        string outputMarkdownPath,
        QualityEvaluationRun qualityEvaluationRun,
        string summaryCsvPath,
        string playerCsvPath,
        ExperimentalReportGroupingOptions options)
    {
        var directoryPath = Path.GetDirectoryName(outputMarkdownPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

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

        var lines = new List<string>
        {
            "# 品質評価サマリーレポート",
            string.Empty,
            "## 概要",
            $"- 計算モード: {qualityEvaluationRun.CalculationMode}",
            $"- 対象選手数: {qualityEvaluationRun.PlayerRows.Count}",
            $"- サマリーCSV: `{summaryCsvPath}`",
            $"- 選手別CSV: `{playerCsvPath}`"
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

        File.WriteAllLines(outputMarkdownPath, lines, new UTF8Encoding(false));
    }

    static void WriteQualitySweepCsv(string outputCsvPath, IReadOnlyList<QualitySweepRow> sweepRows, ExperimentalReportGroupingOptions options)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var lines = new List<string>
        {
            "blackAdvantagePercent,spearmanCorrelation,meanAbsoluteRankError,averageTop8Retention,eloTop1OverallTop1ProbabilityPercent,mostPenalizedPlayer,mostPenalizedDelta,mostAdvantagedPlayer,mostAdvantagedDelta"
        };

        lines.AddRange(sweepRows.Select(row => string.Join(",",
            row.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture),
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

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }

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

    static string FormatSignedDelta(double value)
    {
        return value >= 0
            ? "+" + value.ToString("F6", CultureInfo.InvariantCulture)
            : value.ToString("F6", CultureInfo.InvariantCulture);
    }

    static void WriteQualityPlayerCsv(string outputCsvPath, IReadOnlyList<QualityPlayerRow> playerRows)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

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

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }

    static void WriteResultCsv(string outputCsvPath, string mode, double blackAdvantagePercent, IReadOnlyList<ResultRow> resultRows)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var lines = new List<string>();
        var headerColumns = new List<string>
        {
            "calculationMode",
            "blackAdvantagePercent",
            "playerName",
            "originalElo",
            "effectiveElo",
            "eloDelta",
            "blackCount",
            "whiteCount",
            "blackWinRatePercent",
            "whiteWinRatePercent",
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
                blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture),
                row.Name,
                FormatRating(row.OriginalRating),
                FormatRating(row.EffectiveRating),
                FormatSignedRating(row.RatingDelta),
                row.BlackCount.ToString(CultureInfo.InvariantCulture),
                row.WhiteCount.ToString(CultureInfo.InvariantCulture),
                FormatOptionalPercentValue(row.BlackWinRate),
                FormatOptionalPercentValue(row.WhiteWinRate),
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

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }

    static void WriteResultMarkdown(string outputMarkdownPath, string mode, double blackAdvantagePercent, IReadOnlyList<ResultRow> resultRows)
    {
        var directoryPath = Path.GetDirectoryName(outputMarkdownPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var topChampionshipRows = resultRows
            .OrderByDescending(row => row.ChampionshipProbability)
            .ThenBy(row => row.AveragePlace)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();

        var lines = new List<string>
        {
            "# 通常モード結果レポート",
            string.Empty,
            "## 概要",
            $"- 計算モード: {mode}",
            $"- 同Elo対局時の先手勝率: {blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%",
            $"- 対象選手数: {resultRows.Count}",
            string.Empty,
            "## 上位候補一覧",
            "| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |",
            "| --- | ---: | ---: | ---: | ---: | ---: |"
        };

        lines.AddRange(topChampionshipRows.Select(row =>
            $"| {row.Name} | {FormatRating(row.OriginalRating)} | {FormatRating(row.EffectiveRating)} | {FormatSignedRating(row.RatingDelta)} | {(row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} |"));

        File.WriteAllLines(outputMarkdownPath, lines, new UTF8Encoding(false));
    }

    static void WriteFinalStageResultCsv(string outputCsvPath, string mode, double blackAdvantagePercent, IReadOnlyList<FinalStageResultRow> resultRows)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var lines = new List<string>();
        var headerColumns = new List<string>
        {
            "calculationMode",
            "blackAdvantagePercent",
            "playerName",
            "group",
            "originalElo",
            "effectiveElo",
            "eloDelta",
            "blackCount",
            "whiteCount",
            "blackWinRatePercent",
            "whiteWinRatePercent",
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
                blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture),
                row.Name,
                row.Group,
                FormatRating(row.OriginalRating),
                FormatRating(row.EffectiveRating),
                FormatSignedRating(row.RatingDelta),
                row.BlackCount.ToString(CultureInfo.InvariantCulture),
                row.WhiteCount.ToString(CultureInfo.InvariantCulture),
                FormatOptionalPercentValue(row.BlackWinRate),
                FormatOptionalPercentValue(row.WhiteWinRate),
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

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }

    static void WriteFinalStageResultMarkdown(string outputMarkdownPath, string mode, double blackAdvantagePercent, IReadOnlyList<FinalStageResultRow> resultRows)
    {
        var directoryPath = Path.GetDirectoryName(outputMarkdownPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var topRows = resultRows
            .OrderByDescending(row => row.OverallPlace1Probability)
            .ThenBy(row => row.OverallPlaceAverage)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();

        var lines = new List<string>
        {
            "# 本戦モード結果レポート",
            string.Empty,
            "## 概要",
            $"- 計算モード: {mode}",
            $"- 同Elo対局時の先手勝率: {blackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture)}%",
            $"- 対象選手数: {resultRows.Count}",
            string.Empty,
            "## 上位候補一覧",
            "| 選手 | グループ | 元Elo | 実効Elo | 差分 | グループ1位確率 | 総合1位確率 | 総合平均順位 |",
            "| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |"
        };

        lines.AddRange(topRows.Select(row =>
            $"| {row.Name} | {row.Group} | {FormatRating(row.OriginalRating)} | {FormatRating(row.EffectiveRating)} | {FormatSignedRating(row.RatingDelta)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));

        File.WriteAllLines(outputMarkdownPath, lines, new UTF8Encoding(false));
    }
}

