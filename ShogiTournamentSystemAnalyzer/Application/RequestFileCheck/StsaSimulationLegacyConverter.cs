/*
 * ［アプリケーション　＞　要求ファイルチェック　＞　STSAシミュレーションレガシー変換］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.LegacyInputLineBuilder;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputValueParser;

internal static class StsaSimulationLegacyConverter
{
    const int ExactCalculationMatchThreshold = 20;

    internal static string ConvertStandard(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName,
        IReadOnlyList<string> promptPrefixLines)
    {
        var playersLines = GetRequiredSectionLines(sections, "PlayersCsv", fullPath, formatName);
        var matchesLines = GetRequiredSectionLines(sections, "MatchesInput", fullPath, formatName);
        var players = ParsePlayersForPromptShape(playersLines, fullPath, formatName);
        var matches = ParseMatchesForPromptShape(matchesLines, players, "MatchesInput", fullPath, formatName);
        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var outputPath = GetOptionalMetaValue(output, "SummaryOutputPath")
            ?? GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "SummaryOutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath")
            ?? string.Empty;

        var legacyLines = new List<string>(promptPrefixLines)
        {
            ParseTournamentRuleSetSelection(GetRequiredMetaValue(meta, "TournamentRuleSetMode", fullPath, formatName), formatName),
            GetRequiredMetaValue(meta, "FirstPlayerWinRatePercent", fullPath, formatName),
        };

        AppendDelimitedSection(legacyLines, playersLines);
        AppendEndTerminatedSection(legacyLines, matchesLines);
        AppendSimulationCountIfApproximationNeeded(legacyLines, meta, matches.Count);
        legacyLines.Add(outputPath);

        return string.Join(Environment.NewLine, legacyLines);
    }

    internal static string ConvertFinalStage(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName,
        IReadOnlyList<string> promptPrefixLines)
    {
        var playersLines = GetRequiredSectionLines(sections, "PlayersCsv", fullPath, formatName);
        var matchesLines = GetRequiredSectionLines(sections, "MatchesInput", fullPath, formatName);
        var players = ParsePlayersForPromptShape(playersLines, fullPath, formatName);
        var matches = ParseMatchesForPromptShape(matchesLines, players, "MatchesInput", fullPath, formatName);
        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var outputPath = GetOptionalMetaValue(output, "SummaryOutputPath")
            ?? GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "SummaryOutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath")
            ?? string.Empty;

        var legacyLines = new List<string>(promptPrefixLines)
        {
            GetRequiredMetaValue(meta, "FirstPlayerWinRatePercent", fullPath, formatName),
        };

        AppendDelimitedSection(legacyLines, playersLines);
        AppendDelimitedSection(legacyLines, GetRequiredSectionLines(sections, "GroupMapCsv", fullPath, formatName));
        AppendDelimitedSection(legacyLines, GetOptionalSectionLines(sections, "AdditionalApexPlayersCsv"));
        legacyLines.Add(ParseOffOnSelection(GetRequiredMetaValue(meta, "AdditionalApexPlacementMode", fullPath, formatName), offNumber: "1", onNumber: "2", "AdditionalApexPlacementMode", formatName));
        legacyLines.Add(ParseOffOnSelection(GetRequiredMetaValue(meta, "BoundaryRescueMode", fullPath, formatName), offNumber: "1", onNumber: "2", "BoundaryRescueMode", formatName));
        AppendEndTerminatedSection(legacyLines, matchesLines);
        AppendEndTerminatedSection(legacyLines, GetOptionalSectionLines(sections, "ReferenceMatchesInput"));
        AppendSimulationCountIfApproximationNeeded(legacyLines, meta, matches.Count);
        legacyLines.Add(outputPath);

        return string.Join(Environment.NewLine, legacyLines);
    }

    /// <summary>
    /// TODO: "TournamentFramework" は［４大域］のいずれかに含めてほしいぜ（＾～＾）
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="sections"></param>
    /// <param name="fullPath"></param>
    /// <param name="formatName"></param>
    /// <param name="promptPrefixLines"></param>
    /// <returns></returns>
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

    static List<Domain.Simulation.Player> ParsePlayersForPromptShape(
        IReadOnlyList<string> playersLines,
        string fullPath,
        string formatName)
    {
        if (!InputParsers.TryParsePlayers(playersLines, out var players, out var err))
        {
            throw new OperationCanceledException($"{formatName} の PlayersCsv セクションを解析できません: {err.Value} ({fullPath})");
        }

        return players;
    }

    static List<Domain.Simulation.Match> ParseMatchesForPromptShape(
        IReadOnlyList<string> matchesLines,
        IReadOnlyList<Domain.Simulation.Player> players,
        string sectionName,
        string fullPath,
        string formatName)
    {
        if (!InputParsers.TryParseMatches(matchesLines, players, out var matches, out var err))
        {
            throw new OperationCanceledException($"{formatName} の {sectionName} セクションを解析できません: {err.Value} ({fullPath})");
        }

        return matches;
    }

    static void AppendSimulationCountIfApproximationNeeded(
        List<string> legacyLines,
        Dictionary<string, string> meta,
        int matchCount)
    {
        if (matchCount <= ExactCalculationMatchThreshold) return;

        legacyLines.Add(GetOptionalMetaValue(meta, "SimulationCount") ?? string.Empty);
    }

}
