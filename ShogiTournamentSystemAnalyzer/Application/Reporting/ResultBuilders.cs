internal static partial class Program
{
    static List<ResultRow> BuildResultRows(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, CalculationResult result, double blackAdvantagePercent)
    {
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
        var blackCounts = new int[participants.Count];
        var whiteCounts = new int[participants.Count];
        var blackWinProbabilitySums = new double[participants.Count];
        var whiteWinProbabilitySums = new double[participants.Count];
        var totalWinProbabilitySums = new double[participants.Count];
        var opponentRatings = Enumerable.Range(0, participants.Count)
            .Select(_ => new List<double>())
            .ToArray();

        foreach (var match in matches)
        {
            var blackWinProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);
            blackCounts[match.Black]++;
            whiteCounts[match.White]++;
            blackWinProbabilitySums[match.Black] += blackWinProbability;
            whiteWinProbabilitySums[match.White] += 1.0 - blackWinProbability;
            totalWinProbabilitySums[match.Black] += blackWinProbability;
            totalWinProbabilitySums[match.White] += 1.0 - blackWinProbability;
            opponentRatings[match.Black].Add(participants[match.White].Rating);
            opponentRatings[match.White].Add(participants[match.Black].Rating);
        }

        var rows = new List<ResultRow>(participants.Count);
        for (var participantIndex = 0; participantIndex < participants.Count; participantIndex++)
        {
            var expectedPlace = Enumerable.Range(0, participants.Count)
                .Sum(place => (place + 1) * result.PlaceProbabilities[participantIndex, place]);
            var blackWinRate = blackCounts[participantIndex] == 0
                ? (double?)null
                : blackWinProbabilitySums[participantIndex] / blackCounts[participantIndex];
            var whiteWinRate = whiteCounts[participantIndex] == 0
                ? (double?)null
                : whiteWinProbabilitySums[participantIndex] / whiteCounts[participantIndex];
            var matchCount = blackCounts[participantIndex] + whiteCounts[participantIndex];
            var totalWinRate = matchCount == 0
                ? 0.0
                : totalWinProbabilitySums[participantIndex] / matchCount;
            var effectiveRating = CalculateEquivalentNeutralRating(opponentRatings[participantIndex], totalWinRate);
            var placeProbabilities = Enumerable.Range(0, participants.Count)
                .Select(place => result.PlaceProbabilities[participantIndex, place])
                .ToArray();
            var placeCounts = result.SimulationCount.HasValue
                ? placeProbabilities.Select(value => value * result.SimulationCount.Value).ToArray()
                : null;

            rows.Add(new ResultRow(
                participants[participantIndex].Name,
                participants[participantIndex].Rating,
                effectiveRating,
                effectiveRating - participants[participantIndex].Rating,
                blackCounts[participantIndex],
                whiteCounts[participantIndex],
                blackWinRate,
                whiteWinRate,
                result.PlaceProbabilities[participantIndex, 0],
                expectedPlace,
                placeProbabilities,
                placeCounts));
        }

        return rows;
    }

    static List<FinalStageResultRow> BuildFinalStageResultRows(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, CalculationResult result, double blackAdvantagePercent, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount)
    {
        var standardRows = BuildResultRows(participants, matches, result, blackAdvantagePercent);
        var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
        var innovCount = participants.Count - apexCount;

        return standardRows
            .Select(row =>
            {
                var group = groupMap[row.Name];
                var groupStartIndex = group == FinalStageGroup.Apex ? 0 : apexCount + additionalApexCount;
                var groupSize = group == FinalStageGroup.Apex ? apexCount : innovCount;
                var groupPlaceAverage = Enumerable.Range(0, groupSize)
                    .Sum(offset => (offset + 1) * row.PlaceProbabilities[groupStartIndex + offset]);

                return new FinalStageResultRow(
                    row.Name,
                    group.ToString(),
                    row.OriginalRating,
                    row.EffectiveRating,
                    row.RatingDelta,
                    row.BlackCount,
                    row.WhiteCount,
                    row.BlackWinRate,
                    row.WhiteWinRate,
                    row.PlaceProbabilities[groupStartIndex],
                    groupPlaceAverage,
                    row.PlaceProbabilities[0],
                    row.AveragePlace,
                    row.PlaceProbabilities,
                    row.PlaceCounts);
            })
            .ToList();
    }

    static List<QualityParticipantRow> BuildQualityParticipantRows(IReadOnlyList<ResultRow> resultRows, IReadOnlyDictionary<string, FinalStageGroup>? groupMap, IReadOnlyList<Participant> additionalApexParticipants, AdditionalApexPlacementMode placementMode)
    {
        var allParticipants = resultRows
            .Select(row => new Participant(row.Name, row.OriginalRating))
            .Concat(placementMode == AdditionalApexPlacementMode.Off ? additionalApexParticipants : Enumerable.Empty<Participant>())
            .ToList();

        var eloRanks = allParticipants
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select((participant, index) => new { participant.Name, Rank = index + 1 })
            .ToDictionary(x => x.Name, x => x.Rank, StringComparer.OrdinalIgnoreCase);

        return resultRows
            .Select(row =>
            {
                var eloRank = eloRanks[row.Name];
                var overallTop8Probability = row.PlaceProbabilities.Take(Math.Min(8, row.PlaceProbabilities.Length)).Sum();
                return new QualityParticipantRow(
                    row.Name,
                    groupMap is null ? "Neutral" : groupMap[row.Name].ToString(),
                    row.OriginalRating,
                    eloRank,
                    row.AveragePlace,
                    row.AveragePlace - eloRank,
                    row.ChampionshipProbability,
                    overallTop8Probability);
            })
            .OrderBy(x => x.EloRank)
            .ToList();
    }

    static QualitySummary BuildQualitySummary(IReadOnlyList<QualityParticipantRow> participantRows)
    {
        var spearmanCorrelation = CalculateSpearmanCorrelation(participantRows);
        var meanAbsoluteRankError = participantRows.Average(x => Math.Abs(x.OverallPlaceDeltaFromEloRank));
        var averageTop8Retention = participantRows
            .Where(x => x.EloRank <= 8)
            .Sum(x => x.OverallTop8Probability);

        var topEloParticipant = participantRows.OrderBy(x => x.EloRank).First();
        var mostPenalizedParticipant = participantRows.OrderByDescending(x => x.OverallPlaceDeltaFromEloRank).First();
        var mostAdvantagedParticipant = participantRows.OrderBy(x => x.OverallPlaceDeltaFromEloRank).First();

        return new QualitySummary(
            spearmanCorrelation,
            meanAbsoluteRankError,
            averageTop8Retention,
            topEloParticipant.OverallTop1Probability,
            mostPenalizedParticipant.Name,
            mostPenalizedParticipant.OverallPlaceDeltaFromEloRank,
            mostAdvantagedParticipant.Name,
            mostAdvantagedParticipant.OverallPlaceDeltaFromEloRank);
    }

    static double CalculateSpearmanCorrelation(IReadOnlyList<QualityParticipantRow> participantRows)
    {
        if (participantRows.Count <= 1)
        {
            return 1.0;
        }

        var eloRanks = participantRows
            .OrderBy(x => x.EloRank)
            .Select(x => (double)x.EloRank)
            .ToArray();
        var overallPlaceRanks = GetAverageRanks(participantRows.Select(x => x.ExpectedOverallPlace).ToArray());

        return CalculatePearsonCorrelation(eloRanks, overallPlaceRanks);
    }

    static double[] GetAverageRanks(IReadOnlyList<double> values)
    {
        var ordered = values
            .Select((value, index) => new { Value = value, Index = index })
            .OrderBy(x => x.Value)
            .ToArray();

        var ranks = new double[values.Count];
        var current = 0;
        while (current < ordered.Length)
        {
            var end = current + 1;
            while (end < ordered.Length && ordered[end].Value.Equals(ordered[current].Value))
            {
                end++;
            }

            var averageRank = (current + 1 + end) / 2.0;
            for (var i = current; i < end; i++)
            {
                ranks[ordered[i].Index] = averageRank;
            }

            current = end;
        }

        return ranks;
    }

    static double CalculatePearsonCorrelation(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
    {
        var meanX = xs.Average();
        var meanY = ys.Average();
        var covariance = 0.0;
        var varianceX = 0.0;
        var varianceY = 0.0;

        for (var i = 0; i < xs.Count; i++)
        {
            var dx = xs[i] - meanX;
            var dy = ys[i] - meanY;
            covariance += dx * dy;
            varianceX += dx * dx;
            varianceY += dy * dy;
        }

        if (varianceX <= 0.0 || varianceY <= 0.0)
        {
            return 1.0;
        }

        return covariance / Math.Sqrt(varianceX * varianceY);
    }
}
