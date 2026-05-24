/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System.Globalization;

internal static class FinalRankingDataFileWriter
{
    static string EscapeCsv(string value) => CsvOutputHelpers.EscapeCsv(value);

    internal static IEnumerable<string> CreateRepresentativeExecutionRankCsv(
        TournamentRuleSetMode tournamentRuleSetMode,
        IReadOnlyList<RepresentativeExecutionRankRow> rows,
        string? overviewNote = null)
    {
        var headerColumns = new List<string>
        {
            "tournamentRuleSetMode",
            "playerName",
            "points",
            "rankBand",
            "averagePlace",
            "firstPlaceProbabilityPercent"
        };

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            headerColumns.Add("note");
        }

        var lines = new List<string>
        {
            string.Join(",", headerColumns.Select(EscapeCsv))
        };

        foreach (var row in rows)
        {
            var columns = new List<string>
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
                columns.Add(overviewNote);
            }

            lines.Add(string.Join(",", columns.Select(EscapeCsv)));
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

    internal static IEnumerable<string> CreateResultCsv(string mode, double firstPlayerWinRatePercent, IReadOnlyList<ResultRow> resultRows, string? overviewNote = null)
    {
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

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            headerColumns.Add("note");
        }

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

        var lines = new List<string>
        {
            string.Join(",", headerColumns.Select(EscapeCsv))
        };

        foreach (var row in resultRows)
        {
            var columns = new List<string>
            {
                mode,
                firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
                row.Name,
                SimulationRatingMath.FormatRating(row.OriginalRating),
                SimulationRatingMath.FormatRating(row.EffectiveRating),
                SimulationRatingMath.FormatSignedRating(row.RatingDelta),
                row.FirstPlayerCount.ToString(CultureInfo.InvariantCulture),
                row.SecondPlayerCount.ToString(CultureInfo.InvariantCulture),
                SimulationRatingMath.FormatOptionalPercentValue(row.FirstPlayerWinRate),
                SimulationRatingMath.FormatOptionalPercentValue(row.SecondPlayerWinRate),
                (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture),
                row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)
            };

