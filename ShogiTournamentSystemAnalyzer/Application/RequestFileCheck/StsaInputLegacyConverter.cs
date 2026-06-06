/*
 * ［アプリケーション　＞　要求ファイルチェック　＞　STSA入力レガシー変換］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputValueParser;

internal static class StsaInputLegacyConverter
{
    internal static string ConvertStsaInput2ToLegacyInput(IReadOnlyList<string> rawLines, string fullPath)
    {
        return ConvertStsaInputToLegacyInput(rawLines, fullPath, "STSAInput/2");
    }

    internal static string ConvertStsaInput3ToLegacyInput(IReadOnlyList<string> rawLines, string fullPath)
    {
        return ConvertStsaInputToLegacyInput(rawLines, fullPath, "STSAInput/3");
    }

    internal static string ConvertStsaInput4ToLegacyInput(IReadOnlyList<string> rawLines, string fullPath)
    {
        return ConvertStsaInputToLegacyInput(rawLines, fullPath, "STSAInput/4");
    }

    static string ConvertStsaInputToLegacyInput(IReadOnlyList<string> rawLines, string fullPath, string formatName)
    {
        var sections = ParseStsaInputSections(rawLines, fullPath, formatName);
        var meta = ParseSectionKeyValues(GetRequiredSectionLines(sections, "Meta", fullPath, formatName), "Meta", fullPath, formatName);
        var flowSelection = ReadFlowSelection(meta, fullPath, formatName);
        var ruleProfileAttributes = ParseRuleProfileAttributesFromCompatibilityLabel(GetRequiredMetaValue(meta, "RuleProfileMode", fullPath, formatName), formatName);

        if (flowSelection.Steps.Count != 1)
        {
            throw new OperationCanceledException($"{formatName} の複数 AnalysisFlowSteps は、要求ファイルからの自動実行ではまだ未対応です。ステップ別入力セクションの仕様追加が必要です: {flowSelection.ToRequestFileValue()}");
        }

        if (flowSelection.RunsQualityEvaluation
            && !CanRunQualityEvaluation(ruleProfileAttributes))
        {
            throw new OperationCanceledException($"{formatName} の QualityEvaluation では RuleProfileMode={ruleProfileAttributes.ToCompatibilityLabel()} は未対応です。");
        }

        var promptPrefixLines = BuildPromptPrefixLines(flowSelection, ruleProfileAttributes);
        var analysisFlowMode = flowSelection.Steps[0];

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && (ruleProfileAttributes.SimulationShape == RuleProfileSimulationShape.TournamentFramework
                || GetOptionalMetaValue(meta, "TournamentFrameworkMode") is not null)) return StsaSimulationLegacyConverter.ConvertTournamentFramework(meta, sections, fullPath, formatName, promptPrefixLines);

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && ruleProfileAttributes.SimulationShape == RuleProfileSimulationShape.Empty) return StsaSimulationLegacyConverter.ConvertEmpty(meta, sections, fullPath, formatName, promptPrefixLines);

        if (analysisFlowMode == AnalysisFlowMode.Simulation)
        {
            return IsFinalStageScheduledProfile(ruleProfileAttributes)
                ? StsaSimulationLegacyConverter.ConvertFinalStage(meta, sections, fullPath, formatName, promptPrefixLines)
                : StsaSimulationLegacyConverter.ConvertStandard(meta, sections, fullPath, formatName, promptPrefixLines);
        }

        return IsFinalStageScheduledProfile(ruleProfileAttributes)
            ? StsaQualityEvaluationLegacyConverter.ConvertFinalStage(meta, sections, fullPath, formatName, promptPrefixLines)
            : StsaQualityEvaluationLegacyConverter.ConvertStandard(meta, sections, fullPath, formatName, promptPrefixLines);
    }

    static AnalysisFlowSelection ReadFlowSelection(Dictionary<string, string> meta, string fullPath, string formatName)
    {
        var stepsValue = GetOptionalMetaValue(meta, "AnalysisFlowSteps");
        if (!string.IsNullOrWhiteSpace(stepsValue)) return ParseAnalysisFlowSteps(stepsValue, formatName);

        var modeValue = GetRequiredMetaValue(meta, "AnalysisFlowMode", fullPath, formatName);
        return AnalysisFlowSelection.FromSingle(ParseAnalysisFlowMode(modeValue, formatName));
    }

    static bool CanRunQualityEvaluation(RuleProfileAttributes ruleProfileAttributes)
    {
        return IsStandardScheduledProfile(ruleProfileAttributes) || IsFinalStageScheduledProfile(ruleProfileAttributes);
    }

    static bool IsStandardScheduledProfile(RuleProfileAttributes ruleProfileAttributes)
    {
        return ruleProfileAttributes.PairingSource == RuleProfilePairingSource.ScheduledMatches
            && ruleProfileAttributes.SimulationShape == RuleProfileSimulationShape.ScheduledMatches
            && !ruleProfileAttributes.UsesFinalStageGrouping;
    }

    static bool IsFinalStageScheduledProfile(RuleProfileAttributes ruleProfileAttributes)
    {
        return ruleProfileAttributes.PairingSource == RuleProfilePairingSource.ScheduledMatches
            && (ruleProfileAttributes.SimulationShape == RuleProfileSimulationShape.FinalStageGrouped
                || ruleProfileAttributes.UsesFinalStageGrouping);
    }

    static IReadOnlyList<string> BuildPromptPrefixLines(AnalysisFlowSelection flowSelection, RuleProfileAttributes ruleProfileAttributes)
    {
        return new[]
        {
            flowSelection.RunsSimulation ? "2" : "1",
            flowSelection.RunsQualityEvaluation ? "2" : "1",
            RuleProfileAttributesToPromptNumber(ruleProfileAttributes),
        };
    }

    static string RuleProfileAttributesToPromptNumber(RuleProfileAttributes ruleProfileAttributes)
    {
        if (ruleProfileAttributes.SimulationShape == RuleProfileSimulationShape.TournamentFramework) return "3";
        if (ruleProfileAttributes.SimulationShape == RuleProfileSimulationShape.Empty) return "4";
        if (IsFinalStageScheduledProfile(ruleProfileAttributes)) return "2";
        if (IsStandardScheduledProfile(ruleProfileAttributes)) return "1";

        throw new InvalidOperationException($"未対応のルールプロファイル属性です: {ruleProfileAttributes}");
    }

}
