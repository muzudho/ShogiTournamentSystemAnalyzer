/*
 * ［アプリケーション　＞　要求パース　＞　手入力シミュレーション要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static partial class ManualAnalysisRequestReader
{
    internal static bool TryReadSimulationRequest(
        RuleProfileAttributes ruleProfileAttributes,
        out AnalysisStepRequest stepRequest)
    {
        stepRequest = null!;

        if (ruleProfileAttributes.IsStandardScheduledProfile)
        {
            stepRequest = ReadStandardSimulationRequest();
            return true;
        }

        if (ruleProfileAttributes.IsFinalStageScheduledProfile)
        {
            return TryReadFinalStageSimulationRequest(out stepRequest);
        }

        if (ruleProfileAttributes.IsTournamentFrameworkProfile)
        {
            stepRequest = ReadTournamentFrameworkSimulationRequest();
            return true;
        }

        if (ruleProfileAttributes.IsEmptyProfile)
        {
            stepRequest = ReadEmptySimulationRequest();
            return true;
        }

        return false;
    }

    static StandardSimulationRequest ReadStandardSimulationRequest()
    {
        var context = SimulationModeInputReaders.ReadStandardModeContext();
        return new StandardSimulationRequest(
            context.TournamentRuleSetMode,
            context.FirstPlayerWinRatePercent,
            context.AllPlayers,
            context.Players,
            context.Matches,
            ReadSimulationCountIfNeeded(context.Matches.Count, "標準"),
            null);
    }

    static bool TryReadFinalStageSimulationRequest(out AnalysisStepRequest stepRequest)
    {
        stepRequest = null!;
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
        return true;
    }

    static TournamentFrameworkSimulationRequest ReadTournamentFrameworkSimulationRequest()
    {
        var context = SimulationModeInputReaders.ReadTournamentFrameworkModeContext();
        return new TournamentFrameworkSimulationRequest(
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

    static EmptySimulationRequest ReadEmptySimulationRequest()
    {
        var outputPath = ConsoleInputReaders.ReadOptionalFilePath("空ルール結果CSVの出力先パスまたはフォルダーパスを入力してください（省略可）: ");
        return new EmptySimulationRequest(outputPath);
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
