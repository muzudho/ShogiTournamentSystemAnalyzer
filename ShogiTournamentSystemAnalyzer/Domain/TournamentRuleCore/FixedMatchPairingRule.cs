/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal sealed class FixedMatchPairingRule : IPairingRule
{
    internal static readonly FixedMatchPairingRule Instance = new();

    public TournamentState Apply(TournamentState state, int stageId)
    {
        return state;
    }
}
