/*
 * ［順位付け域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.Ranking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class RankingResultRowBuilder
{
    internal static List<StandardResultRow> BuildResultRows(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, CalculationResult result, double firstPlayerWinRatePercent)
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

        var rows = new List<StandardResultRow>(players.Count);
        for (var playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            var expectedPlace = Enumerable.Range(0, players.Count)
                .Sum(place => (place + 1) * result.PlaceProbabilities[playerIndex, place]);
            var commonData = CreateCommonData(playerIndex);

            rows.Add(new StandardResultRow(
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

    internal static List<FinalStageResultRow> BuildFinalStageResultRows(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount)
    {
        var standardRows = BuildResultRows(players, matches, result, firstPlayerWinRatePercent);
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
                    row.CommonData,
                    group.ToString(),
                    row.PlaceProbabilities[groupStartIndex],
                    groupPlaceAverage,
                    row.PlaceProbabilities[0],
                    row.AveragePlace);
            })
            .ToList();
    }
}
