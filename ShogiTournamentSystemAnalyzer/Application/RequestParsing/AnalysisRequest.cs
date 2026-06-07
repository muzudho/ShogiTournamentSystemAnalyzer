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

/// <summary>
/// TODO: 旧仕様の［標準ルール］がまだ残ってんの（＾～＾）？
/// </summary>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="AllPlayers"></param>
/// <param name="Players"></param>
/// <param name="Matches"></param>
/// <param name="SimulationCount"></param>
/// <param name="OutputPath"></param>
internal sealed record StandardSimulationRequest(
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    IReadOnlyList<Player> AllPlayers,
    IReadOnlyList<Player> Players,
    IReadOnlyList<Match> Matches,
    int? SimulationCount,
    string? OutputPath) : AnalysisStepRequest;

/// <summary>
/// TODO: 旧仕様の［本戦ルール］がまだ残ってんの（＾～＾）？　要素に分解して、より柔軟な要求構造にしてくれだぜ（＾～＾）
/// </summary>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="Players"></param>
/// <param name="GroupingMode"></param>
/// <param name="GroupMap"></param>
/// <param name="AdditionalApexPlayers"></param>
/// <param name="AdditionalApexPlacementMode"></param>
/// <param name="EffectiveAdditionalApexCount"></param>
/// <param name="BoundaryRescueMode"></param>
/// <param name="ApexCount"></param>
/// <param name="InnovCount"></param>
/// <param name="Matches"></param>
/// <param name="ReferenceMatches"></param>
/// <param name="SimulationCount"></param>
/// <param name="OutputPath"></param>
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

/// <summary>
/// シミュレーション域で大会進行フレームワークを実行する要求。　要素に分解して、より柔軟な要求構造にしてくれだぜ（＾～＾）
/// </summary>
/// <param name="PlayersCsvPath"></param>
/// <param name="StagesCsvPath"></param>
/// <param name="TournamentMatchRecordsCsvPath"></param>
/// <param name="RuleFilePath"></param>
/// <param name="RandomSeed"></param>
/// <param name="SimulationCount"></param>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="OutputPath"></param>
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

/// <summary>
/// TODO: 旧仕様の［空ルール］がまだ残ってんの（＾～＾）？　要素に分解して、より柔軟な要求構造にしてくれだぜ（＾～＾）
/// </summary>
/// <param name="OutputPath"></param>
internal sealed record EmptySimulationRequest(
    string? OutputPath) : AnalysisStepRequest;

/// <summary>
/// TODO: 旧仕様の［標準ルール］がまだ残ってんの（＾～＾）？　要素に分解して、より柔軟な要求構造にしてくれだぜ（＾～＾）
/// </summary>
/// <param name="RuleDefinition"></param>
/// <param name="Input"></param>
/// <param name="ExecutionOptions"></param>
/// <param name="OutputOptions"></param>
internal sealed record StandardQualityEvaluationRequest(
    TournamentQualityEvaluationRuleDefinition RuleDefinition,
    TournamentQualityEvaluationInput Input,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;

/// <summary>
/// TODO: 旧仕様の［標準ルール］がまだ残ってんの（＾～＾）？　要素に分解して、より柔軟な要求構造にしてくれだぜ（＾～＾）
/// </summary>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="ExecutionOptions"></param>
/// <param name="OutputOptions"></param>
internal sealed record DeferredStandardQualityEvaluationRequest(
    TournamentRuleSetMode TournamentRuleSetMode,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;

/// <summary>
/// TODO: 旧仕様の［本戦ルール］がまだ残ってんの（＾～＾）？　要素に分解して、より柔軟な要求構造にしてくれだぜ（＾～＾）
/// </summary>
/// <param name="RuleDefinition"></param>
/// <param name="Input"></param>
/// <param name="ExecutionOptions"></param>
/// <param name="OutputOptions"></param>
internal sealed record FinalStageQualityEvaluationRequest(
    TournamentQualityEvaluationRuleDefinition RuleDefinition,
    TournamentQualityEvaluationInput Input,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;

/// <summary>
/// TODO: 旧仕様の［本戦ルール］がまだ残ってんの（＾～＾）？　要素に分解して、より柔軟な要求構造にしてくれだぜ（＾～＾）
/// </summary>
/// <param name="VariableTop8Mode"></param>
/// <param name="InnovExpectedRankOffsetMode"></param>
/// <param name="ExecutionOptions"></param>
/// <param name="OutputOptions"></param>
internal sealed record DeferredFinalStageQualityEvaluationRequest(
    VariableTop8Mode VariableTop8Mode,
    TournamentQualityEvaluationInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions) : AnalysisStepRequest;
