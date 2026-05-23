/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal readonly record struct TournamentQualityEvaluationRuleDefinition(
    FinalStageGroupingMode GroupingMode,
    TournamentRuleSetMode TournamentRuleSetMode,
    IReadOnlyDictionary<string, FinalStageGroup>? GroupMap,
    IReadOnlyList<Player> AdditionalApexPlayers,
    AdditionalApexPlacementMode AdditionalApexPlacementMode,
    int EffectiveAdditionalApexCount,
    BoundaryRescueMode BoundaryRescueMode,
    VariableTop8Mode VariableTop8Mode,
    int PromotedInnovCount)
{
    internal bool UsesFinalStageGrouping => GroupingMode == FinalStageGroupingMode.On;
}

