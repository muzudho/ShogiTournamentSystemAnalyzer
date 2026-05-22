internal static class TournamentQualityEvaluationInnovExpectedRankOffsetRule
{
    internal static string GetLabel(TournamentQualityEvaluationInnovExpectedRankOffsetMode mode)
    {
        return mode switch
        {
            TournamentQualityEvaluationInnovExpectedRankOffsetMode.On => "On: Innov の比較基準順位を本戦不出場Apex人数+1ぶん後ろへずらす",
            _ => "Off: 使わない",
        };
    }

    internal static int GetComparisonRankOffset(int effectiveAdditionalApexCount, TournamentQualityEvaluationInnovExpectedRankOffsetMode mode)
    {
        return mode == TournamentQualityEvaluationInnovExpectedRankOffsetMode.On
            ? effectiveAdditionalApexCount + 1
            : 0;
    }
}
