/*
 * ［アプリケーション］のうち、モード共通補助
 */
namespace ShogiTournamentSystemAnalyzer.Application.Helpers;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Presentation.Console;

internal static class ModeSupportHelpers
{
    internal static Dictionary<string, FinalStageGroup>? ReadOptionalFinalStageGroupMap(FinalStageGroupingMode groupingMode, IReadOnlyList<Player> players)
    {
        if (groupingMode == FinalStageGroupingMode.Off) return null;

        return ConsoleInputReaders.ReadFinalStageGroupMap();
    }

    internal static (List<Player> Players, List<Match> Matches) FilterToScheduledPlayers(IReadOnlyList<Player> players, IReadOnlyList<Match> matches)
    {
        var activeIndexes = matches
            .SelectMany(match => new[] { match.FirstPlayer, match.SecondPlayer })
            .Distinct()
            .OrderBy(index => index)
            .ToList();

        var indexMap = activeIndexes
            .Select((oldIndex, newIndex) => new { oldIndex, newIndex })
            .ToDictionary(x => x.oldIndex, x => x.newIndex);

        var filteredPlayers = activeIndexes
            .Select(index => players[index])
            .ToList();

        var filteredMatches = matches
            .Select(match => new Match(indexMap[match.FirstPlayer], indexMap[match.SecondPlayer]))
            .ToList();

        return (filteredPlayers, filteredMatches);
    }
}

