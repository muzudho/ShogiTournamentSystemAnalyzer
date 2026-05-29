namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［最終順位］境界の writer 設定だ。
/// </summary>
internal sealed record class FinalRankingDataFileWriterSettings(RuleProfileMode RuleProfileMode)
{
    internal string GetFinalRankingTableTypeFileName()
    {
        return RuleProfileMode switch
        {
            RuleProfileMode.Standard => "FinalRankingStandardTableType.json",
            RuleProfileMode.FinalStage => "FinalRankingFinalStageTableType.json",
            RuleProfileMode.TournamentFramework => "FinalRankingStandardTableType.json",
            _ => throw new InvalidOperationException($"未対応のルールプロファイルモード: {RuleProfileMode}")
        };
    }

    internal string GetSchemaName()
    {
        return RuleProfileMode switch
        {
            RuleProfileMode.Standard => "standardFinalRanking",
            RuleProfileMode.FinalStage => "finalStageFinalRanking",
            RuleProfileMode.TournamentFramework => "standardFinalRanking",
            _ => throw new InvalidOperationException($"未対応のルールプロファイルモード: {RuleProfileMode}")
        };
    }
}
