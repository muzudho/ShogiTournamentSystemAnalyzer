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
        var ruleProfileMode = ParseRuleProfileMode(GetRequiredMetaValue(meta, "RuleProfileMode", fullPath, formatName), formatName);

        if (flowSelection.Steps.Count != 1)
        {
            throw new OperationCanceledException($"{formatName} の複数 AnalysisFlowSteps は、要求ファイルからの自動実行ではまだ未対応です。ステップ別入力セクションの仕様追加が必要です: {flowSelection.ToRequestFileValue()}");
        }

        if (flowSelection.RunsQualityEvaluation
            && (ruleProfileMode == RuleProfileMode.TournamentFramework || ruleProfileMode == RuleProfileMode.Empty))
        {
            throw new OperationCanceledException($"{formatName} の QualityEvaluation では RuleProfileMode={ruleProfileMode} は未対応です。");
        }

        var promptPrefixLines = BuildPromptPrefixLines(flowSelection, ruleProfileMode);
        var analysisFlowMode = flowSelection.Steps[0];

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && (ruleProfileMode == RuleProfileMode.TournamentFramework
                || GetOptionalMetaValue(meta, "TournamentFrameworkMode") is not null)) return StsaSimulationLegacyConverter.ConvertTournamentFramework(meta, sections, fullPath, formatName, promptPrefixLines);

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && ruleProfileMode == RuleProfileMode.Empty) return StsaSimulationLegacyConverter.ConvertEmpty(meta, sections, fullPath, formatName, promptPrefixLines);

        if (analysisFlowMode == AnalysisFlowMode.Simulation)
        {
            return ruleProfileMode == RuleProfileMode.FinalStage
                ? StsaSimulationLegacyConverter.ConvertFinalStage(meta, sections, fullPath, formatName, promptPrefixLines)
                : StsaSimulationLegacyConverter.ConvertStandard(meta, sections, fullPath, formatName, promptPrefixLines);
        }

        return ruleProfileMode == RuleProfileMode.FinalStage
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

    static IReadOnlyList<string> BuildPromptPrefixLines(AnalysisFlowSelection flowSelection, RuleProfileMode ruleProfileMode)
    {
        return new[]
        {
            flowSelection.RunsSimulation ? "2" : "1",
            flowSelection.RunsQualityEvaluation ? "2" : "1",
            RuleProfileModeToPromptNumber(ruleProfileMode),
        };
    }

    static string RuleProfileModeToPromptNumber(RuleProfileMode ruleProfileMode)
    {
        return ruleProfileMode switch
        {
            RuleProfileMode.Standard => "1",
            RuleProfileMode.FinalStage => "2",
            RuleProfileMode.TournamentFramework => "3",
            RuleProfileMode.Empty => "4",
            _ => throw new InvalidOperationException($"未対応のルールプロファイルモードです: {ruleProfileMode}"),
        };
    }

}