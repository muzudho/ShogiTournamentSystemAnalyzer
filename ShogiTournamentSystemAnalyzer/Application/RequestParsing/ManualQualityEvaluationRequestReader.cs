/*
 * ［アプリケーション　＞　要求パース　＞　手入力品質評価要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static partial class ManualAnalysisRequestReader
{
    static bool TryReadQualityEvaluationRequest(
        RuleProfileAttributes ruleProfileAttributes,
        out AnalysisStepRequest stepRequest)
    {
        stepRequest = null!;
        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationRuleDefinition(players, ruleProfileAttributes, out var ruleDefinition)) return false;
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationInput(players, ruleDefinition, out var input)) return false;

        var executionOptions = TournamentQualityEvaluationExecutor.ReadTournamentQualityEvaluationExecutionOptions(input, ruleDefinition);
        var outputOptions = executionOptions.IsSweep
            ? TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualitySweepReportOutputOptions(ruleDefinition)
            : TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualityReportOutputOptions(ruleDefinition);

        stepRequest = !ruleProfileAttributes.UsesFinalStageGrouping
            ? new StandardQualityEvaluationRequest(ruleDefinition, input, executionOptions, outputOptions)
            : new FinalStageQualityEvaluationRequest(ruleDefinition, input, executionOptions, outputOptions);
        return true;
    }
}
