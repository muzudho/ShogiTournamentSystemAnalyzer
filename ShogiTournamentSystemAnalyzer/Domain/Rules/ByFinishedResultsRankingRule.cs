internal sealed class ByFinishedResultsRankingRule : IRankingRule
{
    internal static readonly ByFinishedResultsRankingRule Instance = new();

    public IReadOnlyList<PlayerRankRow> Rank(TournamentState state, int? stageId)
    {
        var targetMatches = state.MatchRecords
            .Where(match => match.Status == MatchStatus.Finished)
            .Where(match => stageId is null || match.StageId == stageId.Value)
            .ToArray();

        var pointsByPlayerId = new Dictionary<int, int>();
        foreach (var player in state.Players)
        {
            pointsByPlayerId[player.PlayerId] = 0;
        }

        foreach (var match in targetMatches)
        {
            switch (match.ResultType)
            {
                case MatchResultType.FirstPlayerWin:
                case MatchResultType.FirstPlayerForfeitWin:
                    pointsByPlayerId[match.FirstPlayerId]++;
                    break;
                case MatchResultType.SecondPlayerWin:
                case MatchResultType.SecondPlayerForfeitWin:
                    pointsByPlayerId[match.SecondPlayerId]++;
                    break;
                case MatchResultType.Draw:
                    break;
            }
        }

        return state.Players
            .OrderByDescending(player => pointsByPlayerId[player.PlayerId])
            .ThenByDescending(player => player.Rating)
            .ThenBy(player => player.PlayerId)
            .Select((player, index) => new PlayerRankRow(
                player.PlayerId,
                index + 1,
                pointsByPlayerId[player.PlayerId],
                stageId is null ? "overall" : $"stage:{stageId.Value}"))
            .ToArray();
    }
}
