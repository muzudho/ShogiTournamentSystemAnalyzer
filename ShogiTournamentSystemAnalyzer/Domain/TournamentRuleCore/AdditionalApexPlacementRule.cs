/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class AdditionalApexPlacementRule
{
    internal static int GetEffectiveAdditionalApexCount(int additionalApexCount, AdditionalApexPlacementMode placementMode)
    {
        return placementMode == AdditionalApexPlacementMode.On ? 0 : additionalApexCount;
    }

    internal static string GetLabel(AdditionalApexPlacementMode placementMode)
    {
        return placementMode == AdditionalApexPlacementMode.On
            ? "On（改善案A: 総合順位へ挿入しない）"
            : "Off（現行案: Innov より前に順位帯を確保する）";
    }
}

