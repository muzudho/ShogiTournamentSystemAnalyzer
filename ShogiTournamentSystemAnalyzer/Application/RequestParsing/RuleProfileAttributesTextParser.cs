/*
 * ［アプリケーション　＞　要求パース　＞　ルールプロファイル属性テキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputValueParser;

internal static class RuleProfileAttributesTextParser
{
    const string AttributeFormatName = "STSAInput/5";
    const string SectionName = "RuleProfileAttributes";

    internal static bool TryParse(
        IReadOnlyList<string> lines,
        string sourceName,
        out RuleProfileAttributes attributes,
        out string errorMessage)
    {
        attributes = default;
        errorMessage = string.Empty;

        try
        {
            var values = ReadAttributeValues(lines, sourceName);
            attributes = Parse(values, sourceName, SectionName);
            return true;
        }
        catch (OperationCanceledException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    internal static RuleProfileAttributes Parse(
        Dictionary<string, string> values,
        string sourceName,
        string sectionName)
    {
        var attributes = new RuleProfileAttributes(
            ParseRuleProfileSimulationShape(GetRequiredMetaValue(values, "SimulationShape", sourceName, AttributeFormatName), AttributeFormatName),
            ParseOnOffBool(GetRequiredMetaValue(values, "UsesFinalStageGrouping", sourceName, AttributeFormatName), "UsesFinalStageGrouping", AttributeFormatName),
            ParseOnOffBool(GetRequiredMetaValue(values, "UsesAdditionalApexPlacement", sourceName, AttributeFormatName), "UsesAdditionalApexPlacement", AttributeFormatName),
            ParseOnOffBool(GetRequiredMetaValue(values, "UsesBoundaryRescue", sourceName, AttributeFormatName), "UsesBoundaryRescue", AttributeFormatName),
            ParseOnOffBool(GetRequiredMetaValue(values, "UsesVariableTop8", sourceName, AttributeFormatName), "UsesVariableTop8", AttributeFormatName),
            ParseTournamentRuleSetModeValue(GetRequiredMetaValue(values, "RankingRuleSetMode", sourceName, AttributeFormatName), AttributeFormatName),
            ParseOnOffBool(GetRequiredMetaValue(values, "HasReferenceMatches", sourceName, AttributeFormatName), "HasReferenceMatches", AttributeFormatName),
            ParseRuleProfilePairingSource(GetRequiredMetaValue(values, "PairingSource", sourceName, AttributeFormatName), AttributeFormatName));

        if (!attributes.TryValidate(out var errorMessage))
        {
            throw new OperationCanceledException($"{AttributeFormatName} の {sectionName} セクションの属性の組み合わせが不正です: {errorMessage} ({sourceName})");
        }

        return attributes;
    }

    static Dictionary<string, string> ReadAttributeValues(IReadOnlyList<string> lines, string sourceName)
    {
        var hasFormatDeclaration = lines.Any(line => line.Trim().Equals($"#[Format] {AttributeFormatName}", StringComparison.OrdinalIgnoreCase));
        if (!hasFormatDeclaration)
        {
            return ParseSectionKeyValues(lines, SectionName, sourceName, AttributeFormatName);
        }

        var sections = ParseStsaInputSections(lines, sourceName, AttributeFormatName);
        return ParseSectionKeyValues(
            GetRequiredSectionLines(sections, SectionName, sourceName, AttributeFormatName),
            SectionName,
            sourceName,
            AttributeFormatName);
    }
}