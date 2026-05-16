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
            $"mostPenalizedParticipantDelta,{summary.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture)},{EscapeCsv(summary.MostPenalizedParticipantName)}",
            $"mostAdvantagedParticipantDelta,{summary.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture)},{EscapeCsv(summary.MostAdvantagedParticipantName)}"
        };

        if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
        {
            lines.Add($"evaluationMemo,,{EscapeCsv(options.EvaluationMemo)}");
        }

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
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
            "blackAdvantagePercent,spearmanCorrelation,meanAbsoluteRankError,averageTop8Retention,eloTop1OverallTop1ProbabilityPercent,mostPenalizedParticipant,mostPenalizedDelta,mostAdvantagedParticipant,mostAdvantagedDelta"
        };

        lines.AddRange(sweepRows.Select(row => string.Join(",",
            row.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture),
            row.SpearmanCorrelation.ToString("F6", CultureInfo.InvariantCulture),
            row.MeanAbsoluteRankError.ToString("F6", CultureInfo.InvariantCulture),
            row.AverageTop8Retention.ToString("F6", CultureInfo.InvariantCulture),
            (row.EloTop1OverallTop1Probability * 100).ToString("F6", CultureInfo.InvariantCulture),
            EscapeCsv(row.MostPenalizedParticipantName),
            row.MostPenalizedDelta.ToString("F6", CultureInfo.InvariantCulture),
            EscapeCsv(row.MostAdvantagedParticipantName),
            row.MostAdvantagedDelta.ToString("F6", CultureInfo.InvariantCulture))));

        if (!string.IsNullOrWhiteSpace(options.EvaluationMemo))
        {
            lines.Add($"evaluationMemo,,,,,,,{EscapeCsv(options.EvaluationMemo)},");
        }

        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }

    static void WriteQualityParticipantCsv(string outputCsvPath, IReadOnlyList<QualityParticipantRow> participantRows)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var lines = new List<string>
        {
            "participantName,group,originalElo,eloRank,expectedOverallPlace,overallPlaceDeltaFromEloRank,overallTop1ProbabilityPercent,overallTop8ProbabilityPercent"
        };

        lines.AddRange(participantRows.Select(row => string.Join(",",
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
            "participantName",
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
            "participantName",
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
}
