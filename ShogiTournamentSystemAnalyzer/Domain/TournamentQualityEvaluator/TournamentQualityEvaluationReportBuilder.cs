/*
 * ［大会品質評価域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class TournamentQualityEvaluationReportBuilder
{
    internal static List<TournamentQualityReportPlayerRow> BuildTournamentQualityReportPlayerRows(
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        IReadOnlyDictionary<string, FinalStageGroup>? groupMap,
        IReadOnlyList<Player> additionalApexPlayers,
        AdditionalApexPlacementMode placementMode,
        TournamentQualityEvaluationInnovExpectedRankOffsetMode innovExpectedRankOffsetMode,
        int innovExpectedRankOffsetCount)
    {
        var allPlayers = resultRows
            .Select(row => new Player(row.CommonData.Name, row.CommonData.OriginalRating))
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
                var commonData = row.CommonData;
                var averagePlace = GetMetric(row, "averagePlace");
                var championshipProbability = GetMetric(row, "championshipProbability");
                var eloRank = eloRanks[commonData.Name];
                var comparisonEloRank = groupMap is not null
                    && groupMap[commonData.Name] == FinalStageGroup.Innov
                    && innovExpectedRankOffsetMode == TournamentQualityEvaluationInnovExpectedRankOffsetMode.On
                    ? eloRank + innovExpectedRankOffsetCount
                    : eloRank;
                var overallTop8Probability = commonData.PlaceProbabilities.Take(Math.Min(8, commonData.PlaceProbabilities.Length)).Sum();
                return new TournamentQualityReportPlayerRow(
                    commonData.Name,
                    groupMap is null ? "Neutral" : groupMap[commonData.Name].ToString(),
                    commonData.OriginalRating,
                    eloRank,
                    averagePlace,
                    averagePlace - comparisonEloRank,
                    championshipProbability,
                    overallTop8Probability);
            })
            .OrderBy(x => x.EloRank)
            .ToList();
    }

    static double GetMetric(GeneralSimulationResultRow row, string key)
    {
        if (row.Metrics.TryGetValue(key, out var metric))
        {
            return metric.Value;
        }

        throw new InvalidOperationException($"品質評価レポート生成に必要な metric がありません: {key}");
    }

    internal static TournamentQualityReportSummary BuildTournamentQualityReportSummary(
        IReadOnlyList<TournamentQualityReportPlayerRow> playerRows,
        TournamentQualityScoreRule scoreRule,
        int simulationCount)
    {
        if (playerRows.Count == 0)
        {
            return BuildTournamentQualityReportSummary(
                1.0,
                0.0,
                0.0,
                0.0,
                "",
                0.0,
                "",
                0.0,
                scoreRule,
                simulationCount);
        }

        var spearmanCorrelation = CalculateTournamentQualityReportSpearmanCorrelation(playerRows);
        var meanAbsoluteRankError = playerRows.Average(x => Math.Abs(x.OverallPlaceDeltaFromEloRank));
        var averageTop8Retention = playerRows
            .Where(x => x.EloRank <= 8)
            .Sum(x => x.OverallTop8Probability);

        var topEloPlayer = playerRows.OrderBy(x => x.EloRank).First();
        var mostPenalizedPlayer = playerRows.OrderByDescending(x => x.OverallPlaceDeltaFromEloRank).First();
        var mostAdvantagedPlayer = playerRows.OrderBy(x => x.OverallPlaceDeltaFromEloRank).First();

        return BuildTournamentQualityReportSummary(
            spearmanCorrelation,
            meanAbsoluteRankError,
            averageTop8Retention,
            topEloPlayer.OverallTop1Probability,
            mostPenalizedPlayer.Name,
            mostPenalizedPlayer.OverallPlaceDeltaFromEloRank,
            mostAdvantagedPlayer.Name,
            mostAdvantagedPlayer.OverallPlaceDeltaFromEloRank,
            scoreRule,
            simulationCount);
    }

    static TournamentQualityReportSummary BuildTournamentQualityReportSummary(
        double spearmanCorrelation,
        double meanAbsoluteRankError,
        double averageTop8Retention,
        double eloTop1OverallTop1Probability,
        string mostPenalizedPlayerName,
        double mostPenalizedDelta,
        string mostAdvantagedPlayerName,
        double mostAdvantagedDelta,
        TournamentQualityScoreRule scoreRule,
        int simulationCount)
    {
        var summaryWithoutScore = new TournamentQualityReportSummary(
            spearmanCorrelation,
            meanAbsoluteRankError,
            averageTop8Retention,
            eloTop1OverallTop1Probability,
            mostPenalizedPlayerName,
            mostPenalizedDelta,
            mostAdvantagedPlayerName,
            mostAdvantagedDelta,
            default);
        var scoreBreakdown = TournamentQualityScoreCalculator.Calculate(summaryWithoutScore, scoreRule, simulationCount);
        return summaryWithoutScore with { ScoreBreakdown = scoreBreakdown };
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
