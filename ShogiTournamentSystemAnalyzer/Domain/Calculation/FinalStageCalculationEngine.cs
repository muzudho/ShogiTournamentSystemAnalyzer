internal static partial class Program
{
    static CalculationResult CalculateFinalStageExactly(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double blackAdvantageRating, int promotedInnovCount = 0)
    {
        var placeProbabilities = new double[participants.Count, participants.Count + additionalApexCount];
        var wins = new int[participants.Count];
        var apexParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Apex);
        var innovParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Innov);

        void Explore(int matchIndex, double scenarioProbability)
        {
            if (matchIndex == matches.Count)
            {
                AccumulateFinalStagePlaceProbabilities(wins, participants, apexParticipantIndexes, innovParticipantIndexes, additionalApexCount, boundaryRescueMode, blackAdvantageRating, scenarioProbability, placeProbabilities, promotedInnovCount);
                return;
            }

            var match = matches[matchIndex];
            var blackWinsProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);

            wins[match.Black]++;
            Explore(matchIndex + 1, scenarioProbability * blackWinsProbability);
            wins[match.Black]--;

            wins[match.White]++;
            Explore(matchIndex + 1, scenarioProbability * (1.0 - blackWinsProbability));
            wins[match.White]--;
        }

        Explore(0, 1.0);
        return new CalculationResult(placeProbabilities, "本戦専用 厳密計算", null);
    }

    static CalculationResult CalculateFinalStageBySimulation(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches, IReadOnlyDictionary<string, FinalStageGroup> groupMap, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double blackAdvantageRating, int simulationCount, int promotedInnovCount = 0)
    {
        var placeProbabilities = new double[participants.Count, participants.Count + additionalApexCount];
        var wins = new int[participants.Count];
        var apexParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Apex);
        var innovParticipantIndexes = GetParticipantIndexesByGroup(participants, groupMap, FinalStageGroup.Innov);
        var completedSimulationCount = 0;

        for (var simulation = 0; simulation < simulationCount; simulation++)
        {
            if (!HasSimulationTimeRemaining())
            {
                break;
            }

            Array.Clear(wins);

            foreach (var match in matches)
            {
                var blackWinsProbability = GetWinProbability(participants[match.Black], participants[match.White], blackAdvantageRating);
                if (Random.Shared.NextDouble() < blackWinsProbability)
                {
                    wins[match.Black]++;
                }
                else
                {
                    wins[match.White]++;
                }
            }

            AccumulateFinalStagePlaceProbabilities(wins, participants, apexParticipantIndexes, innovParticipantIndexes, additionalApexCount, boundaryRescueMode, blackAdvantageRating, 1.0, placeProbabilities, promotedInnovCount);
            completedSimulationCount++;
        }

        NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

        var modeLabel = completedSimulationCount < simulationCount
            ? $"本戦専用 シミュレーション ({completedSimulationCount:N0}/{simulationCount:N0}回, 時間切れ)"
            : $"本戦専用 シミュレーション ({simulationCount:N0}回)";
        return new CalculationResult(placeProbabilities, modeLabel, completedSimulationCount);
    }

    static List<int> GetParticipantIndexesByGroup(IReadOnlyList<Participant> participants, IReadOnlyDictionary<string, FinalStageGroup> groupMap, FinalStageGroup targetGroup)
    {
        return participants
            .Select((participant, index) => new { participant.Name, Index = index })
            .Where(x => groupMap[x.Name] == targetGroup)
            .Select(x => x.Index)
            .ToList();
    }

    static void AccumulateFinalStagePlaceProbabilities(int[] wins, IReadOnlyList<Participant> participants, IReadOnlyList<int> apexParticipantIndexes, IReadOnlyList<int> innovParticipantIndexes, int additionalApexCount, BoundaryRescueMode boundaryRescueMode, double blackAdvantageRating, double scenarioProbability, double[,] placeProbabilities, int promotedInnovCount = 0)
    {
        if (promotedInnovCount > 0)
        {
            AccumulateVariableTop8PlaceProbabilities(wins, apexParticipantIndexes, innovParticipantIndexes, additionalApexCount, promotedInnovCount, scenarioProbability, placeProbabilities);
            return;
        }

        if (boundaryRescueMode == BoundaryRescueMode.Off)
        {
            AccumulateGroupPlaceProbabilities(wins, apexParticipantIndexes, 0, scenarioProbability, placeProbabilities);
            AccumulateGroupPlaceProbabilities(wins, innovParticipantIndexes, apexParticipantIndexes.Count + additionalApexCount, scenarioProbability, placeProbabilities);
            return;
        }

        var apexRanking = BuildRankingForParticipantIndexes(wins, apexParticipantIndexes);
        var innovRanking = BuildRankingForParticipantIndexes(wins, innovParticipantIndexes);

        var apexBoundaryIndexes = GetTiedParticipantIndexesAtPosition(apexRanking, apexRanking.Length - 1);
        var innovBoundaryIndexes = GetTiedParticipantIndexesAtPosition(innovRanking, 0);

        var rescueScenarioProbability = scenarioProbability / (apexBoundaryIndexes.Count * innovBoundaryIndexes.Count);
        foreach (var apexBoundaryIndex in apexBoundaryIndexes)
        {
            foreach (var innovBoundaryIndex in innovBoundaryIndexes)
            {
                var blackWinsProbability = GetWinProbability(participants[innovBoundaryIndex], participants[apexBoundaryIndex], blackAdvantageRating);
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

    static void AccumulateVariableTop8PlaceProbabilities(int[] wins, IReadOnlyList<int> apexParticipantIndexes, IReadOnlyList<int> innovParticipantIndexes, int additionalApexCount, int promotedInnovCount, double scenarioProbability, double[,] placeProbabilities)
    {
        var apexRanking = BuildRankingForParticipantIndexes(wins, apexParticipantIndexes);
        var innovRanking = BuildRankingForParticipantIndexes(wins, innovParticipantIndexes);
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

    static ParticipantScore[] BuildRankingForParticipantIndexes(int[] wins, IReadOnlyList<int> participantIndexes)
    {
        return participantIndexes
            .Select(index => new ParticipantScore(index, wins[index]))
            .OrderByDescending(x => x.Wins)
            .ThenBy(x => x.ParticipantIndex)
            .ToArray();
    }

    static List<int> GetTiedParticipantIndexesAtPosition(IReadOnlyList<ParticipantScore> ranking, int position)
    {
        var tiedParticipantIndexes = new List<int>();
        var targetWins = ranking[position].Wins;
        var index = position;
        while (index > 0 && ranking[index - 1].Wins == targetWins)
        {
            index--;
        }

        while (index < ranking.Count && ranking[index].Wins == targetWins)
        {
            tiedParticipantIndexes.Add(ranking[index].ParticipantIndex);
            index++;
        }

        return tiedParticipantIndexes;
    }

    static void AccumulateFinalStagePlaceProbabilitiesWithBoundaryRescue(
        int[] wins,
        IReadOnlyList<ParticipantScore> apexRanking,
        IReadOnlyList<ParticipantScore> innovRanking,
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
        IReadOnlyList<ParticipantScore> apexRanking,
        IReadOnlyList<ParticipantScore> innovRanking,
        int additionalApexCount,
        int apexBoundaryIndex,
        int innovBoundaryIndex,
        double scenarioProbability,
        bool innovWins,
        double[,] placeProbabilities)
    {
        var apexGroupSize = apexRanking.Count;
        var rescuedApexRanking = apexRanking
            .Where(x => x.ParticipantIndex != apexBoundaryIndex)
            .ToArray();
        var rescuedInnovRanking = innovRanking
            .Where(x => x.ParticipantIndex != innovBoundaryIndex)
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

    static void AccumulateRankingProbabilities(IReadOnlyList<ParticipantScore> ranking, int placeOffset, double scenarioProbability, double[,] placeProbabilities)
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
                var participantIndex = ranking[i].ParticipantIndex;
                for (var place = currentPlace; place < groupEnd; place++)
                {
                    placeProbabilities[participantIndex, placeOffset + place] += splitProbability;
                }
            }

            currentPlace = groupEnd;
        }
    }

    static void AccumulateGroupPlaceProbabilities(int[] wins, IReadOnlyList<int> participantIndexes, int placeOffset, double scenarioProbability, double[,] placeProbabilities)
    {
        var ranking = participantIndexes
            .Select(index => new ParticipantScore(index, wins[index]))
            .OrderByDescending(x => x.Wins)
            .ThenBy(x => x.ParticipantIndex)
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
                var participantIndex = ranking[i].ParticipantIndex;
                for (var place = currentPlace; place < groupEnd; place++)
                {
                    placeProbabilities[participantIndex, placeOffset + place] += splitProbability;
                }
            }

            currentPlace = groupEnd;
        }
    }
}
