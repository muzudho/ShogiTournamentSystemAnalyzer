internal sealed class FixedMatchResultResolver : IMatchResultResolver
{
    internal static readonly FixedMatchResultResolver Instance = new();

    public TournamentMatchRecord Resolve(TournamentState state, TournamentMatchRecord match, Random random)
    {
        return match;
    }
}
