/*
 * ［大会品質評価フロー域　＞　ルールプロファイル属性］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

enum RuleProfileSimulationShape
{
    ScheduledMatches,
    FinalStageGrouped,
    TournamentFramework,
    Empty,
}

enum RuleProfilePairingSource
{
    None,
    ScheduledMatches,
    TournamentFramework,
}

internal readonly record struct RuleProfileAttributes(
    RuleProfileSimulationShape SimulationShape,
    bool UsesFinalStageGrouping,
    bool UsesAdditionalApexPlacement,
    bool UsesBoundaryRescue,
    bool UsesVariableTop8,
    TournamentRuleSetMode RankingRuleSetMode,
    bool HasReferenceMatches,
    RuleProfilePairingSource PairingSource)
{
    internal bool IsStandardScheduledProfile =>
        PairingSource == RuleProfilePairingSource.ScheduledMatches
        && SimulationShape == RuleProfileSimulationShape.ScheduledMatches
        && !UsesFinalStageGrouping;

    internal bool IsFinalStageScheduledProfile =>
        PairingSource == RuleProfilePairingSource.ScheduledMatches
        && (SimulationShape == RuleProfileSimulationShape.FinalStageGrouped
            || UsesFinalStageGrouping);

    internal bool IsTournamentFrameworkProfile =>
        SimulationShape == RuleProfileSimulationShape.TournamentFramework;

    internal bool IsEmptyProfile =>
        SimulationShape == RuleProfileSimulationShape.Empty;

    internal static RuleProfileAttributes CreateStandardScheduled(
        TournamentRuleSetMode rankingRuleSetMode = TournamentRuleSetMode.Neutral,
        bool hasReferenceMatches = false)
    {
        return new RuleProfileAttributes(
            RuleProfileSimulationShape.ScheduledMatches,
            UsesFinalStageGrouping: false,
            UsesAdditionalApexPlacement: false,
            UsesBoundaryRescue: false,
            UsesVariableTop8: false,
            rankingRuleSetMode,
            hasReferenceMatches,
            RuleProfilePairingSource.ScheduledMatches);
    }

    internal static RuleProfileAttributes CreateFinalStageGrouped(
        TournamentRuleSetMode rankingRuleSetMode = TournamentRuleSetMode.Neutral,
        bool usesAdditionalApexPlacement = true,
        bool usesBoundaryRescue = true,
        bool usesVariableTop8 = true,
        bool hasReferenceMatches = true)
    {
        return new RuleProfileAttributes(
            RuleProfileSimulationShape.FinalStageGrouped,
            UsesFinalStageGrouping: true,
            usesAdditionalApexPlacement,
            usesBoundaryRescue,
            usesVariableTop8,
            rankingRuleSetMode,
            hasReferenceMatches,
            RuleProfilePairingSource.ScheduledMatches);
    }

    internal static RuleProfileAttributes CreateTournamentFramework(
        TournamentRuleSetMode rankingRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        return new RuleProfileAttributes(
            RuleProfileSimulationShape.TournamentFramework,
            UsesFinalStageGrouping: false,
            UsesAdditionalApexPlacement: false,
            UsesBoundaryRescue: false,
            UsesVariableTop8: false,
            rankingRuleSetMode,
            false,
            RuleProfilePairingSource.TournamentFramework);
    }

    internal static RuleProfileAttributes CreateEmpty(
        TournamentRuleSetMode rankingRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        return new RuleProfileAttributes(
            RuleProfileSimulationShape.Empty,
            UsesFinalStageGrouping: false,
            UsesAdditionalApexPlacement: false,
            UsesBoundaryRescue: false,
            UsesVariableTop8: false,
            rankingRuleSetMode,
            false,
            RuleProfilePairingSource.None);
    }

    internal static RuleProfileAttributes FromCompatibilityLabel(
        RuleProfileMode ruleProfileMode,
        TournamentRuleSetMode rankingRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        return ruleProfileMode switch
        {
            RuleProfileMode.Standard => CreateStandardScheduled(rankingRuleSetMode),
            RuleProfileMode.FinalStage => CreateFinalStageGrouped(rankingRuleSetMode),
            RuleProfileMode.TournamentFramework => CreateTournamentFramework(rankingRuleSetMode),
            RuleProfileMode.Empty => CreateEmpty(rankingRuleSetMode),
            _ => throw new InvalidOperationException($"未対応のルールプロファイルモード: {ruleProfileMode}"),
        };
    }

    internal RuleProfileMode ToCompatibilityLabel()
    {
        return SimulationShape switch
        {
            RuleProfileSimulationShape.ScheduledMatches when UsesFinalStageGrouping => RuleProfileMode.FinalStage,
            RuleProfileSimulationShape.ScheduledMatches => RuleProfileMode.Standard,
            RuleProfileSimulationShape.FinalStageGrouped => RuleProfileMode.FinalStage,
            RuleProfileSimulationShape.TournamentFramework => RuleProfileMode.TournamentFramework,
            RuleProfileSimulationShape.Empty => RuleProfileMode.Empty,
            _ => throw new InvalidOperationException($"互換ルールプロファイルモードへ変換できません: {this}"),
        };
    }
}
