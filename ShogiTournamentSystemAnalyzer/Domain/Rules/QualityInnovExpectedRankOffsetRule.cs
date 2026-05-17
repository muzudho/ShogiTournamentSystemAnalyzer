internal static class QualityInnovExpectedRankOffsetRule
{
    internal static string GetLabel(QualityInnovExpectedRankOffsetMode mode)
    {
        return mode switch
        {
            QualityInnovExpectedRankOffsetMode.On => "On: Innov の比較基準順位を本戦不出場Apex人数+1ぶん後ろへずらす",
            _ => "Off: 使わない",
        };
    }

    internal static int GetComparisonRankOffset(int effectiveAdditionalApexCount, QualityInnovExpectedRankOffsetMode mode)
    {
        return mode == QualityInnovExpectedRankOffsetMode.On
            ? effectiveAdditionalApexCount + 1
            : 0;
    }
}
