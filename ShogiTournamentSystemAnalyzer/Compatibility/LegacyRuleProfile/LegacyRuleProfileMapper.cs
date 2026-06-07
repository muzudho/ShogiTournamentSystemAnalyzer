/*
 * ［互換性　＞　レガシールールプロファイル］
 */
namespace ShogiTournamentSystemAnalyzer.Compatibility.LegacyRuleProfile;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class LegacyRuleProfileMapper
{
    internal static LegacyRuleProfileMode ParseMode(string value, string formatName)
    {
        if (value.Equals("Empty", StringComparison.OrdinalIgnoreCase) || value == "4") return LegacyRuleProfileMode.Empty;

        if (value.Equals("TournamentFramework", StringComparison.OrdinalIgnoreCase) || value == "3") return LegacyRuleProfileMode.TournamentFramework;

        if (value.Equals("FinalStage", StringComparison.OrdinalIgnoreCase) || value == "2") return LegacyRuleProfileMode.FinalStage;

        if (value.Equals("Standard", StringComparison.OrdinalIgnoreCase) || value == "1") return LegacyRuleProfileMode.Standard;

        throw new OperationCanceledException($"{formatName} の RuleProfileMode の値が解釈できません: {value}");
    }

    internal static RuleProfileAttributes ParseAttributesFromLabel(string value, string formatName)
    {
        return FromCompatibilityLabel(ParseMode(value, formatName));
    }

    internal static RuleProfileAttributes FromCompatibilityLabel(
        LegacyRuleProfileMode mode,
        TournamentRuleSetMode rankingRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        return mode switch
        {
            LegacyRuleProfileMode.Standard => RuleProfileAttributes.CreateStandardScheduled(rankingRuleSetMode),
            LegacyRuleProfileMode.FinalStage => RuleProfileAttributes.CreateFinalStageGrouped(rankingRuleSetMode),
            LegacyRuleProfileMode.TournamentFramework => RuleProfileAttributes.CreateTournamentFramework(rankingRuleSetMode),
            LegacyRuleProfileMode.Empty => RuleProfileAttributes.CreateEmpty(rankingRuleSetMode),
            _ => throw new InvalidOperationException($"未対応のルールプロファイルモード: {mode}"),
        };
    }

    internal static LegacyRuleProfileMode ToCompatibilityLabel(RuleProfileAttributes attributes)
    {
        return attributes.SimulationShape switch
        {
            RuleProfileSimulationShape.ScheduledMatches when attributes.UsesFinalStageGrouping => LegacyRuleProfileMode.FinalStage,
            RuleProfileSimulationShape.ScheduledMatches => LegacyRuleProfileMode.Standard,
            RuleProfileSimulationShape.FinalStageGrouped => LegacyRuleProfileMode.FinalStage,
            RuleProfileSimulationShape.TournamentFramework => LegacyRuleProfileMode.TournamentFramework,
            RuleProfileSimulationShape.Empty => LegacyRuleProfileMode.Empty,
            _ => throw new InvalidOperationException($"互換ルールプロファイルモードへ変換できません: {attributes}"),
        };
    }

    internal static string FormatLabel(RuleProfileAttributes attributes)
    {
        return ToCompatibilityLabel(attributes).ToString();
    }
}
