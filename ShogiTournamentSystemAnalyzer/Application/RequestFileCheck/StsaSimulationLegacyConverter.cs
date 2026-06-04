/*
 * ［アプリケーション　＞　要求ファイルチェック　＞　STSAシミュレーションレガシー変換］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputValueParser;

internal static class StsaSimulationLegacyConverter
{
    internal static string ConvertTournamentFramework(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName,
        IReadOnlyList<string> promptPrefixLines)
    {
        var inputs = sections.TryGetValue("Inputs", out var inputLines)
            ? ParseSectionKeyValues(inputLines, "Inputs", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var playersCsvPath = GetRequiredMetaValue(inputs, "PlayersCsvPath", fullPath, formatName);
        var stagesCsvPath = GetRequiredMetaValue(inputs, "StagesCsvPath", fullPath, formatName);
        var tournamentMatchRecordsCsvPath = GetRequiredMetaValue(inputs, "TournamentMatchRecordsCsvPath", fullPath, formatName);
        var ruleFilePath = GetOptionalMetaValue(inputs, "RuleFilePath")
            ?? GetOptionalMetaValue(meta, "RuleFilePath")
            ?? string.Empty;
        var firstPlayerWinRatePercent = GetOptionalMetaValue(meta, "FirstPlayerWinRatePercent") ?? string.Empty;
        var tournamentRuleSetMode = ParseTournamentRuleSetSelection(GetOptionalMetaValue(meta, "TournamentRuleSetMode") ?? "1", formatName);
        var randomSeed = GetOptionalMetaValue(meta, "RandomSeed") ?? string.Empty;
        var simulationCount = GetOptionalMetaValue(meta, "SimulationCount") ?? string.Empty;
        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var outputPath = GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath")
            ?? string.Empty;

        var legacyLines = new List<string>(promptPrefixLines)
        {
            firstPlayerWinRatePercent,
            tournamentRuleSetMode,
            playersCsvPath,
            stagesCsvPath,
            tournamentMatchRecordsCsvPath,
            ruleFilePath,
            randomSeed,
            simulationCount,
            outputPath,
        };

        return string.Join(Environment.NewLine, legacyLines);
    }

    internal static string ConvertEmpty(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName,
        IReadOnlyList<string> promptPrefixLines)
    {
        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var outputPath = GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath")
            ?? string.Empty;

        var legacyLines = new List<string>(promptPrefixLines)
        {
            outputPath,
        };

        return string.Join(Environment.NewLine, legacyLines);
    }

}