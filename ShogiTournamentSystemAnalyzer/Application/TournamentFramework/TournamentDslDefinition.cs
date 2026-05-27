/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

sealed record class TournamentDslDefinition(
    string TimeAxis,
    string DefaultMatchResultResolver,
    IReadOnlyList<StageEntry> Stages,
    IReadOnlyList<string> FlowSteps,
    IReadOnlyDictionary<int, string> PairingRuleNames,
    string OverallRankingRuleName,
    string TerminationRuleName);
