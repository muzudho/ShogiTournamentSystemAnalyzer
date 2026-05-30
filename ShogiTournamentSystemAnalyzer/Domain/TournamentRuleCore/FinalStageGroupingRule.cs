/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class FinalStageGroupingRule
{
    internal static string GetLabel(FinalStageGroupingMode mode)
    {
        return mode == FinalStageGroupingMode.On
            ? "On（Apex / Innov を使う）"
            : "Off（ニュートラルに扱う）";
    }
}

