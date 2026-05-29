namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［最終順位］境界の writer 設定だ。
/// </summary>
internal sealed record class FinalRankingDataFileWriterSettings(RuleProfileMode RuleProfileMode)
{
    /// <summary>
    ///     <pre>
    /// ［最終順位テーブルタイプ・ファイルの名前］取得
    /// 
    ///     - TODO: 例えば、入力画面で［RuleProfileMode］を選ばせるんじゃなくて、［{大会ルール設定ファイル名}.json］を選択させるようにして、そこからルールプロファイルモードを決定するようにしたらいいんじゃないか（＾▽＾）？　どう思う（＾～＾）？
    ///     </pre>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    ///     <pre>
    /// ［スキーマ名］取得
    /// 
    ///     - TODO: ［{大会ルール設定ファイル名}.json］からスキーマ名を取得するようにしたらいいんじゃないか（＾▽＾）？　どう思う（＾～＾）？
    ///     </pre>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
