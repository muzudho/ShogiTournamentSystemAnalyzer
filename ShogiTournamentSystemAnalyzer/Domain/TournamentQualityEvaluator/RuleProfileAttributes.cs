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
    internal static RuleProfileAttributes FromCompatibilityLabel(
        RuleProfileMode ruleProfileMode,
        TournamentRuleSetMode rankingRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        return ruleProfileMode switch
        {
            RuleProfileMode.Standard => new RuleProfileAttributes(
                RuleProfileSimulationShape.ScheduledMatches,
                UsesFinalStageGrouping: false,
                UsesAdditionalApexPlacement: false,
                UsesBoundaryRescue: false,
                UsesVariableTop8: false,
                rankingRuleSetMode,
                HasReferenceMatches: false,
                RuleProfilePairingSource.ScheduledMatches),
            RuleProfileMode.FinalStage => new RuleProfileAttributes(
                RuleProfileSimulationShape.FinalStageGrouped,
                UsesFinalStageGrouping: true,
                UsesAdditionalApexPlacement: true,
                UsesBoundaryRescue: true,
                UsesVariableTop8: true,
                rankingRuleSetMode,
                HasReferenceMatches: true,
                RuleProfilePairingSource.ScheduledMatches),
            RuleProfileMode.TournamentFramework => new RuleProfileAttributes(
                RuleProfileSimulationShape.TournamentFramework,
                UsesFinalStageGrouping: false,
                UsesAdditionalApexPlacement: false,
                UsesBoundaryRescue: false,
                UsesVariableTop8: false,
                rankingRuleSetMode,
                HasReferenceMatches: false,
                RuleProfilePairingSource.TournamentFramework),
            RuleProfileMode.Empty => new RuleProfileAttributes(
                RuleProfileSimulationShape.Empty,
                UsesFinalStageGrouping: false,
                UsesAdditionalApexPlacement: false,
                UsesBoundaryRescue: false,
                UsesVariableTop8: false,
                rankingRuleSetMode,
                HasReferenceMatches: false,
                RuleProfilePairingSource.None),
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
