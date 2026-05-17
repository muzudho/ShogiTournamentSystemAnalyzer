internal static partial class Program
{
    static List<ResultRow> BuildResultRows(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, CalculationResult result, double blackAdvantagePercent)
    {
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
        var blackCounts = new int[players.Count];
        var whiteCounts = new int[players.Count];
        var blackWinProbabilitySums = new double[players.Count];
        var whiteWinProbabilitySums = new double[players.Count];
        var totalWinProbabilitySums = new double[players.Count];
        var opponentRatings = Enumerable.Range(0, players.Count)
            .Select(_ => new List<double>())
            .ToArray();

        foreach (var match in matches)
        {
            var blackWinProbability = GetWinProbability(players[match.Black], players[match.White], blackAdvantageRating);
            blackCounts[match.Black]++;
            whiteCounts[match.White]++;
            blackWinProbabilitySums[match.Black] += blackWinProbability;
            whiteWinProbabilitySums[match.White] += 1.0 - blackWinProbability;
            totalWinProbabilitySums[match.Black] += blackWinProbability;
            totalWinProbabilitySums[match.White] += 1.0 - blackWinProbability;
            opponentRatings[match.Black].Add(players[match.White].Rating);
            opponentRatings[match.White].Add(players[match.Black].Rating);
        }

        var rows = new List<ResultRow>(players.Count);
        for (var playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            var expectedPlace = Enumerable.Range(0, players.Count)
                .Sum(place => (place + 1) * result.PlaceProbabilities[playerIndex, place]);
            var blackWinRate = blackCounts[playerIndex] == 0
                ? (double?)null
                : blackWinProbabilitySums[playerIndex] / blackCounts[playerIndex];
            var whiteWinRate = whiteCounts[playerIndex] == 0
                ? (double?)null
                : whiteWinProbabilitySums[playerIndex] / whiteCounts[playerIndex];
            var matchCount = blackCounts[playerIndex] + whiteCounts[playerIndex];
            var totalWinRate = matchCount == 0
                ? 0.0
                : totalWinProbabilitySums[playerIndex] / matchCount;
            var effectiveRating = CalculateEquivalentNeutralRating(opponentRatings[playerIndex], totalWinRate);
            var placeProbabilities = Enumerable.Range(0, players.Count)
                .Select(place => result.PlaceProbabilities[playerIndex, place])
                .ToArray();
            var placeCounts = result.SimulationCount.HasValue
                ? placeProbabilities.Select(value => value * result.SimulationCount.Value).ToArray()
                : null;

            rows.Add(new ResultRow(
                players[playerIndex].Name,
                players[playerIndex].Rating,
                effectiveRating,
                effectiveRating - players[playerIndex].Rating,
                blackCounts[playerIndex],
                whiteCounts[playerIndex],
                blackWinRate,
                whiteWinRate,
                result.PlaceProbabilities[playerIndex, 0],
                expectedPlace,
                placeProbabilities,
                placeCounts));
        }

        return rows;
    }

    static List<FinalStageResultRow> BuildFinalStageResultRows(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, CalculationResult result, double blackAdvantagePercent, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount)
    {
        var standardRows = BuildResultRows(players, matches, result, blackAdvantagePercent);
        var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
        var innovCount = players.Count - apexCount;

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

    static List<QualityPlayerRow> BuildQualityPlayerRows(
        IReadOnlyList<ResultRow> resultRows,
        IReadOnlyDictionary<string, FinalStageGroup>? groupMap,
        IReadOnlyList<Player> additionalApexPlayers,
        AdditionalApexPlacementMode placementMode,
        QualityInnovExpectedRankOffsetMode innovExpectedRankOffsetMode,
        int innovExpectedRankOffsetCount)
    {
        var allPlayers = resultRows
            .Select(row => new Player(row.Name, row.OriginalRating))
            .Concat(placementMode == AdditionalApexPlacementMode.Off ? additionalApexPlayers : Enumerable.Empty<Player>())
            .ToList();

        var eloRanks = allPlayers
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select((player, index) => new { player.Name, Rank = index + 1 })
            .ToDictionary(x => x.Name, x => x.Rank, StringComparer.OrdinalIgnoreCase);

        return resultRows
            .Select(row =>
            {
                var eloRank = eloRanks[row.Name];
                var comparisonEloRank = groupMap is not null
                    && groupMap[row.Name] == FinalStageGroup.Innov
                    && innovExpectedRankOffsetMode == QualityInnovExpectedRankOffsetMode.On
                    ? eloRank + innovExpectedRankOffsetCount
                    : eloRank;
                var overallTop8Probability = row.PlaceProbabilities.Take(Math.Min(8, row.PlaceProbabilities.Length)).Sum();
                return new QualityPlayerRow(
                    row.Name,
                    groupMap is null ? "Neutral" : groupMap[row.Name].ToString(),
                    row.OriginalRating,
                    eloRank,
                    row.AveragePlace,
                    row.AveragePlace - comparisonEloRank,
                    row.ChampionshipProbability,
                    overallTop8Probability);
            })
            .OrderBy(x => x.EloRank)
            .ToList();
    }

    static QualitySummary BuildQualitySummary(IReadOnlyList<QualityPlayerRow> playerRows)
    {
        var spearmanCorrelation = CalculateSpearmanCorrelation(playerRows);
        var meanAbsoluteRankError = playerRows.Average(x => Math.Abs(x.OverallPlaceDeltaFromEloRank));
        var averageTop8Retention = playerRows
            .Where(x => x.EloRank <= 8)
            .Sum(x => x.OverallTop8Probability);

        var topEloPlayer = playerRows.OrderBy(x => x.EloRank).First();
        var mostPenalizedPlayer = playerRows.OrderByDescending(x => x.OverallPlaceDeltaFromEloRank).First();
        var mostAdvantagedPlayer = playerRows.OrderBy(x => x.OverallPlaceDeltaFromEloRank).First();

        return new QualitySummary(
            spearmanCorrelation,
            meanAbsoluteRankError,
            averageTop8Retention,
            topEloPlayer.OverallTop1Probability,
            mostPenalizedPlayer.Name,
            mostPenalizedPlayer.OverallPlaceDeltaFromEloRank,
            mostAdvantagedPlayer.Name,
            mostAdvantagedPlayer.OverallPlaceDeltaFromEloRank);
    }

    static double CalculateSpearmanCorrelation(IReadOnlyList<QualityPlayerRow> playerRows)
    {
        if (playerRows.Count <= 1)
        {
            return 1.0;
        }

        var eloRanks = playerRows
            .OrderBy(x => x.EloRank)
            .Select(x => (double)x.EloRank)
            .ToArray();
        var overallPlaceRanks = GetAverageRanks(playerRows.Select(x => x.ExpectedOverallPlace).ToArray());

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

