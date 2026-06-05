/*
 * ［アプリケーション　＞　要求パース　＞　分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal sealed record AnalysisRequest(
    AnalysisFlowSelection FlowSelection,
    RuleProfileMode RuleProfileMode,
    IReadOnlyList<AnalysisStepRequest> Steps);

internal abstract record AnalysisStepRequest;

internal sealed record StandardSimulationRequest(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    IReadOnlyList<Player> AllPlayers,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches,
    int? SimulationCount,
    string? OutputPath) : AnalysisStepRequest;