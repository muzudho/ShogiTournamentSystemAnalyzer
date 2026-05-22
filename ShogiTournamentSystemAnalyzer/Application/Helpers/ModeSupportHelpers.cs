/*
 * ［アプリケーション］のうち、モード共通補助
 */
namespace ShogiTournamentSystemAnalyzer.Application.Helpers;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static class ModeSupportHelpers
{
    internal static Dictionary<string, FinalStageGroup>? ReadOptionalFinalStageGroupMap(FinalStageGroupingMode groupingMode, IReadOnlyList<Player> participants)
    {
        if (groupingMode == FinalStageGroupingMode.Off) return null;

        return Program.ReadFinalStageGroupMap();
    }

    internal static (List<Player> Participants, List<Match> Matches) FilterToScheduledParticipants(IReadOnlyList<Player> participants, IReadOnlyList<Match> matches)
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

