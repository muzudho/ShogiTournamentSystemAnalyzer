/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class TournamentRuleSetRule
{
    internal static string GetLabel(TournamentRuleSetMode mode)
    {
        return mode switch
        {
            TournamentRuleSetMode.Twill => "Twill（ツイル式トーナメント）",
            TournamentRuleSetMode.TwillCommonOpponentWeighted => "Twill+CommonOpp（共通相手比較の信頼度付き）",
            _ => "Neutral（ニュートラル）",
        };
    }
}

