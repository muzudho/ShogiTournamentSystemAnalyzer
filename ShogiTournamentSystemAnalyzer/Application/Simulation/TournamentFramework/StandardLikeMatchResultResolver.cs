namespace ShogiTournamentSystemAnalyzer.Application.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// Resolves undecided matches with the standard-like win-rate model.
/// </summary>
sealed class StandardLikeMatchResultResolver(double firstPlayerWinRateRating) : IMatchResultResolver
{
    readonly double _firstPlayerWinRateRating = firstPlayerWinRateRating;

    public TournamentMatchRecord Resolve(TournamentState state, TournamentMatchRecord match, Random random)
    {
        if (match.ResultType != MatchResultType.None) return match;

        var playerMap = state.Players.ToDictionary(player => player.PlayerId);
        var firstPlayerEntry = playerMap[match.FirstPlayerId];
        var secondPlayerEntry = playerMap[match.SecondPlayerId];
        var firstPlayer = new Player(firstPlayerEntry.Name, firstPlayerEntry.Rating);
        var secondPlayer = new Player(secondPlayerEntry.Name, secondPlayerEntry.Rating);
        var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(firstPlayer, secondPlayer, _firstPlayerWinRateRating);
        var resultType = random.NextDouble() < firstPlayerWinProbability
            ? MatchResultType.FirstPlayerWin
            : MatchResultType.SecondPlayerWin;

        return match with { ResultType = resultType };
    }
}