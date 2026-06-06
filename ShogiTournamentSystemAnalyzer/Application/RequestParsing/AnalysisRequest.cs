/*
 * ［アプリケーション　＞　要求パース　＞　分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using Match = ShogiTournamentSystemAnalyzer.Domain.Simulation.Match;

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

internal sealed record FinalStageSimulationRequest(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    IReadOnlyList<Player> Players,
    FinalStageGroupingMode GroupingMode,
    IReadOnlyDictionary<string, FinalStageGroup> GroupMap,
    IReadOnlyList<Player> AdditionalApexPlayers,
    AdditionalApexPlacementMode AdditionalApexPlacementMode,
    int EffectiveAdditionalApexCount,
    BoundaryRescueMode BoundaryRescueMode,
    int ApexCount,
    int InnovCount,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches,
    int? SimulationCount,
    string? OutputPath) : AnalysisStepRequest;

internal sealed record TournamentFrameworkSimulationRequest(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed,
    int? SimulationCount,
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    string? OutputPath) : AnalysisStepRequest;

internal sealed record EmptySimulationRequest(
    string? OutputPath) : AnalysisStepRequest;

internal sealed record StandardQualityEvaluationRequest(
    TournamentQualityEvaluationRuleDefinition RuleDefinition,
    TournamentQualityEvaluationInput Input,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;

internal sealed record DeferredStandardQualityEvaluationRequest(
    TournamentRuleSetMode TournamentRuleSetMode,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;

internal sealed record FinalStageQualityEvaluationRequest(
    TournamentQualityEvaluationRuleDefinition RuleDefinition,
    TournamentQualityEvaluationInput Input,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;

internal sealed record DeferredFinalStageQualityEvaluationRequest(
    VariableTop8Mode VariableTop8Mode,
    TournamentQualityEvaluationInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;
