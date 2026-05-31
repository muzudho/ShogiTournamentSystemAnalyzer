/*
 * ［アプリケーション　＞　モード　＞　シミュレーションコンテキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal sealed record class FinalStageModeSimulationContext(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    IReadOnlyList<Player> Players,
    FinalStageGroupingMode GroupingMode,
    IReadOnlyDictionary<string, FinalStageGroup>? GroupMap,
    IReadOnlyList<Player> AdditionalApexPlayers,
    AdditionalApexPlacementMode AdditionalApexPlacementMode,
    int EffectiveAdditionalApexCount,
    BoundaryRescueMode BoundaryRescueMode,
    int ApexCount,
    int InnovCount,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches)
    : AbstractSimulationContext(
        TournamentRuleSetMode,
        FirstPlayerWinRatePercent,
        FirstPlayerWinRateRating,
        Players,
        Matches)
{
    internal bool UsesFinalStageGrouping => GroupingMode == FinalStageGroupingMode.On;
}

