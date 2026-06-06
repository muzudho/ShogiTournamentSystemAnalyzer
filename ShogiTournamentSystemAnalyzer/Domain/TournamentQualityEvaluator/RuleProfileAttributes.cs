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

    internal bool TryValidate(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (SimulationShape == RuleProfileSimulationShape.ScheduledMatches)
        {
            if (PairingSource != RuleProfilePairingSource.ScheduledMatches)
            {
                errorMessage = "SimulationShape=ScheduledMatches では PairingSource=ScheduledMatches を指定してください。";
                return false;
            }

            if (!UsesFinalStageGrouping && (UsesAdditionalApexPlacement || UsesBoundaryRescue || UsesVariableTop8))
            {
                errorMessage = "UsesFinalStageGrouping=Off では UsesAdditionalApexPlacement / UsesBoundaryRescue / UsesVariableTop8 は Off にしてください。";
                return false;
            }

            return true;
        }

        if (SimulationShape == RuleProfileSimulationShape.FinalStageGrouped)
        {
            if (PairingSource != RuleProfilePairingSource.ScheduledMatches)
            {
                errorMessage = "SimulationShape=FinalStageGrouped では PairingSource=ScheduledMatches を指定してください。";
                return false;
            }

            if (!UsesFinalStageGrouping)
            {
                errorMessage = "SimulationShape=FinalStageGrouped では UsesFinalStageGrouping=On を指定してください。";
                return false;
            }

            return true;
        }

        if (SimulationShape == RuleProfileSimulationShape.TournamentFramework)
        {
            if (PairingSource != RuleProfilePairingSource.TournamentFramework)
            {
                errorMessage = "SimulationShape=TournamentFramework では PairingSource=TournamentFramework を指定してください。";
                return false;
            }

            if (UsesFinalStageGrouping || UsesAdditionalApexPlacement || UsesBoundaryRescue || UsesVariableTop8 || HasReferenceMatches)
            {
                errorMessage = "SimulationShape=TournamentFramework では本戦用属性と HasReferenceMatches は Off にしてください。";
                return false;
            }

            return true;
        }

        if (SimulationShape == RuleProfileSimulationShape.Empty)
        {
            if (PairingSource != RuleProfilePairingSource.None)
            {
                errorMessage = "SimulationShape=Empty では PairingSource=None を指定してください。";
                return false;
            }

            if (UsesFinalStageGrouping || UsesAdditionalApexPlacement || UsesBoundaryRescue || UsesVariableTop8 || HasReferenceMatches)
            {
                errorMessage = "SimulationShape=Empty では本戦用属性と HasReferenceMatches は Off にしてください。";
                return false;
            }

            return true;
        }

        errorMessage = $"未対応の SimulationShape です: {SimulationShape}";
        return false;
    }

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
