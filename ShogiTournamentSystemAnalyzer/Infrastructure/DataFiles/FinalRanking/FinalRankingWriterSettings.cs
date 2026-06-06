namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

enum FinalRankingTableProfile
{
    Standard,
    FinalStage,
}

/// <summary>
/// ［最終順位］境界の writer 設定だ。
/// </summary>
internal sealed record class FinalRankingDataFileWriterSettings(FinalRankingTableProfile TableProfile)
{
    internal FinalRankingDataFileWriterSettings(RuleProfileMode ruleProfileMode)
        : this(ToFinalRankingTableProfile(RuleProfileAttributes.FromCompatibilityLabel(ruleProfileMode)))
    {
    }

    internal FinalRankingDataFileWriterSettings(RuleProfileAttributes ruleProfileAttributes)
        : this(ToFinalRankingTableProfile(ruleProfileAttributes))
    {
    }

    internal string GetFinalRankingTableTypeFileName()
    {
        return TableProfile switch
        {
            FinalRankingTableProfile.Standard => "FinalRankingStandardTableType.json",
            FinalRankingTableProfile.FinalStage => "FinalRankingFinalStageTableType.json",
            _ => throw new InvalidOperationException($"未対応の最終順位表プロファイル: {TableProfile}")
        };
    }

    internal string GetSchemaName()
    {
        return TableProfile switch
        {
            FinalRankingTableProfile.Standard => "standardFinalRanking",
            FinalRankingTableProfile.FinalStage => "finalStageFinalRanking",
            _ => throw new InvalidOperationException($"未対応の最終順位表プロファイル: {TableProfile}")
        };
    }

    static FinalRankingTableProfile ToFinalRankingTableProfile(RuleProfileAttributes ruleProfileAttributes)
    {
        return ruleProfileAttributes.UsesFinalStageGrouping
            ? FinalRankingTableProfile.FinalStage
            : FinalRankingTableProfile.Standard;
    }
}
