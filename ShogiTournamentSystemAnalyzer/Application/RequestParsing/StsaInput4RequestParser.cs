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

        AnalysisStepRequest stepRequest;
        if (flowSelection.Steps[0] == AnalysisFlowMode.Simulation && ruleProfileMode == RuleProfileMode.Standard)
        {
            var standardSimulationRequest = ParseStandardSimulationRequest(meta, sections, fullPath);
            if (standardSimulationRequest.Matches.Count > ExactCalculationMatchThreshold) return false;

            stepRequest = standardSimulationRequest;
        }
        else if (flowSelection.Steps[0] == AnalysisFlowMode.Simulation && ruleProfileMode == RuleProfileMode.FinalStage)
        {
            var finalStageSimulationRequest = ParseFinalStageSimulationRequest(meta, sections, fullPath);
            if (finalStageSimulationRequest.Matches.Count > ExactCalculationMatchThreshold
                && !finalStageSimulationRequest.SimulationCount.HasValue) return false;

            stepRequest = finalStageSimulationRequest;
        }
        else if (flowSelection.Steps[0] == AnalysisFlowMode.QualityEvaluation && ruleProfileMode == RuleProfileMode.Standard)
        {
            var standardQualityEvaluationRequest = ParseStandardQualityEvaluationRequest(meta, sections, fullPath);
            if (standardQualityEvaluationRequest.Input.Matches.Count > ExactCalculationMatchThreshold
                && !standardQualityEvaluationRequest.ExecutionOptions.SimulationCount.HasValue) return false;

            stepRequest = standardQualityEvaluationRequest;
        }
        else if (flowSelection.Steps[0] == AnalysisFlowMode.QualityEvaluation && ruleProfileMode == RuleProfileMode.FinalStage)
        {
            var finalStageQualityEvaluationRequest = ParseFinalStageQualityEvaluationRequest(meta, sections, fullPath);
            if (finalStageQualityEvaluationRequest.Input.Matches.Count > ExactCalculationMatchThreshold
                && !finalStageQualityEvaluationRequest.ExecutionOptions.SimulationCount.HasValue) return false;

            stepRequest = finalStageQualityEvaluationRequest;
        }
        else
        {
            return false;
        }

        request = new AnalysisRequest(
            flowSelection,
            ruleProfileMode,
            new[] { stepRequest });
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
        var output = ReadOutputKeyValues(sections, fullPath);
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

    static FinalStageSimulationRequest ParseFinalStageSimulationRequest(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath)
    {
        var firstPlayerWinRatePercent = ParseDouble(GetRequiredMetaValue(meta, "FirstPlayerWinRatePercent", fullPath, FormatName), "FirstPlayerWinRatePercent");
        var players = ParsePlayers(GetRequiredSectionLines(sections, "PlayersCsv", fullPath, FormatName), fullPath);
        var groupMap = ParseFinalStageGroupMap(GetRequiredSectionLines(sections, "GroupMapCsv", fullPath, FormatName), fullPath);
        if (!FinalStageValidators.ValidateFinalStagePlayers(players, groupMap, out var playerErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の PlayersCsv / GroupMapCsv セクションを本戦シミュレーションの選手一覧として検証できません: {playerErrorMessage} ({fullPath})");
        }

        var additionalApexPlayers = ParseOptionalPlayers(GetOptionalSectionLines(sections, "AdditionalApexPlayersCsv"), fullPath);
        if (!FinalStageValidators.ValidateAdditionalApexPlayers(players, groupMap, additionalApexPlayers, out var additionalApexErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の AdditionalApexPlayersCsv セクションを検証できません: {additionalApexErrorMessage} ({fullPath})");
        }

        var matches = ParseMatches(GetRequiredSectionLines(sections, "MatchesInput", fullPath, FormatName), players, "MatchesInput", fullPath);
        if (!FinalStageValidators.ValidateFinalStageMatches(players, groupMap, matches, out var matchErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の MatchesInput セクションを本戦シミュレーションの対局として検証できません: {matchErrorMessage} ({fullPath})");
        }

        var additionalApexPlacementMode = ParseAdditionalApexPlacementMode(GetRequiredMetaValue(meta, "AdditionalApexPlacementMode", fullPath, FormatName));
        var effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexPlayers.Count, additionalApexPlacementMode);
        var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
        var innovCount = groupMap.Count - apexCount;
        var outputPath = ReadSimulationOutputPath(meta, sections, fullPath);

        return new FinalStageSimulationRequest(
            TournamentRuleSetMode.Neutral,
            firstPlayerWinRatePercent,
            players,
            FinalStageGroupingMode.On,
            groupMap,
            additionalApexPlayers,
            additionalApexPlacementMode,
            effectiveAdditionalApexCount,
            ParseBoundaryRescueMode(GetRequiredMetaValue(meta, "BoundaryRescueMode", fullPath, FormatName)),
            apexCount,
            innovCount,
            matches,
            ParseOptionalMatches(GetOptionalSectionLines(sections, "ReferenceMatchesInput"), players, "ReferenceMatchesInput", fullPath),
            ParseOptionalInt(GetOptionalMetaValue(meta, "SimulationCount"), "SimulationCount"),
            outputPath);
    }

    static StandardQualityEvaluationRequest ParseStandardQualityEvaluationRequest(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath)
    {
        var tournamentRuleSetMode = ParseTournamentRuleSetMode(GetRequiredMetaValue(meta, "TournamentRuleSetMode", fullPath, FormatName));
        var players = ParsePlayers(GetRequiredSectionLines(sections, "PlayersCsv", fullPath, FormatName), fullPath);
        var matches = ParseMatches(GetRequiredSectionLines(sections, "MatchesInput", fullPath, FormatName), players, "MatchesInput", fullPath);
        var referenceMatches = ParseOptionalMatches(GetOptionalSectionLines(sections, "ReferenceMatchesInput"), players, "ReferenceMatchesInput", fullPath);

        if (!FinalStageValidators.ValidateFinalStagePlayers(players, out var playerErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の PlayersCsv セクションを標準品質評価の選手一覧として検証できません: {playerErrorMessage} ({fullPath})");
        }

        if (!FinalStageValidators.ValidateFinalStageMatches(players, matches, out var matchErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の MatchesInput セクションを標準品質評価の対局として検証できません: {matchErrorMessage} ({fullPath})");
        }

        var ruleDefinition = new TournamentQualityEvaluationRuleDefinition(
            FinalStageGroupingMode.Off,
            tournamentRuleSetMode,
            null,
            Array.Empty<Player>(),
            AdditionalApexPlacementMode.Off,
            0,
            BoundaryRescueMode.Off,
            VariableTop8Mode.Off,
            0);
        var input = new TournamentQualityEvaluationInput(
            players,
            matches,
            referenceMatches,
            TournamentQualityEvaluationInnovExpectedRankOffsetMode.Off,
            0);
        var executionOptions = ParseQualityEvaluationExecutionOptions(meta, fullPath);
        var outputOptions = ParseQualityEvaluationOutputOptions(meta, sections, executionOptions.IsSweep, fullPath, RuleProfileMode.Standard);

        return new StandardQualityEvaluationRequest(
            ruleDefinition,
            input,
            executionOptions,
            outputOptions);
    }

    static FinalStageQualityEvaluationRequest ParseFinalStageQualityEvaluationRequest(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath)
    {
        var players = ParsePlayers(GetRequiredSectionLines(sections, "PlayersCsv", fullPath, FormatName), fullPath);
        var groupMap = ParseFinalStageGroupMap(GetRequiredSectionLines(sections, "GroupMapCsv", fullPath, FormatName), fullPath);
        if (!FinalStageValidators.ValidateFinalStagePlayers(players, groupMap, out var playerErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の PlayersCsv / GroupMapCsv セクションを本戦品質評価の選手一覧として検証できません: {playerErrorMessage} ({fullPath})");
        }

        var additionalApexPlayers = ParseOptionalPlayers(GetOptionalSectionLines(sections, "AdditionalApexPlayersCsv"), fullPath);
        if (!FinalStageValidators.ValidateAdditionalApexPlayers(players, groupMap, additionalApexPlayers, out var additionalApexErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の AdditionalApexPlayersCsv セクションを検証できません: {additionalApexErrorMessage} ({fullPath})");
        }

        var matches = ParseMatches(GetRequiredSectionLines(sections, "MatchesInput", fullPath, FormatName), players, "MatchesInput", fullPath);
        if (!FinalStageValidators.ValidateFinalStageMatches(players, groupMap, matches, out var matchErrorMessage))
        {
            throw new OperationCanceledException($"{FormatName} の MatchesInput セクションを本戦品質評価の対局として検証できません: {matchErrorMessage} ({fullPath})");
        }

        var additionalApexPlacementMode = ParseAdditionalApexPlacementMode(GetRequiredMetaValue(meta, "AdditionalApexPlacementMode", fullPath, FormatName));
        var effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexPlayers.Count, additionalApexPlacementMode);
        var variableTop8Mode = ParseVariableTop8Mode(GetRequiredMetaValue(meta, "VariableTop8Mode", fullPath, FormatName));
        var ruleDefinition = new TournamentQualityEvaluationRuleDefinition(
            FinalStageGroupingMode.On,
            TournamentRuleSetMode.Neutral,
            groupMap,
            additionalApexPlayers,
            additionalApexPlacementMode,
            effectiveAdditionalApexCount,
            ParseBoundaryRescueMode(GetRequiredMetaValue(meta, "BoundaryRescueMode", fullPath, FormatName)),
            variableTop8Mode,
            VariableTop8Rule.GetPromotedInnovCount(variableTop8Mode, additionalApexPlayers.Count));
        var innovExpectedRankOffsetMode = ParseInnovExpectedRankOffsetMode(GetRequiredMetaValue(meta, "QualityInnovExpectedRankOffsetMode", fullPath, FormatName));
        var input = new TournamentQualityEvaluationInput(
            players,
            matches,
            ParseOptionalMatches(GetOptionalSectionLines(sections, "ReferenceMatchesInput"), players, "ReferenceMatchesInput", fullPath),
            innovExpectedRankOffsetMode,
            TournamentQualityEvaluationInnovExpectedRankOffsetRule.GetComparisonRankOffset(
                effectiveAdditionalApexCount,
                innovExpectedRankOffsetMode));
        var executionOptions = ParseQualityEvaluationExecutionOptions(meta, fullPath);
        var outputOptions = ParseQualityEvaluationOutputOptions(meta, sections, executionOptions.IsSweep, fullPath, RuleProfileMode.FinalStage);

        return new FinalStageQualityEvaluationRequest(
            ruleDefinition,
            input,
            executionOptions,
            outputOptions);
    }

    static string? ReadSimulationOutputPath(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath)
    {
        var output = ReadOutputKeyValues(sections, fullPath);
        return GetOptionalMetaValue(output, "SummaryOutputPath")
            ?? GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "SummaryOutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath");
    }

    static TournamentQualityEvaluationExecutionOptions ParseQualityEvaluationExecutionOptions(
        Dictionary<string, string> meta,
        string fullPath)
    {
        var executionMode = GetRequiredMetaValue(meta, "ExecutionMode", fullPath, FormatName);
        var simulationCount = ParseOptionalInt(GetOptionalMetaValue(meta, "SimulationCount"), "SimulationCount");
        if (executionMode.Equals("Sweep", StringComparison.OrdinalIgnoreCase) || executionMode == "2")
        {
            var sweepOptions = new TournamentQualitySweepOptions(
                true,
                ParseDouble(GetRequiredMetaValue(meta, "SweepStartPercent", fullPath, FormatName), "SweepStartPercent"),
                ParseDouble(GetRequiredMetaValue(meta, "SweepEndPercent", fullPath, FormatName), "SweepEndPercent"),
                ParseDouble(GetRequiredMetaValue(meta, "SweepStepPercent", fullPath, FormatName), "SweepStepPercent"));
            return new TournamentQualityEvaluationExecutionOptions(simulationCount, sweepOptions, null);
        }

        if (executionMode.Equals("Single", StringComparison.OrdinalIgnoreCase) || executionMode == "1")
        {
            var firstPlayerWinRatePercent = ParseDouble(GetRequiredMetaValue(meta, "FirstPlayerWinRatePercent", fullPath, FormatName), "FirstPlayerWinRatePercent");
            return new TournamentQualityEvaluationExecutionOptions(
                simulationCount,
                new TournamentQualitySweepOptions(false, 0.0, 0.0, 0.0),
                firstPlayerWinRatePercent);
        }

        throw new OperationCanceledException($"{FormatName} の ExecutionMode の値が解釈できません: {executionMode}");
    }

    static TournamentQualityEvaluationOutputOptions ParseQualityEvaluationOutputOptions(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        bool isSweep,
        string fullPath,
        RuleProfileMode ruleProfileMode)
    {
        var output = ReadOutputKeyValues(sections, fullPath);
        var groupingOptions = ParseReportGroupingOptions(meta, output);
        var outputPath = GetOptionalMetaValue(output, isSweep ? "SweepOutputPath" : "SummaryOutputPath")
            ?? GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, isSweep ? "SweepOutputPath" : "SummaryOutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath");
        if (string.IsNullOrWhiteSpace(outputPath)) throw new OperationCanceledException($"{FormatName} の Output セクションまたは Meta セクションに出力先パスがありません: {fullPath}");

        var playerCsvPath = GetOptionalMetaValue(output, "PlayerCsvPath")
            ?? GetOptionalMetaValue(meta, "PlayerCsvPath");
        return new TournamentQualityEvaluationOutputOptions(
            groupingOptions,
            outputPath,
            isSweep ? null : playerCsvPath,
            isSweep ? null : Path.ChangeExtension(outputPath, ".stsa.txt"),
            ruleProfileMode);
    }

    static TournamentQualityEvaluationReportGroupingOptions ParseReportGroupingOptions(
        Dictionary<string, string> meta,
        Dictionary<string, string> output)
    {
        var groupingMode = GetOptionalMetaValue(output, "TournamentQualityEvaluationReportGrouping")
            ?? GetOptionalMetaValue(meta, "TournamentQualityEvaluationReportGrouping")
            ?? GetOptionalMetaValue(output, "ExperimentalReportGrouping")
            ?? GetOptionalMetaValue(meta, "ExperimentalReportGrouping")
            ?? "Off";
        var groupingEnabled = groupingMode.Equals("On", StringComparison.OrdinalIgnoreCase) || groupingMode == "2";
        if (!groupingEnabled) return new TournamentQualityEvaluationReportGroupingOptions(false, null, string.Empty);

        var outcomeValue = GetOptionalMetaValue(output, "TournamentQualityEvaluationReportOutcome")
            ?? GetOptionalMetaValue(meta, "TournamentQualityEvaluationReportOutcome")
            ?? GetOptionalMetaValue(output, "ExperimentalReportOutcome")
            ?? GetOptionalMetaValue(meta, "ExperimentalReportOutcome")
            ?? "Good";
        var outcome = ParseGoodBadSelection(outcomeValue, "TournamentQualityEvaluationReportOutcome", FormatName) == "2"
            ? TournamentQualityEvaluationReportOutcome.Bad
            : TournamentQualityEvaluationReportOutcome.Good;
        var evaluationMemo = GetOptionalMetaValue(output, "EvaluationMemo")
            ?? GetOptionalMetaValue(meta, "EvaluationMemo")
            ?? string.Empty;
        return new TournamentQualityEvaluationReportGroupingOptions(true, outcome, evaluationMemo);
    }

    static Dictionary<string, string> ReadOutputKeyValues(Dictionary<string, List<string>> sections, string fullPath)
    {
        return sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, FormatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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

    static List<Player> ParseOptionalPlayers(IReadOnlyList<string> lines, string fullPath)
    {
        return lines.Any(line => !string.IsNullOrWhiteSpace(line))
            ? ParsePlayers(lines, fullPath)
            : new List<Player>();
    }

    static Dictionary<string, FinalStageGroup> ParseFinalStageGroupMap(IReadOnlyList<string> lines, string fullPath)
    {
        if (InputParsers.TryParseFinalStageGroups(lines, out var groupMap, out var err)) return groupMap;

        throw new OperationCanceledException($"{FormatName} の GroupMapCsv セクションを解析できません: {err.Value} ({fullPath})");
    }

    static AdditionalApexPlacementMode ParseAdditionalApexPlacementMode(string value)
    {
        return ParseOffOnSelection(value, offNumber: "1", onNumber: "2", "AdditionalApexPlacementMode", FormatName) == "2"
            ? AdditionalApexPlacementMode.On
            : AdditionalApexPlacementMode.Off;
    }

    static BoundaryRescueMode ParseBoundaryRescueMode(string value)
    {
        return ParseOffOnSelection(value, offNumber: "1", onNumber: "2", "BoundaryRescueMode", FormatName) == "2"
            ? BoundaryRescueMode.On
            : BoundaryRescueMode.Off;
    }

    static VariableTop8Mode ParseVariableTop8Mode(string value)
    {
        return ParseOffOnSelection(value, offNumber: "1", onNumber: "2", "VariableTop8Mode", FormatName) == "2"
            ? VariableTop8Mode.On
            : VariableTop8Mode.Off;
    }

    static TournamentQualityEvaluationInnovExpectedRankOffsetMode ParseInnovExpectedRankOffsetMode(string value)
    {
        return ParseOffOnSelection(value, offNumber: "1", onNumber: "2", "QualityInnovExpectedRankOffsetMode", FormatName) == "2"
            ? TournamentQualityEvaluationInnovExpectedRankOffsetMode.On
            : TournamentQualityEvaluationInnovExpectedRankOffsetMode.Off;
    }

    static List<Match> ParseOptionalMatches(
        IReadOnlyList<string> lines,
        IReadOnlyList<Player> players,
        string sectionName,
        string fullPath)
    {
        return lines.Any(line => !string.IsNullOrWhiteSpace(line))
            ? ParseMatches(lines, players, sectionName, fullPath)
            : new List<Match>();
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