            if (!string.IsNullOrWhiteSpace(overviewNote))
            {
                columns.Add(overviewNote);
            }

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
    }

    internal static IEnumerable<string> CreateResultMarkdown(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<ResultRow> resultRows,
        string? overviewNote = null,
        string? representativeRankingMarkdownPath = null)
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
        var bestChampionshipRowName = resultRows.Count > 0 ? bestChampionshipRow.Name : "該当なし";
        var bestAveragePlaceRowName = resultRows.Count > 0 ? bestAveragePlaceRow.Name : "該当なし";
        var biggestBoostRowName = resultRows.Count > 0 ? biggestBoostRow.Name : "該当なし";
        var biggestDropRowName = resultRows.Count > 0 ? biggestDropRow.Name : "該当なし";
        var bestChampionshipProbability = resultRows.Count > 0 ? bestChampionshipRow.ChampionshipProbability : 0;
        var bestAveragePlace = resultRows.Count > 0 ? bestAveragePlaceRow.AveragePlace : 0;
        var biggestBoost = resultRows.Count > 0 ? biggestBoostRow.RatingDelta : 0;
        var biggestDrop = resultRows.Count > 0 ? biggestDropRow.RatingDelta : 0;

        var lines = new List<string>
        {
            "# 通常モード結果レポート",
            string.Empty,
            "## 概要",
            $"- 結果CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 計算モード: {mode}",
            $"- 同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%",
            $"- 対象選手数: {resultRows.Count}",
            string.Empty,
            "## 注目ポイント",
            $"- 優勝確率が最も高い選手: **{bestChampionshipRowName}**（{(bestChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
            $"- 平均順位が最も良い選手: **{bestAveragePlaceRowName}**（{bestAveragePlace.ToString("F3", CultureInfo.InvariantCulture)}）",
            $"- 実効Elo差分が最も大きくプラスの選手: **{biggestBoostRowName}**（{SimulationRatingMath.FormatSignedRating(biggestBoost)}）",
            $"- 実効Elo差分が最も大きくマイナスの選手: **{biggestDropRowName}**（{SimulationRatingMath.FormatSignedRating(biggestDrop)}）",
            string.Empty,
            "## 自動コメント",
            $"- 優勝候補の強さ: {BuildTop1Comment(bestChampionshipProbability)}",
            $"- 先頭の平均順位: {BuildAveragePlaceComment(bestAveragePlace)}",
            $"- 実効Eloの押し上げ: {BuildRatingDeltaComment(biggestBoost, biggestDrop)}",
            string.Empty,
            "## 上位候補一覧",
            "| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |",
            "| --- | ---: | ---: | ---: | ---: | ---: |"
        };

        if (!string.IsNullOrWhiteSpace(representativeRankingMarkdownPath))
        {
            lines.Insert(7, $"- representative順位表: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, representativeRankingMarkdownPath)}");
        }

        if (!string.IsNullOrWhiteSpace(overviewNote))
        {
            lines.Insert(7, $"- 注記: {overviewNote}");
        }

        lines.AddRange(topChampionshipRows.Select(row =>
            $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture)} |"));

        if (topChampionshipRows.Length > 0)
        {
            lines.AddRange(new[]
            {
                string.Empty,
                "## Mermaid 図",
                "```mermaid",
                "xychart-beta",
                "    title \"上位候補の優勝確率\"",
                "    x-axis [" + MarkdownOutputHelpers.BuildMermaidCategoryList(topChampionshipRows.Select(row => row.Name)) + "]",
                "    y-axis \"優勝確率(%)\" 0 --> 100",
                "    bar [" + string.Join(", ", topChampionshipRows.Select(row => (row.ChampionshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                "```",
                string.Empty,
                "```mermaid",
                "xychart-beta",
                "    title \"上位候補の平均順位\"",
                "    x-axis [" + MarkdownOutputHelpers.BuildMermaidCategoryList(topChampionshipRows.Select(row => row.Name)) + "]",
                "    y-axis \"平均順位\" 1 --> " + Math.Max(2, resultRows.Count).ToString(CultureInfo.InvariantCulture),
                "    bar [" + string.Join(", ", topChampionshipRows.Select(row => row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture))) + "]",
                "```"
            });
        }

        return lines;
    }

    internal static IEnumerable<string> CreateFinalStageResultCsv(string outputCsvPath, string mode, double firstPlayerWinRatePercent, IReadOnlyList<FinalStageResultRow> resultRows)
    {
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

        var lines = new List<string>
        {
            string.Join(",", headerColumns.Select(EscapeCsv))
        };

        foreach (var row in resultRows)
        {
            var columns = new List<string>
            {
                mode,
                firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
                row.Name,
                row.Group,
                SimulationRatingMath.FormatRating(row.OriginalRating),
                SimulationRatingMath.FormatRating(row.EffectiveRating),
                SimulationRatingMath.FormatSignedRating(row.RatingDelta),
                row.FirstPlayerCount.ToString(CultureInfo.InvariantCulture),
                row.SecondPlayerCount.ToString(CultureInfo.InvariantCulture),
                SimulationRatingMath.FormatOptionalPercentValue(row.FirstPlayerWinRate),
                SimulationRatingMath.FormatOptionalPercentValue(row.SecondPlayerWinRate),
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
    }

    internal static IEnumerable<string> CreateFinalStageResultMarkdown(string outputMarkdownPath, string outputCsvPath, string mode, double firstPlayerWinRatePercent, IReadOnlyList<FinalStageResultRow> resultRows, string? referenceMatchesCsvPath = null)
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

        var lines = new List<string>
        {
            "# 本戦モード結果レポート",
            string.Empty,
            "## 概要",
            $"- 結果CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath)}",
            $"- 計算モード: {mode}",
            $"- 同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%",
            $"- 対象選手数: {resultRows.Count}",
            string.Empty,
            "## 注目ポイント",
            $"- 総合1位確率が最も高い選手: **{bestOverallRowName}**（{(bestOverallProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
            $"- Apex で最も有力な選手: **{bestApexRowName}**（グループ1位確率 {(bestApexProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
            $"- Innov で最も有力な選手: **{bestInnovRowName}**（グループ1位確率 {(bestInnovProbability * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
            string.Empty,
            "## 自動コメント",
            $"- 総合1位候補の強さ: {BuildTop1Comment(bestOverallProbability)}",
            $"- Apex の先頭感: {BuildGroupLeadComment(bestApexProbability, bestApexAverage)}",
            $"- Innov の先頭感: {BuildGroupLeadComment(bestInnovProbability, bestInnovAverage)}",
            $"- Apex / Innov の先頭差: {BuildApexInnovGapComment(bestApexProbability, bestInnovProbability)}",
            string.Empty,
            "## 上位候補一覧",
            "| 選手 | グループ | 元Elo | 実効Elo | 差分 | グループ1位確率 | 総合1位確率 | 総合平均順位 |",
            "| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |"
        };

        if (!string.IsNullOrWhiteSpace(referenceMatchesCsvPath))
        {
            lines.Insert(4, $"- 参考対局CSV: {MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, referenceMatchesCsvPath)}");
        }

        lines.AddRange(topRows.Select(row =>
            $"| {row.Name} | {row.Group} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.RatingDelta)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));

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
                $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
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
                $"| {row.Name} | {SimulationRatingMath.FormatRating(row.OriginalRating)} | {SimulationRatingMath.FormatRating(row.EffectiveRating)} | {(row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} | {row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture)} |"));
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
                "    x-axis [" + MarkdownOutputHelpers.BuildMermaidCategoryList(topRows.Select(row => row.Name)) + "]",
                "    y-axis \"総合1位確率(%)\" 0 --> 100",
                "    bar [" + string.Join(", ", topRows.Select(row => (row.OverallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                "```",
                string.Empty,
                "```mermaid",
                "xychart-beta",
                "    title \"上位候補のグループ1位確率\"",
                "    x-axis [" + MarkdownOutputHelpers.BuildMermaidCategoryList(topRows.Select(row => row.Name)) + "]",
                "    y-axis \"グループ1位確率(%)\" 0 --> 100",
                "    bar [" + string.Join(", ", topRows.Select(row => (row.GroupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture))) + "]",
                "```"
            });
        }

        return lines;
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

    static string BuildGroupLeadComment(double groupPlace1Probability, double groupPlaceAverage)
    {
        var percent = groupPlace1Probability * 100;
        if (percent >= 35.0 && groupPlaceAverage <= 2.0) return "先頭がかなりはっきりしています。";
        if (percent >= 20.0 && groupPlaceAverage <= 3.0) return "先頭候補が見えています。";
        return "まだ横並び気味です。";
    }

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
}
