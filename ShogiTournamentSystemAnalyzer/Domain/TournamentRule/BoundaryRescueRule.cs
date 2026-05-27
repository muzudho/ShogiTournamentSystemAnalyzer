/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static class BoundaryRescueRule
{
    internal static string GetLabel(BoundaryRescueMode boundaryRescueMode)
    {
        return boundaryRescueMode == BoundaryRescueMode.On
            ? "On（境界救済戦あり）"
            : "Off（境界救済戦なし）";
    }
}

