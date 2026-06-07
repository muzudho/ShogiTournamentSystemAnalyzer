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
    /// <summary>
    /// ［要求ファイル］から読んだ、実行可能な具体要求のリスト構造。
    /// </summary>
    IReadOnlyList<AnalysisStepRequest> StepRequests);

internal abstract record AnalysisStepRequest;

internal sealed record SimulationStepRequest(
    RuleProfileAttributes RuleProfileAttributes,
    double FirstPlayerWinRatePercent,
    ScheduledMatchesSimulationInput? ScheduledMatchesInput,
    FinalStageGroupingRequest? FinalStageGrouping,
    AdditionalApexPlacementRequest? AdditionalApexPlacement,
    BoundaryRescueRequest? BoundaryRescue,
    TournamentFrameworkSimulationInput? TournamentFrameworkInput,
    int? SimulationCount,
    string? OutputPath) : AnalysisStepRequest;

internal sealed record ScheduledMatchesSimulationInput(
    TournamentRuleSetMode TournamentRuleSetMode,
    IReadOnlyList<Player> AllPlayers,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches,
    IReadOnlyList<Match> ReferenceMatches);

internal sealed record FinalStageGroupingRequest(
    FinalStageGroupingMode GroupingMode,
    IReadOnlyDictionary<string, FinalStageGroup> GroupMap,
    int ApexCount,
    int InnovCount);

internal sealed record AdditionalApexPlacementRequest(
    IReadOnlyList<Player> AdditionalApexPlayers,
    AdditionalApexPlacementMode AdditionalApexPlacementMode,
    int EffectiveAdditionalApexCount);

internal sealed record BoundaryRescueRequest(
    BoundaryRescueMode BoundaryRescueMode);

internal sealed record TournamentFrameworkSimulationInput(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed,
    TournamentRuleSetMode TournamentRuleSetMode);

internal sealed record QualityEvaluationStepRequest(
    RuleProfileAttributes RuleProfileAttributes,
    TournamentQualityEvaluationRuleDefinition RuleDefinition,
    TournamentQualityEvaluationInput Input,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;

internal sealed record DeferredQualityEvaluationStepRequest(
    RuleProfileAttributes RuleProfileAttributes,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions,
    DeferredQualityEvaluationOptions DeferredOptions) : AnalysisStepRequest;

internal sealed record DeferredQualityEvaluationOptions(
    VariableTop8Mode VariableTop8Mode,
    TournamentQualityEvaluationInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode);