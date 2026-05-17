internal static partial class Program
{
    static Dictionary<string, FinalStageGroup>? ReadOptionalFinalStageGroupMap(FinalStageGroupingMode groupingMode, IReadOnlyList<Player> participants)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            return null;
        }

        return ReadFinalStageGroupMap();
    }

    static (List<Player> Participants, List<Match> Matches) FilterToScheduledParticipants(IReadOnlyList<Player> participants, IReadOnlyList<Match> matches)
    {
        var activeIndexes = matches
            .SelectMany(match => new[] { match.FirstPlayer, match.SecondPlayer })
            .Distinct()
            .OrderBy(index => index)
            .ToList();

        var indexMap = activeIndexes
            .Select((oldIndex, newIndex) => new { oldIndex, newIndex })
            .ToDictionary(x => x.oldIndex, x => x.newIndex);

        var filteredParticipants = activeIndexes
            .Select(index => participants[index])
            .ToList();

        var filteredMatches = matches
            .Select(match => new Match(indexMap[match.FirstPlayer], indexMap[match.SecondPlayer]))
            .ToList();

        return (filteredParticipants, filteredMatches);
    }
}

