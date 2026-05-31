/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.Simulation;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class FinalStageCalculationEngine
{
    internal static CalculationResult CalculateFinalStageExactly(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double firstPlayerWinRateRating, int promotedInnovCount = 0)
    {
        var placeProbabilities = new double[players.Count, players.Count + additionalApexCount];
        var wins = new int[players.Count];
        var apexPlayerIndexes = GetPlayerIndexesByGroup(players, groupMap, FinalStageGroup.Apex);
        var innovPlayerIndexes = GetPlayerIndexesByGroup(players, groupMap, FinalStageGroup.Innov);
        var completedScenarioWeight = 0.0;

        void Explore(int matchIndex, double scenarioProbability)
        {
            if (!SimulationTimeBudget.HasApplicationTimeRemaining()) return;

            if (matchIndex == matches.Count)
            {
                completedScenarioWeight += scenarioProbability;
                AccumulateFinalStagePlaceProbabilities(wins, players, apexPlayerIndexes, innovPlayerIndexes, additionalApexCount, boundaryRescueMode, firstPlayerWinRateRating, scenarioProbability, placeProbabilities, promotedInnovCount);
                return;
            }

            var match = matches[matchIndex];
            var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(players[match.FirstPlayer], players[match.SecondPlayer], firstPlayerWinRateRating);

            wins[match.FirstPlayer]++;
            Explore(matchIndex + 1, scenarioProbability * firstPlayerWinProbability);
            wins[match.FirstPlayer]--;

            wins[match.SecondPlayer]++;
            Explore(matchIndex + 1, scenarioProbability * (1.0 - firstPlayerWinProbability));
            wins[match.SecondPlayer]--;
        }

        Explore(0, 1.0);
        SimulationTimeBudget.NormalizePlaceProbabilities(placeProbabilities, completedScenarioWeight);
        var modeLabel = completedScenarioWeight < 1.0
            ? $"本戦専用 厳密計算 ({(completedScenarioWeight * 100.0).ToString("F2")}%, 時間切れ)"
            : "本戦専用 厳密計算";
        return new CalculationResult(placeProbabilities, modeLabel, null);
    }

    internal static CalculationResult CalculateFinalStageBySimulation(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double firstPlayerWinRateRating, int simulationCount, int promotedInnovCount = 0)
    {
        var placeProbabilities = new double[players.Count, players.Count + additionalApexCount];
        var wins = new int[players.Count];
        var apexPlayerIndexes = GetPlayerIndexesByGroup(players, groupMap, FinalStageGroup.Apex);
        var innovPlayerIndexes = GetPlayerIndexesByGroup(players, groupMap, FinalStageGroup.Innov);
        var completedSimulationCount = 0;

        for (var simulation = 0; simulation < simulationCount; simulation++)
        {
            if (!SimulationTimeBudget.HasSimulationTimeRemaining()) break;

            Array.Clear(wins);

            foreach (var match in matches)
            {
                if (!SimulationTimeBudget.HasApplicationTimeRemaining())
                {
                    simulation = simulationCount;
                    break;
                }

                var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(players[match.FirstPlayer], players[match.SecondPlayer], firstPlayerWinRateRating);
                if (Random.Shared.NextDouble() < firstPlayerWinProbability)
                {
                    wins[match.FirstPlayer]++;
                }
                else
                {
                    wins[match.SecondPlayer]++;
                }
            }

            if (!SimulationTimeBudget.HasApplicationTimeRemaining()) break;

            AccumulateFinalStagePlaceProbabilities(wins, players, apexPlayerIndexes, innovPlayerIndexes, additionalApexCount, boundaryRescueMode, firstPlayerWinRateRating, 1.0, placeProbabilities, promotedInnovCount);
            completedSimulationCount++;
        }

        SimulationTimeBudget.NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

        var modeLabel = completedSimulationCount < simulationCount
            ? $"本戦専用 シミュレーション ({completedSimulationCount:N0}/{simulationCount:N0}回, 時間切れ)"
            : $"本戦専用 シミュレーション ({simulationCount:N0}回)";
        return new CalculationResult(placeProbabilities, modeLabel, completedSimulationCount);
    }

    static List<int> GetPlayerIndexesByGroup(IReadOnlyList<Player> players, IReadOnlyDictionary<string, FinalStageGroup> groupMap, FinalStageGroup targetGroup)
    {
        return players
            .Select((player, index) => new { player.Name, Index = index })
            .Where(x => groupMap[x.Name] == targetGroup)
            .Select(x => x.Index)
            .ToList();
    }

    static void AccumulateFinalStagePlaceProbabilities(int[] wins, IReadOnlyList<Player> players, IReadOnlyList<int> apexPlayerIndexes, IReadOnlyList<int> innovPlayerIndexes, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double blackAdvantageRating, double scenarioProbability, double[,] placeProbabilities, int promotedInnovCount = 0)
    {
        if (promotedInnovCount > 0)
        {
            AccumulateVariableTop8PlaceProbabilities(wins, apexPlayerIndexes, innovPlayerIndexes, additionalApexCount, promotedInnovCount, scenarioProbability, placeProbabilities);
            return;
        }

        if (boundaryRescueMode == BoundaryRescueMode.Off)
        {
            AccumulateGroupPlaceProbabilities(wins, apexPlayerIndexes, 0, scenarioProbability, placeProbabilities);
            AccumulateGroupPlaceProbabilities(wins, innovPlayerIndexes, apexPlayerIndexes.Count + additionalApexCount, scenarioProbability, placeProbabilities);
            return;
        }

        var apexRanking = BuildRankingForPlayerIndexes(wins, apexPlayerIndexes);
        var innovRanking = BuildRankingForPlayerIndexes(wins, innovPlayerIndexes);

        var apexBoundaryIndexes = GetTiedPlayerIndexesAtPosition(apexRanking, apexRanking.Length - 1);
        var innovBoundaryIndexes = GetTiedPlayerIndexesAtPosition(innovRanking, 0);

        var rescueScenarioProbability = scenarioProbability / (apexBoundaryIndexes.Count * innovBoundaryIndexes.Count);
        foreach (var apexBoundaryIndex in apexBoundaryIndexes)
        {
            foreach (var innovBoundaryIndex in innovBoundaryIndexes)
            {
                var blackWinsProbability = SimulationRatingMath.GetWinProbability(players[innovBoundaryIndex], players[apexBoundaryIndex], blackAdvantageRating);
                AccumulateFinalStagePlaceProbabilitiesWithBoundaryRescue(
                    wins,
                    apexRanking,
                    innovRanking,
                    additionalApexCount,
                    apexBoundaryIndex,
                    innovBoundaryIndex,
                    rescueScenarioProbability,
                    blackWinsProbability,
                    placeProbabilities);
            }
        }
    }

    static void AccumulateVariableTop8PlaceProbabilities(int[] wins, IReadOnlyList<int> apexPlayerIndexes, IReadOnlyList<int> innovPlayerIndexes, int additionalApexCount, int promotedInnovCount, double scenarioProbability, double[,] placeProbabilities)
    {
        var apexRanking = BuildRankingForPlayerIndexes(wins, apexPlayerIndexes);
        var innovRanking = BuildRankingForPlayerIndexes(wins, innovPlayerIndexes);
        var actualPromotedInnovCount = Math.Min(promotedInnovCount, Math.Min(apexRanking.Length, innovRanking.Length));
        var leadingApexCount = Math.Max(0, apexRanking.Length - actualPromotedInnovCount);

        var leadingApexRanking = apexRanking.Take(leadingApexCount).ToArray();
        var trailingApexRanking = apexRanking.Skip(leadingApexCount).ToArray();
        var promotedInnovRanking = innovRanking.Take(actualPromotedInnovCount).ToArray();
        var remainingInnovRanking = innovRanking.Skip(actualPromotedInnovCount).ToArray();

        AccumulateRankingProbabilities(leadingApexRanking, 0, scenarioProbability, placeProbabilities);
        AccumulateRankingProbabilities(promotedInnovRanking, leadingApexCount, scenarioProbability, placeProbabilities);
        AccumulateRankingProbabilities(trailingApexRanking, leadingApexCount + actualPromotedInnovCount, scenarioProbability, placeProbabilities);
        AccumulateRankingProbabilities(remainingInnovRanking, apexRanking.Length + actualPromotedInnovCount + additionalApexCount, scenarioProbability, placeProbabilities);
    }

    static PlayerScore[] BuildRankingForPlayerIndexes(int[] wins, IReadOnlyList<int> playerIndexes)
    {
        return playerIndexes
            .Select(index => new PlayerScore(index, wins[index]))
            .OrderByDescending(x => x.Wins)
            .ThenBy(x => x.PlayerIndex)
            .ToArray();
    }

    static List<int> GetTiedPlayerIndexesAtPosition(IReadOnlyList<PlayerScore> ranking, int position)
    {
        var tiedPlayerIndexes = new List<int>();
        var targetWins = ranking[position].Wins;
        var index = position;
        while (index > 0 && ranking[index - 1].Wins == targetWins)
        {
            index--;
        }

        while (index < ranking.Count && ranking[index].Wins == targetWins)
        {
            tiedPlayerIndexes.Add(ranking[index].PlayerIndex);
            index++;
        }

        return tiedPlayerIndexes;
    }

    static void AccumulateFinalStagePlaceProbabilitiesWithBoundaryRescue(
        int[] wins,
        IReadOnlyList<PlayerScore> apexRanking,
        IReadOnlyList<PlayerScore> innovRanking,
        int additionalApexCount,
        int apexBoundaryIndex,
        int innovBoundaryIndex,
        double rescueScenarioProbability,
        double innovWinsProbability,
        double[,] placeProbabilities)
    {
        AccumulateBoundaryRescueOutcome(wins, apexRanking, innovRanking, additionalApexCount, apexBoundaryIndex, innovBoundaryIndex, rescueScenarioProbability * innovWinsProbability, innovWins: true, placeProbabilities);
        AccumulateBoundaryRescueOutcome(wins, apexRanking, innovRanking, additionalApexCount, apexBoundaryIndex, innovBoundaryIndex, rescueScenarioProbability * (1.0 - innovWinsProbability), innovWins: false, placeProbabilities);
    }

    static void AccumulateBoundaryRescueOutcome(
        int[] wins,
        IReadOnlyList<PlayerScore> apexRanking,
        IReadOnlyList<PlayerScore> innovRanking,
        int additionalApexCount,
        int apexBoundaryIndex,
        int innovBoundaryIndex,
        double scenarioProbability,
        bool innovWins,
        double[,] placeProbabilities)
    {
        var apexGroupSize = apexRanking.Count;
        var rescuedApexRanking = apexRanking
            .Where(x => x.PlayerIndex != apexBoundaryIndex)
            .ToArray();
        var rescuedInnovRanking = innovRanking
            .Where(x => x.PlayerIndex != innovBoundaryIndex)
            .ToArray();

        AccumulateRankingProbabilities(rescuedApexRanking, 0, scenarioProbability, placeProbabilities);

        if (innovWins)
        {
            placeProbabilities[innovBoundaryIndex, apexGroupSize - 1] += scenarioProbability;
            placeProbabilities[apexBoundaryIndex, apexGroupSize + additionalApexCount] += scenarioProbability;
        }
        else
        {
            placeProbabilities[apexBoundaryIndex, apexGroupSize - 1] += scenarioProbability;
            placeProbabilities[innovBoundaryIndex, apexGroupSize + additionalApexCount] += scenarioProbability;
        }

        AccumulateRankingProbabilities(
            rescuedInnovRanking,
            apexGroupSize + additionalApexCount + 1,
            scenarioProbability,
            placeProbabilities);
    }

    static void AccumulateRankingProbabilities(IReadOnlyList<PlayerScore> ranking, int placeOffset, double scenarioProbability, double[,] placeProbabilities)
    {
        var currentPlace = 0;
        while (currentPlace < ranking.Count)
        {
            var groupEnd = currentPlace + 1;
            while (groupEnd < ranking.Count && ranking[groupEnd].Wins == ranking[currentPlace].Wins)
            {
                groupEnd++;
            }

            var groupSize = groupEnd - currentPlace;
            var splitProbability = scenarioProbability / groupSize;

            for (var i = currentPlace; i < groupEnd; i++)
            {
                var playerIndex = ranking[i].PlayerIndex;
                for (var place = currentPlace; place < groupEnd; place++)
                {
                    placeProbabilities[playerIndex, placeOffset + place] += splitProbability;
                }
            }

            currentPlace = groupEnd;
        }
    }

    static void AccumulateGroupPlaceProbabilities(int[] wins, IReadOnlyList<int> playerIndexes, int placeOffset, double scenarioProbability, double[,] placeProbabilities)
    {
        var ranking = playerIndexes
            .Select(index => new PlayerScore(index, wins[index]))
            .OrderByDescending(x => x.Wins)
            .ThenBy(x => x.PlayerIndex)
            .ToArray();

        var currentPlace = 0;
        while (currentPlace < ranking.Length)
        {
            var groupEnd = currentPlace + 1;
            while (groupEnd < ranking.Length && ranking[groupEnd].Wins == ranking[currentPlace].Wins)
            {
                groupEnd++;
            }

            var groupSize = groupEnd - currentPlace;
            var splitProbability = scenarioProbability / groupSize;

            for (var i = currentPlace; i < groupEnd; i++)
            {
                var playerIndex = ranking[i].PlayerIndex;
                for (var place = currentPlace; place < groupEnd; place++)
                {
                    placeProbabilities[playerIndex, placeOffset + place] += splitProbability;
                }
            }

            currentPlace = groupEnd;
        }
    }
}

