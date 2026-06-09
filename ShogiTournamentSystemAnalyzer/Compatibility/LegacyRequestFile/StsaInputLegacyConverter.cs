/*
 * ［互換性　＞　レガシー要求ファイル　＞　STSA入力レガシー変換］
 */
namespace ShogiTournamentSystemAnalyzer.Compatibility.LegacyRequestFile;

using ShogiTournamentSystemAnalyzer.Compatibility.LegacyRuleProfile;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
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
        var flowModes = ReadFlowStepList(meta, fullPath, formatName);
        var flowSelection = BuildFlowSelection(flowModes);
        var ruleProfileAttributes = LegacyRuleProfileMapper.ParseAttributesFromLabel(GetRequiredMetaValue(meta, "RuleProfileMode", fullPath, formatName), formatName);

        if (flowModes.Length != 1)
        {
            throw new OperationCanceledException($"{formatName} の複数 AnalysisFlowSteps は、要求ファイルからの自動実行ではまだ未対応です。ステップ別入力セクションの仕様追加が必要です: {FormatAnalysisFlowSteps(flowModes)}");
        }

        if (flowSelection.RunsQualityEvaluationDomain
            && !CanRunQualityEvaluation(ruleProfileAttributes))
        {
            throw new OperationCanceledException($"{formatName} の QualityEvaluation では RuleProfileMode={LegacyRuleProfileMapper.FormatLabel(ruleProfileAttributes)} は未対応です。");
        }

        var promptPrefixLines = BuildPromptPrefixLines(flowSelection, ruleProfileAttributes);
        var analysisFlowMode = flowModes[0];

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && (ruleProfileAttributes.IsTournamentFrameworkProfile
                || GetOptionalMetaValue(meta, "TournamentFrameworkMode") is not null)) return StsaSimulationLegacyConverter.ConvertTournamentFramework(meta, sections, fullPath, formatName, promptPrefixLines);

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && ruleProfileAttributes.IsEmptyProfile) return StsaSimulationLegacyConverter.ConvertEmpty(meta, sections, fullPath, formatName, promptPrefixLines);

        if (analysisFlowMode == AnalysisFlowMode.Simulation)
        {
            return ruleProfileAttributes.IsFinalStageScheduledProfile
                ? StsaSimulationLegacyConverter.ConvertFinalStage(meta, sections, fullPath, formatName, promptPrefixLines)
                : StsaSimulationLegacyConverter.ConvertStandard(meta, sections, fullPath, formatName, promptPrefixLines);
        }

        return ruleProfileAttributes.IsFinalStageScheduledProfile
            ? StsaQualityEvaluationLegacyConverter.ConvertFinalStage(meta, sections, fullPath, formatName, promptPrefixLines)
            : StsaQualityEvaluationLegacyConverter.ConvertStandard(meta, sections, fullPath, formatName, promptPrefixLines);
    }

    static AnalysisFlowMode[] ReadFlowStepList(Dictionary<string, string> meta, string fullPath, string formatName)
    {
        var stepsValue = GetOptionalMetaValue(meta, "AnalysisFlowSteps");
        if (!string.IsNullOrWhiteSpace(stepsValue)) return ParseAnalysisFlowStepList(stepsValue, formatName);

        var modeValue = GetRequiredMetaValue(meta, "AnalysisFlowMode", fullPath, formatName);
        return new[] { ParseAnalysisFlowMode(modeValue, formatName) };
    }

    static AnalysisFlowSelection BuildFlowSelection(IReadOnlyList<AnalysisFlowMode> flowModes)
    {
        return AnalysisFlowSelection.FromFlags(
            flowModes.Contains(AnalysisFlowMode.Simulation),
            flowModes.Contains(AnalysisFlowMode.QualityEvaluation));
    }

    static string FormatAnalysisFlowSteps(IReadOnlyList<AnalysisFlowMode> flowModes)
    {
        return string.Join(",", flowModes.Select(step => step.ToString()));
    }

    static bool CanRunQualityEvaluation(RuleProfileAttributes ruleProfileAttributes)
    {
        return ruleProfileAttributes.IsStandardScheduledProfile || ruleProfileAttributes.IsFinalStageScheduledProfile;
    }


    static IReadOnlyList<string> BuildPromptPrefixLines(AnalysisFlowSelection flowSelection, RuleProfileAttributes ruleProfileAttributes)
    {
        return new[]
        {
            flowSelection.RunsSimulationDomain ? "2" : "1",
            flowSelection.RunsQualityEvaluationDomain ? "2" : "1",
            "1",
        }.Concat(RuleProfileAttributesToPromptLines(ruleProfileAttributes)).ToArray();
    }

    static IReadOnlyList<string> RuleProfileAttributesToPromptLines(RuleProfileAttributes ruleProfileAttributes)
    {
        return new[]
        {
            RuleProfileSimulationShapeToPromptNumber(ruleProfileAttributes.SimulationShape),
            BoolToPromptNumber(ruleProfileAttributes.UsesFinalStageGrouping),
            BoolToPromptNumber(ruleProfileAttributes.UsesAdditionalApexPlacement),
            BoolToPromptNumber(ruleProfileAttributes.UsesBoundaryRescue),
            BoolToPromptNumber(ruleProfileAttributes.UsesVariableTop8),
            TournamentRuleSetModeToPromptNumber(ruleProfileAttributes.RankingRuleSetMode),
            BoolToPromptNumber(ruleProfileAttributes.HasReferenceMatches),
            RuleProfilePairingSourceToPromptNumber(ruleProfileAttributes.PairingSource),
        };
    }

    static string RuleProfileSimulationShapeToPromptNumber(RuleProfileSimulationShape simulationShape)
    {
        return simulationShape switch
        {
            RuleProfileSimulationShape.ScheduledMatches => "1",
            RuleProfileSimulationShape.FinalStageGrouped => "2",
            RuleProfileSimulationShape.TournamentFramework => "3",
            RuleProfileSimulationShape.Empty => "4",
            _ => throw new InvalidOperationException($"未対応の SimulationShape です: {simulationShape}"),
        };
    }

    static string RuleProfilePairingSourceToPromptNumber(RuleProfilePairingSource pairingSource)
    {
        return pairingSource switch
        {
            RuleProfilePairingSource.None => "1",
            RuleProfilePairingSource.ScheduledMatches => "2",
            RuleProfilePairingSource.TournamentFramework => "3",
            _ => throw new InvalidOperationException($"未対応の PairingSource です: {pairingSource}"),
        };
    }

    static string TournamentRuleSetModeToPromptNumber(TournamentRuleSetMode tournamentRuleSetMode)
    {
        return tournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Neutral => "1",
            TournamentRuleSetMode.Twill => "2",
            TournamentRuleSetMode.TwillCommonOpponentWeighted => "3",
            _ => throw new InvalidOperationException($"未対応の RankingRuleSetMode です: {tournamentRuleSetMode}"),
        };
    }

    static string BoolToPromptNumber(bool value)
    {
        return value ? "2" : "1";
    }
}
