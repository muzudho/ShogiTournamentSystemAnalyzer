/*
 * ［アプリケーション　＞　要求パース　＞　STSAInput/4］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries.Request;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.StsaInputValueParser;

internal static class StsaInput4RequestParser
{
    const int ExactCalculationMatchThreshold = 20;
    const string FormatName = "STSAInput/4";

    internal static bool TryParse(RequestText requestText, out AnalysisRequest? request)
    {
        request = null;
        if (!requestText.FormatName.Equals(FormatName, StringComparison.OrdinalIgnoreCase)) return false;

        var fullPath = requestText.SourcePath ?? "(要求テキスト)";
        var sections = ParseStsaInputSections(requestText.Lines, fullPath, FormatName);
        var meta = ParseSectionKeyValues(GetRequiredSectionLines(sections, "Meta", fullPath, FormatName), "Meta", fullPath, FormatName);
        var flowSelection = ReadFlowSelection(meta, fullPath);
        var ruleProfileMode = ParseRuleProfileMode(GetRequiredMetaValue(meta, "RuleProfileMode", fullPath, FormatName), FormatName);

        if (flowSelection.Steps.Count != 1) return false;
        if (flowSelection.Steps[0] != AnalysisFlowMode.Simulation) return false;
        if (ruleProfileMode != RuleProfileMode.Standard) return false;

        var stepRequest = ParseStandardSimulationRequest(meta, sections, fullPath);
        if (stepRequest.Matches.Count > ExactCalculationMatchThreshold) return false;

        request = new AnalysisRequest(
            flowSelection,
            ruleProfileMode,
            new AnalysisStepRequest[] { stepRequest });
        return true;
    }

    static StandardSimulationRequest ParseStandardSimulationRequest(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath)
    {
        var tournamentRuleSetMode = ParseTournamentRuleSetMode(GetRequiredMetaValue(meta, "TournamentRuleSetMode", fullPath, FormatName));
        var firstPlayerWinRatePercent = ParseDouble(GetRequiredMetaValue(meta, "FirstPlayerWinRatePercent", fullPath, FormatName), "FirstPlayerWinRatePercent");
        var playersLines = GetRequiredSectionLines(sections, "PlayersCsv", fullPath, FormatName);
        var matchesLines = GetRequiredSectionLines(sections, "MatchesInput", fullPath, FormatName);
        var allPlayers = ParsePlayers(playersLines, fullPath);
        var allMatches = ParseMatches(matchesLines, allPlayers, "MatchesInput", fullPath);
        var (players, matches) = ModeSupportHelpers.FilterToScheduledPlayers(allPlayers, allMatches);
        var simulationCount = ParseOptionalInt(GetOptionalMetaValue(meta, "SimulationCount"), "SimulationCount");
        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, FormatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var outputPath = GetOptionalMetaValue(output, "SummaryOutputPath")
            ?? GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "SummaryOutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath");

        return new StandardSimulationRequest(
            tournamentRuleSetMode,
            firstPlayerWinRatePercent,
            allPlayers,
            players,
            matches,
            simulationCount,
            outputPath);
    }

    static AnalysisFlowSelection ReadFlowSelection(Dictionary<string, string> meta, string fullPath)
    {
        var stepsValue = GetOptionalMetaValue(meta, "AnalysisFlowSteps");
        if (!string.IsNullOrWhiteSpace(stepsValue)) return ParseAnalysisFlowSteps(stepsValue, FormatName);

        var modeValue = GetRequiredMetaValue(meta, "AnalysisFlowMode", fullPath, FormatName);
        return AnalysisFlowSelection.FromSingle(ParseAnalysisFlowMode(modeValue, FormatName));
    }

    static TournamentRuleSetMode ParseTournamentRuleSetMode(string value)
    {
        return ParseTournamentRuleSetSelection(value, FormatName) switch
        {
            "2" => TournamentRuleSetMode.Twill,
            "3" => TournamentRuleSetMode.TwillCommonOpponentWeighted,
            _ => TournamentRuleSetMode.Neutral,
        };
    }

    static double ParseDouble(string value, string keyName)
    {
        if (InputParsers.TryParseDouble(value, out var parsed)) return parsed;

        throw new OperationCanceledException($"{FormatName} の {keyName} を数値として解釈できません: {value}");
    }

    static int? ParseOptionalInt(string? value, string keyName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (int.TryParse(value, out var parsed) && parsed > 0) return parsed;

        throw new OperationCanceledException($"{FormatName} の {keyName} は 1 以上の整数で入力してください: {value}");
    }

    static List<Player> ParsePlayers(IReadOnlyList<string> lines, string fullPath)
    {
        if (InputParsers.TryParsePlayers(lines, out var players, out var err)) return players;

        throw new OperationCanceledException($"{FormatName} の PlayersCsv セクションを解析できません: {err.Value} ({fullPath})");
    }

    static List<Match> ParseMatches(
        IReadOnlyList<string> lines,
        IReadOnlyList<Player> players,
        string sectionName,
        string fullPath)
    {
        if (InputParsers.TryParseMatches(lines, players, out var matches, out var err)) return matches;

        throw new OperationCanceledException($"{FormatName} の {sectionName} セクションを解析できません: {err.Value} ({fullPath})");
    }
}
