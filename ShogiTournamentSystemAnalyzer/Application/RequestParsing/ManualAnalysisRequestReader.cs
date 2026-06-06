/*
 * ［アプリケーション　＞　要求パース　＞　手入力分析要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ManualAnalysisRequestReader
{
    internal static bool TryRead(
        AnalysisFlowSelection analysisFlowSelection,
        RuleProfileMode ruleProfileMode,
        out AnalysisRequest? analysisRequest)
    {
        analysisRequest = null;
        if (analysisFlowSelection.Steps.Count != 1) return false;

        AnalysisStepRequest stepRequest;
        if (analysisFlowSelection.Steps[0] == AnalysisFlowMode.Simulation && ruleProfileMode == RuleProfileMode.Standard)
        {
            var context = SimulationModeInputReaders.ReadStandardModeContext();
            stepRequest = new StandardSimulationRequest(
                context.TournamentRuleSetMode,
                context.FirstPlayerWinRatePercent,
                context.AllPlayers,
                context.Players,
                context.Matches,
                ReadSimulationCountIfNeeded(context.Matches.Count, "標準"),
                null);
        }
        else if (analysisFlowSelection.Steps[0] == AnalysisFlowMode.Simulation && ruleProfileMode == RuleProfileMode.FinalStage)
        {
            if (!SimulationModeInputReaders.TryReadFinalStageModeContext(out var context) || context is null) return false;

            stepRequest = new FinalStageSimulationRequest(
                context.TournamentRuleSetMode,
                context.FirstPlayerWinRatePercent,
                context.Players,
                context.GroupingMode,
                context.GroupMap!,
                context.AdditionalApexPlayers,
                context.AdditionalApexPlacementMode,
                context.EffectiveAdditionalApexCount,
                context.BoundaryRescueMode,
                context.ApexCount,
                context.InnovCount,
                context.Matches,
                context.ReferenceMatches,
                ReadSimulationCountIfNeeded(context.Matches.Count, "本戦"),
                null);
        }
        else if (analysisFlowSelection.Steps[0] == AnalysisFlowMode.Simulation && ruleProfileMode == RuleProfileMode.TournamentFramework)
        {
            var context = SimulationModeInputReaders.ReadTournamentFrameworkModeContext();
            stepRequest = new TournamentFrameworkSimulationRequest(
                context.PlayersCsvPath,
                context.StagesCsvPath,
                context.TournamentMatchRecordsCsvPath,
                context.RuleFilePath,
                context.RandomSeed,
                context.SimulationCount,
                context.TournamentRuleSetMode,
                context.FirstPlayerWinRatePercent,
                context.OutputPath);
        }
        else if (analysisFlowSelection.Steps[0] == AnalysisFlowMode.Simulation && ruleProfileMode == RuleProfileMode.Empty)
        {
            var outputPath = ConsoleInputReaders.ReadOptionalFilePath("空ルール結果CSVの出力先パスまたはフォルダーパスを入力してください（省略可）: ");
            stepRequest = new EmptySimulationRequest(outputPath);
        }
        else if (analysisFlowSelection.Steps[0] == AnalysisFlowMode.QualityEvaluation
            && (ruleProfileMode == RuleProfileMode.Standard || ruleProfileMode == RuleProfileMode.FinalStage))
        {
            if (!TryReadQualityEvaluationRequest(ruleProfileMode, out stepRequest)) return false;
        }
        else
        {
            return false;
        }

        analysisRequest = new AnalysisRequest(
            analysisFlowSelection,
            ruleProfileMode,
            new[] { stepRequest });
        return true;
    }

    static bool TryReadQualityEvaluationRequest(
        RuleProfileMode ruleProfileMode,
        out AnalysisStepRequest stepRequest)
    {
        stepRequest = null!;

        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationRuleDefinition(players, ruleProfileMode, out var ruleDefinition)) return false;
        if (!TournamentQualityEvaluationInputReader.TryReadQualityEvaluationInput(players, ruleDefinition, out var input)) return false;

        var executionOptions = TournamentQualityEvaluationExecutor.ReadTournamentQualityEvaluationExecutionOptions(input, ruleDefinition);
        var outputOptions = executionOptions.IsSweep
            ? TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualitySweepReportOutputOptions(ruleDefinition)
            : TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualityReportOutputOptions(ruleDefinition);

        stepRequest = ruleProfileMode == RuleProfileMode.Standard
            ? new StandardQualityEvaluationRequest(ruleDefinition, input, executionOptions, outputOptions)
            : new FinalStageQualityEvaluationRequest(ruleDefinition, input, executionOptions, outputOptions);
        return true;
    }

    static int? ReadSimulationCountIfNeeded(int matchCount, string modeLabel)
    {
        if (matchCount <= 20) return null;

        const int defaultSimulationCount = 200_000;
        var simulationCount = ConsolePromptReaders.ReadIntWithDefault(
            $"局数が多いため{modeLabel}シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
            defaultSimulationCount,
            min: 1);

        Console.WriteLine();
        return simulationCount;
    }
}