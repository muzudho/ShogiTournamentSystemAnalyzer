internal static partial class Program
{
    static Dictionary<string, FinalStageGroup>? ReadOptionalFinalStageGroupMap(FinalStageGroupingMode groupingMode, IReadOnlyList<Participant> participants)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            return null;
        }

        return ReadFinalStageGroupMap();
    }

    static (List<Participant> Participants, List<Match> Matches) FilterToScheduledParticipants(IReadOnlyList<Participant> participants, IReadOnlyList<Match> matches)
    {
        var activeIndexes = matches
            .SelectMany(match => new[] { match.Black, match.White })
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
            .Select(match => new Match(indexMap[match.Black], indexMap[match.White]))
            .ToList();

        return (filteredParticipants, filteredMatches);
    }
}
