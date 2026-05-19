internal sealed class FixedMatchResultResolver : IMatchResultResolver
{
    internal static readonly FixedMatchResultResolver Instance = new();

    public TournamentMatchRecord Resolve(TournamentState state, TournamentMatchRecord match, Random random)
    {
        if (match.ResultType != MatchResultType.None)
        {
            return match;
        }

        var firstPlayer = state.Players.FirstOrDefault(player => player.PlayerId == match.FirstPlayerId);
        var secondPlayer = state.Players.FirstOrDefault(player => player.PlayerId == match.SecondPlayerId);
        if (firstPlayer == default || secondPlayer == default)
        {
            return match with { ResultType = MatchResultType.FirstPlayerWin };
        }

        var resultType = firstPlayer.Rating.CompareTo(secondPlayer.Rating) switch
        {
            > 0 => MatchResultType.FirstPlayerWin,
            < 0 => MatchResultType.SecondPlayerWin,
            _ => random.Next(2) == 0 ? MatchResultType.FirstPlayerWin : MatchResultType.SecondPlayerWin,
        };

        return match with { ResultType = resultType };
    }
}
