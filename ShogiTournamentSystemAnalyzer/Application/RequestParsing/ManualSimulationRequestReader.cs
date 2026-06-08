/*
 * ［アプリケーション　＞　要求パース　＞　手入力シミュレーション要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Application.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;
using Match = ShogiTournamentSystemAnalyzer.Domain.Simulation.Match;

internal static partial class ManualAnalysisRequestReader
{
    internal static bool TryReadSimulationRequest(
        RuleProfileAttributes ruleProfileAttributes,
        out AnalysisStepRequest stepRequest)
    {
        stepRequest = null!;

        if (ruleProfileAttributes.IsStandardScheduledProfile)
        {
            stepRequest = ReadScheduledMatchesSimulationRequest(ruleProfileAttributes);
            return true;
        }

        if (ruleProfileAttributes.IsFinalStageScheduledProfile)
        {
            return TryReadFinalStageGroupedSimulationRequest(ruleProfileAttributes, out stepRequest);
        }

        if (ruleProfileAttributes.IsTournamentFrameworkProfile)
        {
            stepRequest = ReadTournamentFrameworkSimulationStepRequest(ruleProfileAttributes);
            return true;
        }

        if (ruleProfileAttributes.IsEmptyProfile)
        {
            stepRequest = ReadEmptySimulationStepRequest(ruleProfileAttributes);
            return true;
        }

        return false;
    }

    static SimulationStepRequest ReadScheduledMatchesSimulationRequest(RuleProfileAttributes ruleProfileAttributes)
    {
        var context = SimulationModeInputReaders.ReadStandardModeContext();
        return new SimulationStepRequest(
            ruleProfileAttributes,
            context.FirstPlayerWinRatePercent,
            new ScheduledMatchesSimulationInput(
                context.TournamentRuleSetMode,
                context.AllPlayers,
                context.Players,
                context.Matches,
                Array.Empty<Match>()),
            null,
            null,
            null,
            null,
            ReadSimulationCountIfNeeded(context.Matches.Count, "標準"),
            null);
    }

    static bool TryReadFinalStageGroupedSimulationRequest(RuleProfileAttributes ruleProfileAttributes, out AnalysisStepRequest stepRequest)
    {
        stepRequest = null!;
        if (!SimulationModeInputReaders.TryReadFinalStageModeContext(out var context) || context is null) return false;

        stepRequest = new SimulationStepRequest(
            ruleProfileAttributes,
            context.FirstPlayerWinRatePercent,
            new ScheduledMatchesSimulationInput(
                context.TournamentRuleSetMode,
                context.Players,
                context.Players,
                context.Matches,
                context.ReferenceMatches),
            new FinalStageGroupingRequest(
                context.GroupingMode,
                context.GroupMap!,
                context.ApexCount,
                context.InnovCount),
            new AdditionalApexPlacementRequest(
                context.AdditionalApexPlayers,
                context.AdditionalApexPlacementMode,
                context.EffectiveAdditionalApexCount),
            new BoundaryRescueRequest(context.BoundaryRescueMode),
            null,
            ReadSimulationCountIfNeeded(context.Matches.Count, "本戦"),
            null);
        return true;
    }

    static SimulationStepRequest ReadTournamentFrameworkSimulationStepRequest(RuleProfileAttributes ruleProfileAttributes)
    {
        var context = SimulationModeInputReaders.ReadTournamentFrameworkModeContext();
        return new SimulationStepRequest(
            ruleProfileAttributes,
            context.FirstPlayerWinRatePercent,
            null,
            null,
            null,
            null,
            new TournamentFrameworkSimulationInput(
                context.PlayersCsvPath,
                context.StagesCsvPath,
                context.TournamentMatchRecordsCsvPath,
                context.RuleFilePath,
                context.RandomSeed,
                context.TournamentRuleSetMode),
            context.SimulationCount,
            context.OutputPath);
    }

    static SimulationStepRequest ReadEmptySimulationStepRequest(RuleProfileAttributes ruleProfileAttributes)
    {
        var outputPath = ConsoleInputReaders.ReadOptionalFilePath("空ルール結果CSVの出力先パスまたはフォルダーパスを入力してください（省略可）: ");
        return new SimulationStepRequest(
            ruleProfileAttributes,
            51.0,
            null,
            null,
            null,
            null,
            null,
            null,
            outputPath);
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