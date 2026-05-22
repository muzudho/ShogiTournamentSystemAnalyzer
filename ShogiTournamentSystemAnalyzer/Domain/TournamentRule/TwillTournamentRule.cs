using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal static class TwillTournamentRule
{
    internal static IReadOnlyList<List<int>> BuildRankingGroups(
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins,
        int participantCount)
    {
        return BuildRankingGroupsCore(matches, blackWins, participantCount, useCommonOpponentWeight: false);
    }

    internal static IReadOnlyList<List<int>> BuildRankingGroupsWithCommonOpponentWeight(
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins,
        int participantCount)
    {
        return BuildRankingGroupsCore(matches, blackWins, participantCount, useCommonOpponentWeight: true);
    }

    internal static void AccumulatePlaceProbabilities(
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins,
        double scenarioProbability,
        double[,] placeProbabilities)
    {
        AccumulatePlaceProbabilitiesCore(matches, blackWins, scenarioProbability, placeProbabilities, useCommonOpponentWeight: false);
    }

    internal static void AccumulatePlaceProbabilitiesWithCommonOpponentWeight(
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins,
        double scenarioProbability,
        double[,] placeProbabilities)
    {
        AccumulatePlaceProbabilitiesCore(matches, blackWins, scenarioProbability, placeProbabilities, useCommonOpponentWeight: true);
    }

    private static void AccumulatePlaceProbabilitiesCore(
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins,
        double scenarioProbability,
        double[,] placeProbabilities,
        bool useCommonOpponentWeight)
    {
        var finalGroups = BuildRankingGroupsCore(matches, blackWins, placeProbabilities.GetLength(0), useCommonOpponentWeight);
        AccumulateGroupedPlaces(finalGroups, scenarioProbability, placeProbabilities);
    }

    private static List<List<int>> BuildRankingGroupsCore(
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins,
        int participantCount,
        bool useCommonOpponentWeight)
    {
        var nodeCount = participantCount * 2;
        var adjacency = Enumerable.Range(0, nodeCount)
            .Select(_ => new HashSet<int>())
            .ToArray();
        var incomingCounts = new int[nodeCount];

        for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
        {
            var match = matches[matchIndex];
            var blackNode = GetBlackNodeIndex(match.FirstPlayer);
            var whiteNode = GetWhiteNodeIndex(match.SecondPlayer);
            var fromNode = blackWins[matchIndex] ? whiteNode : blackNode;
            var toNode = blackWins[matchIndex] ? blackNode : whiteNode;
            adjacency[fromNode].Add(toNode);
            incomingCounts[toNode]++;
        }

        var reachableCounts = new int[nodeCount];
        var longestPathLengths = new int[nodeCount];
        for (var nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
        {
            reachableCounts[nodeIndex] = CountReachableNodes(nodeIndex, adjacency);
            longestPathLengths[nodeIndex] = CalculateLongestPathLength(nodeIndex, adjacency);
        }

        var metrics = BuildPlayerMetrics(participantCount, adjacency, incomingCounts, reachableCounts, longestPathLengths);
        var primaryGroups = BuildPrimaryGroups(metrics);
        var refinedGroups = RefineGroupsByWhiteLossStrength(primaryGroups, metrics, adjacency);
        if (useCommonOpponentWeight)
        {
            refinedGroups = refinedGroups
                .SelectMany(group => RefineGroupByCommonOpponentReliability(group, matches, blackWins))
                .ToList();
        }

        var finalGroups = refinedGroups
            .SelectMany(group => RefineGroupByDirectEncounter(group, matches, blackWins))
            .ToList();

        return finalGroups;
    }

    private static int GetBlackNodeIndex(int participantIndex)
    {
        return participantIndex * 2;
    }

    private static int GetWhiteNodeIndex(int participantIndex)
    {
        return participantIndex * 2 + 1;
    }

    private static int CountReachableNodes(int startNode, IReadOnlyList<HashSet<int>> adjacency)
    {
        var visited = new bool[adjacency.Count];
        var stack = new Stack<int>();
        foreach (var next in adjacency[startNode])
        {
            stack.Push(next);
        }

        var reachableCount = 0;
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (visited[node]) continue;

            visited[node] = true;
            reachableCount++;
            foreach (var next in adjacency[node])
            {
                if (!visited[next])
                {
                    stack.Push(next);
                }
            }
        }

        return reachableCount;
    }

    private static int CalculateLongestPathLength(int startNode, IReadOnlyList<HashSet<int>> adjacency)
    {
        var visited = new bool[adjacency.Count];
        return CalculateLongestPathLengthCore(startNode, adjacency, visited);
    }

    private static int CalculateLongestPathLengthCore(int nodeIndex, IReadOnlyList<HashSet<int>> adjacency, bool[] visited)
    {
        visited[nodeIndex] = true;
        var best = 0;
        foreach (var next in adjacency[nodeIndex])
        {
            if (visited[next]) continue;

            var candidate = 1 + CalculateLongestPathLengthCore(next, adjacency, visited);
            if (candidate > best)
            {
                best = candidate;
            }
        }

        visited[nodeIndex] = false;
        return best;
    }

    private static List<TwillPlayerMetric> BuildPlayerMetrics(
        int participantCount,
        IReadOnlyList<HashSet<int>> adjacency,
        IReadOnlyList<int> incomingCounts,
        IReadOnlyList<int> reachableCounts,
        IReadOnlyList<int> longestPathLengths)
    {
        var metrics = new List<TwillPlayerMetric>(participantCount);
        for (var participantIndex = 0; participantIndex < participantCount; participantIndex++)
        {
            var blackNode = GetBlackNodeIndex(participantIndex);
            var whiteNode = GetWhiteNodeIndex(participantIndex);
            var whiteLossOpponents = adjacency[whiteNode]
                .Select(node => node / 2)
                .Distinct()
                .ToArray();

            metrics.Add(new TwillPlayerMetric(
                participantIndex,
                reachableCounts[blackNode] + reachableCounts[whiteNode],
                incomingCounts[blackNode] + incomingCounts[whiteNode],
                longestPathLengths[blackNode] + longestPathLengths[whiteNode],
                whiteLossOpponents));
        }

        return metrics;
    }

    private static List<List<int>> BuildPrimaryGroups(IReadOnlyList<TwillPlayerMetric> metrics)
    {
        var ordered = metrics
            .OrderBy(metric => metric.TotalRightNodeCount)
            .ThenByDescending(metric => metric.TotalIncomingWinCount)
            .ThenBy(metric => metric.TotalLongestPathLength)
            .ThenBy(metric => metric.ParticipantIndex)
            .ToList();

        var groups = new List<List<int>>();
        for (var index = 0; index < ordered.Count;)
        {
            var end = index + 1;
            while (end < ordered.Count
                && ordered[end].TotalRightNodeCount == ordered[index].TotalRightNodeCount
                && ordered[end].TotalIncomingWinCount == ordered[index].TotalIncomingWinCount
                && ordered[end].TotalLongestPathLength == ordered[index].TotalLongestPathLength)
            {
                end++;
            }

            groups.Add(ordered
                .Skip(index)
                .Take(end - index)
                .Select(metric => metric.ParticipantIndex)
                .ToList());
            index = end;
        }

        return groups;
    }

    private static List<List<int>> RefineGroupsByWhiteLossStrength(
        IReadOnlyList<List<int>> primaryGroups,
        IReadOnlyList<TwillPlayerMetric> metrics,
        IReadOnlyList<HashSet<int>> adjacency)
    {
        var primaryGroupRanks = new int[metrics.Count];
        var currentRank = 1;
        foreach (var group in primaryGroups)
        {
            foreach (var participantIndex in group)
            {
                primaryGroupRanks[participantIndex] = currentRank;
            }

            currentRank += group.Count;
        }

        var refinedGroups = new List<List<int>>();
        foreach (var group in primaryGroups)
        {
            if (group.Count <= 1)
            {
                refinedGroups.Add(group);
                continue;
            }

            var ordered = group
                .Select(participantIndex => new
                {
                    ParticipantIndex = participantIndex,
                    StrengthVector = BuildWhiteLossStrengthVector(metrics[participantIndex], primaryGroupRanks)
                })
                .OrderBy(x => x.StrengthVector, WhiteLossStrengthVectorComparer.Instance)
                .ThenBy(x => x.ParticipantIndex)
                .ToList();

            for (var index = 0; index < ordered.Count;)
            {
                var end = index + 1;
                while (end < ordered.Count
                    && WhiteLossStrengthVectorComparer.Instance.Compare(ordered[index].StrengthVector, ordered[end].StrengthVector) == 0)
                {
                    end++;
                }

                refinedGroups.Add(ordered
                    .Skip(index)
                    .Take(end - index)
                    .Select(x => x.ParticipantIndex)
                    .ToList());
                index = end;
            }
        }

        return refinedGroups;
    }

    private static int[] BuildWhiteLossStrengthVector(TwillPlayerMetric metric, IReadOnlyList<int> primaryGroupRanks)
    {
        return metric.WhiteLossOpponentParticipantIndexes
            .Select(opponentIndex => primaryGroupRanks[opponentIndex])
            .OrderBy(rank => rank)
            .ToArray();
    }

    private static IEnumerable<List<int>> RefineGroupByDirectEncounter(
        IReadOnlyList<int> group,
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins)
    {
        if (group.Count <= 1)
        {
            yield return group.ToList();
            yield break;
        }

        var groupSet = group.ToHashSet();
        var directWins = group.ToDictionary(participantIndex => participantIndex, _ => 0);
        foreach (var (match, blackWon) in matches.Zip(blackWins, static (match, blackWon) => (match, blackWon)))
        {
            if (!groupSet.Contains(match.FirstPlayer) || !groupSet.Contains(match.SecondPlayer)) continue;

            var winnerIndex = blackWon ? match.FirstPlayer : match.SecondPlayer;
            directWins[winnerIndex]++;
        }

        var ordered = group
            .OrderByDescending(participantIndex => directWins[participantIndex])
            .ThenBy(participantIndex => participantIndex)
            .ToList();

        for (var index = 0; index < ordered.Count;)
        {
            var end = index + 1;
            while (end < ordered.Count && directWins[ordered[end]] == directWins[ordered[index]])
            {
                end++;
            }

            yield return ordered.Skip(index).Take(end - index).ToList();
            index = end;
        }
    }

    private static IEnumerable<List<int>> RefineGroupByCommonOpponentReliability(
        IReadOnlyList<int> group,
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> blackWins)
    {
        if (group.Count <= 1)
        {
            yield return group.ToList();
            yield break;
        }

        var outcomesByPlayer = BuildOutcomeMapByPlayer(matches, blackWins);
        var weightedWins = group.ToDictionary(participantIndex => participantIndex, _ => 0);

        for (var leftIndex = 0; leftIndex < group.Count - 1; leftIndex++)
        {
            for (var rightIndex = leftIndex + 1; rightIndex < group.Count; rightIndex++)
            {
                var leftPlayerIndex = group[leftIndex];
                var rightPlayerIndex = group[rightIndex];
                var weightedEvidence = CalculateWeightedCommonOpponentEvidence(leftPlayerIndex, rightPlayerIndex, outcomesByPlayer);
                if (weightedEvidence > CommonOpponentEvidenceThreshold)
                {
                    weightedWins[leftPlayerIndex]++;
                }
                else if (weightedEvidence < -CommonOpponentEvidenceThreshold)
                {
                    weightedWins[rightPlayerIndex]++;
                }
            }
        }

        var ordered = group
            .OrderByDescending(participantIndex => weightedWins[participantIndex])
            .ThenBy(participantIndex => participantIndex)
            .ToList();

        for (var index = 0; index < ordered.Count;)
        {
            var end = index + 1;
            while (end < ordered.Count && weightedWins[ordered[end]] == weightedWins[ordered[index]])
            {
                end++;
            }

            yield return ordered.Skip(index).Take(end - index).ToList();
            index = end;
        }
    }

    private static Dictionary<int, Dictionary<int, bool>> BuildOutcomeMapByPlayer(
        IReadOnlyList<Match> matches,
        IReadOnlyList<bool> firstPlayerWins)
    {
        var outcomeMap = new Dictionary<int, Dictionary<int, bool>>();
        foreach (var (match, firstPlayerWon) in matches.Zip(firstPlayerWins, static (match, firstPlayerWon) => (match, firstPlayerWon)))
        {
            if (!outcomeMap.TryGetValue(match.FirstPlayer, out var firstPlayerMap))
            {
                firstPlayerMap = new Dictionary<int, bool>();
                outcomeMap.Add(match.FirstPlayer, firstPlayerMap);
            }

            if (!outcomeMap.TryGetValue(match.SecondPlayer, out var secondPlayerMap))
            {
                secondPlayerMap = new Dictionary<int, bool>();
                outcomeMap.Add(match.SecondPlayer, secondPlayerMap);
            }

            firstPlayerMap[match.SecondPlayer] = firstPlayerWon;
            secondPlayerMap[match.FirstPlayer] = !firstPlayerWon;
        }

        return outcomeMap;
    }

    private static double CalculateWeightedCommonOpponentEvidence(
        int leftPlayerIndex,
        int rightPlayerIndex,
        IReadOnlyDictionary<int, Dictionary<int, bool>> outcomesByPlayer)
    {
        if (!outcomesByPlayer.TryGetValue(leftPlayerIndex, out var leftOutcomes)
            || !outcomesByPlayer.TryGetValue(rightPlayerIndex, out var rightOutcomes)) return 0.0;

        var commonOpponentIndexes = leftOutcomes.Keys
            .Intersect(rightOutcomes.Keys)
            .Where(opponentIndex => opponentIndex != leftPlayerIndex && opponentIndex != rightPlayerIndex)
            .ToArray();
        if (commonOpponentIndexes.Length == 0) return 0.0;

        var rawEvidence = 0;
        foreach (var opponentIndex in commonOpponentIndexes)
        {
            var leftWon = leftOutcomes[opponentIndex];
            var rightWon = rightOutcomes[opponentIndex];
            if (leftWon == rightWon) continue;

            rawEvidence += leftWon ? 1 : -1;
        }

        if (rawEvidence == 0) return 0.0;

        return (double)rawEvidence / commonOpponentIndexes.Length * GetCommonOpponentReliability(commonOpponentIndexes.Length);
    }

    private static double GetCommonOpponentReliability(int commonOpponentCount)
    {
        return commonOpponentCount switch
        {
            <= 0 => 0.0,
            1 => 0.35,
            2 => 0.60,
            3 => 0.80,
            _ => 1.00,
        };
    }

    private const double CommonOpponentEvidenceThreshold = 0.5;

    private static void AccumulateGroupedPlaces(IReadOnlyList<List<int>> groups, double scenarioProbability, double[,] placeProbabilities)
    {
        var currentPlace = 0;
        foreach (var group in groups)
        {
            var splitProbability = scenarioProbability / group.Count;
            foreach (var participantIndex in group)
            {
                for (var place = currentPlace; place < currentPlace + group.Count; place++)
                {
                    placeProbabilities[participantIndex, place] += splitProbability;
                }
            }

            currentPlace += group.Count;
        }
    }

    private sealed class WhiteLossStrengthVectorComparer : IComparer<int[]>
    {
        internal static WhiteLossStrengthVectorComparer Instance { get; } = new();

        public int Compare(int[]? x, int[]? y)
        {
            if (ReferenceEquals(x, y)) return 0;

            if (x is null) return -1;

            if (y is null) return 1;

            var maxLength = Math.Max(x.Length, y.Length);
            for (var index = 0; index < maxLength; index++)
            {
                var left = index < x.Length ? x[index] : int.MaxValue;
                var right = index < y.Length ? y[index] : int.MaxValue;
                var comparison = left.CompareTo(right);
                if (comparison != 0) return comparison;
            }

            return 0;
        }
    }

    private readonly record struct TwillPlayerMetric(
        int ParticipantIndex,
        int TotalRightNodeCount,
        int TotalIncomingWinCount,
        int TotalLongestPathLength,
        int[] WhiteLossOpponentParticipantIndexes);
}

