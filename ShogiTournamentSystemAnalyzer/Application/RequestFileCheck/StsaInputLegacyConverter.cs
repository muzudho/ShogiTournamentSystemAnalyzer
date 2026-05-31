/*
 * ［アプリケーション　＞　実行　＞　要求ファイルチェック　＞　STSA入力レガシー変換］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using static ShogiTournamentSystemAnalyzer.Application.Input.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.Input.StsaInputValueParser;

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

    static string ConvertStsaInputToLegacyInput(IReadOnlyList<string> rawLines, string fullPath, string formatName)
    {
        var sections = ParseStsaInputSections(rawLines, fullPath, formatName);
        var meta = ParseSectionKeyValues(GetRequiredSectionLines(sections, "Meta", fullPath, formatName), "Meta", fullPath, formatName);
        var analysisFlowMode = ParseAnalysisFlowMode(GetRequiredMetaValue(meta, "AnalysisFlowMode", fullPath, formatName), formatName);
        var ruleProfileMode = ParseRuleProfileMode(GetRequiredMetaValue(meta, "RuleProfileMode", fullPath, formatName), formatName);

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && (ruleProfileMode == RuleProfileMode.TournamentFramework
                || GetOptionalMetaValue(meta, "TournamentFrameworkMode") is not null)) return StsaSimulationLegacyConverter.ConvertTournamentFramework(meta, sections, fullPath, formatName);

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && ruleProfileMode == RuleProfileMode.Empty) return StsaSimulationLegacyConverter.ConvertEmpty(meta, sections, fullPath, formatName);

        if (analysisFlowMode != AnalysisFlowMode.QualityEvaluation) throw new OperationCanceledException($"{formatName} の最小対応は、現在のところ『品質評価』のみです。");

        return ruleProfileMode == RuleProfileMode.FinalStage
            ? StsaQualityEvaluationLegacyConverter.ConvertFinalStage(meta, sections, fullPath, formatName)
            : StsaQualityEvaluationLegacyConverter.ConvertStandard(meta, sections, fullPath, formatName);
    }

}