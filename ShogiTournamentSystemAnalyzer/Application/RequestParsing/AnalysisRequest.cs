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
    SimulationDomainRequest? SimulationDomainRequest,
    FinalRankingDomainRequest? FinalRankingDomainRequest,
    QualityEvaluationDomainRequest? QualityEvaluationDomainRequest)
{
    internal static AnalysisRequest FromAnalysisSteps(
        AnalysisFlowSelection flowSelection,
        IReadOnlyList<AnalysisStepRequest> stepRequests)
    {
        SimulationStepRequest? simulationStepRequest = null;
        AnalysisStepRequest? qualityEvaluationStepRequest = null;

        foreach (var stepRequest in stepRequests)
        {
            switch (stepRequest)
            {
                case SimulationStepRequest request:
                    if (simulationStepRequest is not null) throw new InvalidOperationException("シミュレーション域要求が重複しています。");
                    simulationStepRequest = request;
                    break;

                case QualityEvaluationStepRequest or DeferredQualityEvaluationStepRequest:
                    if (qualityEvaluationStepRequest is not null) throw new InvalidOperationException("大会品質評価域要求が重複しています。");
                    qualityEvaluationStepRequest = stepRequest;
                    break;

                default:
                    throw new InvalidOperationException($"未対応の分析要求です: {stepRequest.GetType().Name}");
            }
        }

        return new AnalysisRequest(
            flowSelection,
            simulationStepRequest is null ? null : new SimulationDomainRequest(simulationStepRequest),
            simulationStepRequest is null || !flowSelection.RunsFinalRankingDomain ? null : new FinalRankingDomainRequest(simulationStepRequest),
            qualityEvaluationStepRequest is null ? null : new QualityEvaluationDomainRequest(qualityEvaluationStepRequest));
    }

    internal IReadOnlyList<AnalysisStepRequest> GetExecutableAnalysisSteps()
    {
        var stepRequests = new List<AnalysisStepRequest>();
        if (SimulationDomainRequest is not null) stepRequests.Add(SimulationDomainRequest.StepRequest);
        if (QualityEvaluationDomainRequest is not null) stepRequests.Add(QualityEvaluationDomainRequest.StepRequest);
        return stepRequests;
    }

    internal RuleProfileAttributes GetPrimaryRuleProfileAttributes()
    {
        return SimulationDomainRequest?.StepRequest.GetRuleProfileAttributes()
            ?? QualityEvaluationDomainRequest?.StepRequest.GetRuleProfileAttributes()
            ?? throw new InvalidOperationException("分析要求に実行可能な大域要求がありません。");
    }
}

internal sealed record SimulationDomainRequest(SimulationStepRequest StepRequest);

internal sealed record FinalRankingDomainRequest(SimulationStepRequest SourceSimulationRequest);

internal sealed record QualityEvaluationDomainRequest(AnalysisStepRequest StepRequest);

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
    TournamentQualityEvaluationOutputOptions OutputOptions,
    TournamentQualityScoreRule ScoreRule) : AnalysisStepRequest;

internal sealed record DeferredQualityEvaluationStepRequest(
    RuleProfileAttributes RuleProfileAttributes,
    TournamentQualityEvaluationExecutionOptions ExecutionOptions,
    TournamentQualityEvaluationOutputOptions OutputOptions,
    DeferredQualityEvaluationOptions DeferredOptions,
    TournamentQualityScoreRule ScoreRule) : AnalysisStepRequest;

internal sealed record DeferredQualityEvaluationOptions(
    VariableTop8Mode VariableTop8Mode,
    TournamentQualityEvaluationInnovExpectedRankOffsetMode InnovExpectedRankOffsetMode);
