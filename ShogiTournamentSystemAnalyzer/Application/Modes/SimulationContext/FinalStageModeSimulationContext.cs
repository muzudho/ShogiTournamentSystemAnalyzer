/*
 * ［アプリケーション　＞　モード　＞　シミュレーションコンテキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal readonly record struct FinalStageModeSimulationContext(
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    IReadOnlyList<Player> Players,
    FinalStageGroupingMode GroupingMode,
    TournamentRuleSetMode TournamentRuleSetMode,
    IReadOnlyDictionary<string, FinalStageGroup>? GroupMap,
    IReadOnlyList<Player> AdditionalApexPlayers,
    AdditionalApexPlacementMode AdditionalApexPlacementMode,
    int EffectiveAdditionalApexCount,
    BoundaryRescueMode BoundaryRescueMode,
    int ApexCount,
    int InnovCount,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches)
{
    internal bool UsesFinalStageGrouping => GroupingMode == FinalStageGroupingMode.On;
}

