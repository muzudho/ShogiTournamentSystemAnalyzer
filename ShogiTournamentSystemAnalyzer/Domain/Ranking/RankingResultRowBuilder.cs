/*
 * ［順位付け域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.Ranking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using System.Globalization;

internal static class RankingResultRowBuilder
{
    internal static List<GeneralSimulationResultRow> BuildGeneralResultRows(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, CalculationResult result, double firstPlayerWinRatePercent)
    {
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        var firstPlayerCounts = new int[players.Count];
        var secondPlayerCounts = new int[players.Count];
        var firstPlayerWinProbabilitySums = new double[players.Count];
        var secondPlayerWinProbabilitySums = new double[players.Count];
        var totalWinProbabilitySums = new double[players.Count];
        var opponentRatings = Enumerable.Range(0, players.Count)
            .Select(_ => new List<double>())
            .ToArray();

        foreach (var match in matches)
        {
            var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(players[match.FirstPlayer], players[match.SecondPlayer], firstPlayerWinRateRating);
            firstPlayerCounts[match.FirstPlayer]++;
            secondPlayerCounts[match.SecondPlayer]++;
            firstPlayerWinProbabilitySums[match.FirstPlayer] += firstPlayerWinProbability;
            secondPlayerWinProbabilitySums[match.SecondPlayer] += 1.0 - firstPlayerWinProbability;
            totalWinProbabilitySums[match.FirstPlayer] += firstPlayerWinProbability;
            totalWinProbabilitySums[match.SecondPlayer] += 1.0 - firstPlayerWinProbability;
            opponentRatings[match.FirstPlayer].Add(players[match.SecondPlayer].Rating);
            opponentRatings[match.SecondPlayer].Add(players[match.FirstPlayer].Rating);
        }

        var rows = new List<GeneralSimulationResultRow>(players.Count);
        for (var playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            var expectedPlace = Enumerable.Range(0, players.Count)
                .Sum(place => (place + 1) * result.PlaceProbabilities[playerIndex, place]);
            var commonData = CreateCommonData(playerIndex);

            rows.Add(CreateChampionshipResultRow(
                commonData,
                result.PlaceProbabilities[playerIndex, 0],
                expectedPlace));
        }

        return rows;

        SimulationResultRowCommonData CreateCommonData(int playerIndex)
        {
            var firstPlayerWinRate = firstPlayerCounts[playerIndex] == 0
                ? (double?)null
                : firstPlayerWinProbabilitySums[playerIndex] / firstPlayerCounts[playerIndex];
            var secondPlayerWinRate = secondPlayerCounts[playerIndex] == 0
                ? (double?)null
                : secondPlayerWinProbabilitySums[playerIndex] / secondPlayerCounts[playerIndex];
            var matchCount = firstPlayerCounts[playerIndex] + secondPlayerCounts[playerIndex];
            var totalWinRate = matchCount == 0
                ? 0.0
                : totalWinProbabilitySums[playerIndex] / matchCount;
            var effectiveRating = SimulationRatingMath.CalculateEquivalentNeutralRating(opponentRatings[playerIndex], totalWinRate);
            var placeProbabilities = Enumerable.Range(0, players.Count)
                .Select(place => result.PlaceProbabilities[playerIndex, place])
                .ToArray();
            var placeCounts = result.SimulationCount.HasValue
                ? placeProbabilities.Select(value => value * result.SimulationCount.Value).ToArray()
                : null;

            return new SimulationResultRowCommonData(
                players[playerIndex].Name,
                players[playerIndex].Rating,
                effectiveRating,
                effectiveRating - players[playerIndex].Rating,
                firstPlayerCounts[playerIndex],
                secondPlayerCounts[playerIndex],
                firstPlayerWinRate,
                secondPlayerWinRate,
                placeProbabilities,
                placeCounts);
        }
    }

    internal static List<GeneralSimulationResultRow> BuildFinalStageGeneralResultRows(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount)
    {
        var standardRows = BuildGeneralResultRows(players, matches, result, firstPlayerWinRatePercent);
        var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
        var innovCount = players.Count - apexCount;

        return standardRows
            .Select(row =>
            {
                var group = groupMap[row.CommonData.Name];
                var groupStartIndex = group == FinalStageGroup.Apex ? 0 : apexCount + additionalApexCount;
                var groupSize = group == FinalStageGroup.Apex ? apexCount : innovCount;
                var groupPlaceAverage = Enumerable.Range(0, groupSize)
                    .Sum(offset => (offset + 1) * row.CommonData.PlaceProbabilities[groupStartIndex + offset]);
                var overallPlaceAverage = GetMetric(row, "averagePlace");

                return CreateGroupedOverallResultRow(
                    row.CommonData,
                    group.ToString(),
                    row.CommonData.PlaceProbabilities[groupStartIndex],
                    groupPlaceAverage,
                    row.CommonData.PlaceProbabilities[0],
                    overallPlaceAverage);
            })
            .ToList();
    }

    static GeneralSimulationResultRow CreateChampionshipResultRow(
        SimulationResultRowCommonData commonData,
        double championshipProbability,
        double averagePlace)
    {
        var championshipProbabilityPercent = (championshipProbability * 100).ToString("F2", CultureInfo.InvariantCulture);
        var averagePlaceText = averagePlace.ToString("F3", CultureInfo.InvariantCulture);

        return new GeneralSimulationResultRow(
            commonData,
            [
                new SimulationResultFreeColumn("championshipProbabilityPercent", championshipProbabilityPercent, championshipProbabilityPercent),
                new SimulationResultFreeColumn("averagePlace", averagePlaceText, averagePlaceText)
            ],
            new Dictionary<string, SimulationResultMetric>
            {
                ["championshipProbability"] = new("championshipProbability", championshipProbability),
                ["averagePlace"] = new("averagePlace", averagePlace)
            });
    }

    static GeneralSimulationResultRow CreateGroupedOverallResultRow(
        SimulationResultRowCommonData commonData,
        string group,
        double groupPlace1Probability,
        double groupPlaceAverage,
        double overallPlace1Probability,
        double overallPlaceAverage)
    {
        var groupPlace1ProbabilityPercent = (groupPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture);
        var groupPlaceAverageText = groupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture);
        var overallPlace1ProbabilityPercent = (overallPlace1Probability * 100).ToString("F2", CultureInfo.InvariantCulture);
        var overallPlaceAverageText = overallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture);

        return new GeneralSimulationResultRow(
            commonData,
            [
                new SimulationResultFreeColumn("group", group, group),
                new SimulationResultFreeColumn("groupPlace1ProbabilityPercent", groupPlace1ProbabilityPercent, groupPlace1ProbabilityPercent),
                new SimulationResultFreeColumn("groupPlaceAverage", groupPlaceAverageText, groupPlaceAverageText),
                new SimulationResultFreeColumn("overallPlace1ProbabilityPercent", overallPlace1ProbabilityPercent, overallPlace1ProbabilityPercent),
                new SimulationResultFreeColumn("overallPlaceAverage", overallPlaceAverageText, overallPlaceAverageText)
            ],
            new Dictionary<string, SimulationResultMetric>
            {
                ["groupPlace1Probability"] = new("groupPlace1Probability", groupPlace1Probability),
                ["groupPlaceAverage"] = new("groupPlaceAverage", groupPlaceAverage),
                ["overallPlace1Probability"] = new("overallPlace1Probability", overallPlace1Probability),
                ["overallPlaceAverage"] = new("overallPlaceAverage", overallPlaceAverage)
            });
    }

    static double GetMetric(GeneralSimulationResultRow row, string key)
    {
        if (row.Metrics.TryGetValue(key, out var metric))
        {
            return metric.Value;
        }

        throw new InvalidOperationException($"順位付け結果行に必要な metric がありません: {key}");
    }
}