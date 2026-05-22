/*
 * ［大会品質評価域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static class TournamentQualityEvaluationReportBuilder
{
    internal static List<TournamentQualityReportPlayerRow> BuildTournamentQualityReportPlayerRows(
        IReadOnlyList<ResultRow> resultRows,
        IReadOnlyDictionary<string, FinalStageGroup>? groupMap,
        IReadOnlyList<Player> additionalApexPlayers,
        AdditionalApexPlacementMode placementMode,
        TournamentQualityEvaluationInnovExpectedRankOffsetMode innovExpectedRankOffsetMode,
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
                    && innovExpectedRankOffsetMode == TournamentQualityEvaluationInnovExpectedRankOffsetMode.On
                    ? eloRank + innovExpectedRankOffsetCount
                    : eloRank;
                var overallTop8Probability = row.PlaceProbabilities.Take(Math.Min(8, row.PlaceProbabilities.Length)).Sum();
                return new TournamentQualityReportPlayerRow(
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

    internal static TournamentQualityReportSummary BuildTournamentQualityReportSummary(IReadOnlyList<TournamentQualityReportPlayerRow> playerRows)
    {
        var spearmanCorrelation = CalculateTournamentQualityReportSpearmanCorrelation(playerRows);
        var meanAbsoluteRankError = playerRows.Average(x => Math.Abs(x.OverallPlaceDeltaFromEloRank));
        var averageTop8Retention = playerRows
            .Where(x => x.EloRank <= 8)
            .Sum(x => x.OverallTop8Probability);

        var topEloPlayer = playerRows.OrderBy(x => x.EloRank).First();
        var mostPenalizedPlayer = playerRows.OrderByDescending(x => x.OverallPlaceDeltaFromEloRank).First();
        var mostAdvantagedPlayer = playerRows.OrderBy(x => x.OverallPlaceDeltaFromEloRank).First();

        return new TournamentQualityReportSummary(
            spearmanCorrelation,
            meanAbsoluteRankError,
            averageTop8Retention,
            topEloPlayer.OverallTop1Probability,
            mostPenalizedPlayer.Name,
            mostPenalizedPlayer.OverallPlaceDeltaFromEloRank,
            mostAdvantagedPlayer.Name,
            mostAdvantagedPlayer.OverallPlaceDeltaFromEloRank);
    }

    static double CalculateTournamentQualityReportSpearmanCorrelation(IReadOnlyList<TournamentQualityReportPlayerRow> playerRows)
    {
        if (playerRows.Count <= 1) return 1.0;

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

        if (varianceX <= 0.0 || varianceY <= 0.0) return 1.0;

        return covariance / Math.Sqrt(varianceX * varianceY);
    }
}
