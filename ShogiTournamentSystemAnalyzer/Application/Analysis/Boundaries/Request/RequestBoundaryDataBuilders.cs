/*
 * ［分析　＞　境界　＞　要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Helpers;

using ShogiTournamentSystemAnalyzer.Application.Modes;
using ShogiTournamentSystemAnalyzer.Application.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.Request.PlayerList;
using ShogiTournamentSystemAnalyzer.Domain.Request.RankingSettings;
using ShogiTournamentSystemAnalyzer.Domain.Request.TournamentRule;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// 境界データビルダー
/// </summary>
internal static partial class BoundaryDataBuilders
{
    /// <summary>
    /// ［大会ルール］組立
    /// </summary>
    /// <param name="context"></param>
    /// <param name="dslDefinition"></param>
    /// <returns></returns>
    internal static TournamentRuleData BuildTournamentRuleBoundaryData(TournamentFrameworkModeContext context, TournamentDslDefinition? dslDefinition)
    {
        return new TournamentRuleData(
            RuleProfileMode.TournamentFramework,
            context.TournamentRuleSetMode,
            context.RuleFilePath,
            context.FirstPlayerWinRatePercent,
            context.RandomSeed,
            dslDefinition is null
                ? "大会進行フレームワークの大会ルールデータ"
                : "大会進行フレームワークの大会ルールデータ（DSL読込あり）");
    }

    /// <summary>
    /// ［選手一覧］組立
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    internal static PlayerListData BuildPlayerListBoundaryData(IReadOnlyList<PlayerEntry> players)
    {
        return new PlayerListData(players);
    }

    /// <summary>
    /// ［順位設定］組立
    /// </summary>
    /// <param name="tournamentRuleData"></param>
    /// <returns></returns>
    internal static RankingSettingsData BuildRankingSettingsBoundaryData(TournamentRuleData tournamentRuleData)
    {
        return new RankingSettingsData(
            tournamentRuleData.TournamentRuleSetMode ?? TournamentRuleSetMode.Neutral,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位設定データ");
    }
}