/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal sealed class TwillTournamentRankingRule : IRankingRule
{
    internal static readonly TwillTournamentRankingRule Instance = new(useCommonOpponentWeight: false);
    internal static readonly TwillTournamentRankingRule CommonOpponentWeightedInstance = new(useCommonOpponentWeight: true);

    readonly bool _useCommonOpponentWeight;

    TwillTournamentRankingRule(bool useCommonOpponentWeight)
    {
        _useCommonOpponentWeight = useCommonOpponentWeight;
    }

    public IReadOnlyList<PlayerRankRow> Rank(TournamentState state, int? stageId)
    {
        var orderedPlayers = state.Players
            .OrderBy(player => player.PlayerId)
            .ToArray();
        var playerIndexById = orderedPlayers
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);
        var targetMatches = state.MatchRecords
            .Where(match => match.Status == MatchStatus.Finished)
            .Where(match => stageId is null || match.StageId == stageId.Value)
            .ToArray();

        var matches = new Match[targetMatches.Length];
        var blackWins = new bool[targetMatches.Length];
        var pointsByPlayerId = orderedPlayers.ToDictionary(player => player.PlayerId, _ => 0);
        for (var matchIndex = 0; matchIndex < targetMatches.Length; matchIndex++)
        {
            var match = targetMatches[matchIndex];
            matches[matchIndex] = new Match(
                playerIndexById[match.FirstPlayerId],
                playerIndexById[match.SecondPlayerId]);

            switch (match.ResultType)
            {
                case MatchResultType.FirstPlayerWin:
                case MatchResultType.FirstPlayerForfeitWin:
                    blackWins[matchIndex] = true;
                    pointsByPlayerId[match.FirstPlayerId]++;
                    break;
                case MatchResultType.SecondPlayerWin:
                case MatchResultType.SecondPlayerForfeitWin:
                    blackWins[matchIndex] = false;
                    pointsByPlayerId[match.SecondPlayerId]++;
                    break;
                default:
                    throw new OperationCanceledException($"Twill 系順位ルールでは未対応の対局結果です: {match.ResultType}");
            }
        }

        var groups = _useCommonOpponentWeight
            ? TwillTournamentRule.BuildRankingGroupsWithCommonOpponentWeight(matches, blackWins, orderedPlayers.Length)
            : TwillTournamentRule.BuildRankingGroups(matches, blackWins, orderedPlayers.Length);

        var rows = new List<PlayerRankRow>(orderedPlayers.Length);
        var rank = 1;
        foreach (var group in groups)
        {
            foreach (var participantIndex in group)
            {
                var player = orderedPlayers[participantIndex];
                rows.Add(new PlayerRankRow(
                    player.PlayerId,
                    rank,
                    pointsByPlayerId[player.PlayerId],
                    stageId is null
                        ? (_useCommonOpponentWeight ? "overall/twill+commonopp" : "overall/twill")
                        : (_useCommonOpponentWeight ? $"stage:{stageId.Value}/twill+commonopp" : $"stage:{stageId.Value}/twill")));
            }

            rank += group.Count;
        }

        return rows;
    }
}
