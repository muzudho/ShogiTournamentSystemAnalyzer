/*
 * ［アプリケーション　＞　入力　＞　STSA入力レガシー変換］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using static ShogiTournamentSystemAnalyzer.Application.Input.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.Input.StsaInputValueParser;
using static ShogiTournamentSystemAnalyzer.Application.Input.LegacyInputLineBuilder;

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
                || GetOptionalMetaValue(meta, "TournamentFrameworkMode") is not null)) return ConvertStsaTournamentFramework(meta, sections, fullPath, formatName);

        if (analysisFlowMode == AnalysisFlowMode.Simulation
            && ruleProfileMode == RuleProfileMode.Empty) return ConvertStsaEmpty(meta, sections, fullPath, formatName);

        if (analysisFlowMode != AnalysisFlowMode.QualityEvaluation) throw new OperationCanceledException($"{formatName} の最小対応は、現在のところ『品質評価』のみです。");

        return ruleProfileMode == RuleProfileMode.FinalStage
            ? ConvertStsaQualityEvaluationFinalStage(meta, sections, fullPath, formatName)
            : ConvertStsaQualityEvaluationStandard(meta, sections, fullPath, formatName);
    }

    static string ConvertStsaQualityEvaluationFinalStage(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName)
    {
        var legacyLines = new List<string>
        {
            "2",
            "2"
        };

        AppendDelimitedSection(legacyLines, GetRequiredSectionLines(sections, "PlayersCsv", fullPath, formatName));
        AppendDelimitedSection(legacyLines, GetRequiredSectionLines(sections, "GroupMapCsv", fullPath, formatName));
        AppendDelimitedSection(legacyLines, GetOptionalSectionLines(sections, "AdditionalApexPlayersCsv"));
        legacyLines.Add(ParseOffOnSelection(GetRequiredMetaValue(meta, "AdditionalApexPlacementMode", fullPath, formatName), offNumber: "1", onNumber: "2", "AdditionalApexPlacementMode", formatName));
        legacyLines.Add(ParseOffOnSelection(GetRequiredMetaValue(meta, "BoundaryRescueMode", fullPath, formatName), offNumber: "1", onNumber: "2", "BoundaryRescueMode", formatName));
        legacyLines.Add(ParseOffOnSelection(GetRequiredMetaValue(meta, "VariableTop8Mode", fullPath, formatName), offNumber: "1", onNumber: "2", "VariableTop8Mode", formatName));
        AppendEndTerminatedSection(legacyLines, GetRequiredSectionLines(sections, "MatchesInput", fullPath, formatName));
        AppendEndTerminatedSection(legacyLines, GetOptionalSectionLines(sections, "ReferenceMatchesInput"));
        legacyLines.Add(ParseOffOnSelection(GetRequiredMetaValue(meta, "QualityInnovExpectedRankOffsetMode", fullPath, formatName), offNumber: "1", onNumber: "2", "QualityInnovExpectedRankOffsetMode", formatName));

        var executionModeValue = GetRequiredMetaValue(meta, "ExecutionMode", fullPath, formatName);
        var isSweep = executionModeValue.Equals("Sweep", StringComparison.OrdinalIgnoreCase) || executionModeValue == "2";
        legacyLines.Add(isSweep ? "2" : "1");
        if (!isSweep)
        {
            legacyLines.Add(GetRequiredMetaValue(meta, "FirstPlayerWinRatePercent", fullPath, formatName));
            var simulationCount = GetOptionalMetaValue(meta, "SimulationCount");
            if (!string.IsNullOrWhiteSpace(simulationCount))
            {
                legacyLines.Add(simulationCount);
            }
        }
        else
        {
            legacyLines.Add(GetRequiredMetaValue(meta, "SweepStartPercent", fullPath, formatName));
            legacyLines.Add(GetRequiredMetaValue(meta, "SweepEndPercent", fullPath, formatName));
            legacyLines.Add(GetRequiredMetaValue(meta, "SweepStepPercent", fullPath, formatName));
        }

        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var groupingMode = GetOptionalMetaValue(output, "TournamentQualityEvaluationReportGrouping")
            ?? GetOptionalMetaValue(meta, "TournamentQualityEvaluationReportGrouping")
            ?? GetOptionalMetaValue(output, "ExperimentalReportGrouping")
            ?? GetOptionalMetaValue(meta, "ExperimentalReportGrouping")
            ?? "Off";
        var groupingEnabled = groupingMode.Equals("On", StringComparison.OrdinalIgnoreCase) || groupingMode == "2";
        legacyLines.Add(groupingEnabled ? "2" : "1");
        if (groupingEnabled)
        {
            var outcomeValue = GetOptionalMetaValue(output, "TournamentQualityEvaluationReportOutcome")
                ?? GetOptionalMetaValue(meta, "TournamentQualityEvaluationReportOutcome")
                ?? GetOptionalMetaValue(output, "ExperimentalReportOutcome")
                ?? GetOptionalMetaValue(meta, "ExperimentalReportOutcome")
                ?? "Good";
            legacyLines.Add(ParseGoodBadSelection(outcomeValue, "ExperimentalReportOutcome", formatName));
            legacyLines.Add(GetOptionalMetaValue(output, "EvaluationMemo")
                ?? GetOptionalMetaValue(meta, "EvaluationMemo")
                ?? string.Empty);
        }

        var outputPath = GetOptionalMetaValue(output, "SummaryOutputPath")
            ?? GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "SummaryOutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath");
        if (string.IsNullOrWhiteSpace(outputPath)) throw new OperationCanceledException($"{formatName} の Output セクションまたは Meta セクションに出力先パスがありません: {fullPath}");

        legacyLines.Add(outputPath);
        return string.Join(Environment.NewLine, legacyLines);
    }

    static string ConvertStsaTournamentFramework(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName)
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

        var legacyLines = new List<string>
        {
            "1",
            "3",
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

    static string ConvertStsaEmpty(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName)
    {
        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var outputPath = GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath")
            ?? string.Empty;

        var legacyLines = new List<string>
        {
            "1",
            "4",
            outputPath,
        };

        return string.Join(Environment.NewLine, legacyLines);
    }

    static string ConvertStsaQualityEvaluationStandard(
        Dictionary<string, string> meta,
        Dictionary<string, List<string>> sections,
        string fullPath,
        string formatName)
    {
        var legacyLines = new List<string>
        {
            "2",
            "1"
        };

        AppendDelimitedSection(legacyLines, GetRequiredSectionLines(sections, "PlayersCsv", fullPath, formatName));
        legacyLines.Add(ParseTournamentRuleSetSelection(GetRequiredMetaValue(meta, "TournamentRuleSetMode", fullPath, formatName), formatName));
        AppendEndTerminatedSection(legacyLines, GetRequiredSectionLines(sections, "MatchesInput", fullPath, formatName));
        AppendEndTerminatedSection(legacyLines, GetOptionalSectionLines(sections, "ReferenceMatchesInput"));

        var executionModeValue = GetRequiredMetaValue(meta, "ExecutionMode", fullPath, formatName);
        var isSweep = executionModeValue.Equals("Sweep", StringComparison.OrdinalIgnoreCase) || executionModeValue == "2";
        legacyLines.Add(isSweep ? "2" : "1");
        if (!isSweep)
        {
            legacyLines.Add(GetRequiredMetaValue(meta, "FirstPlayerWinRatePercent", fullPath, formatName));
            var simulationCount = GetOptionalMetaValue(meta, "SimulationCount");
            if (!string.IsNullOrWhiteSpace(simulationCount))
            {
                legacyLines.Add(simulationCount);
            }
        }
        else
        {
            legacyLines.Add(GetRequiredMetaValue(meta, "SweepStartPercent", fullPath, formatName));
            legacyLines.Add(GetRequiredMetaValue(meta, "SweepEndPercent", fullPath, formatName));
            legacyLines.Add(GetRequiredMetaValue(meta, "SweepStepPercent", fullPath, formatName));
        }

        var output = sections.TryGetValue("Output", out var outputLines)
            ? ParseSectionKeyValues(outputLines, "Output", fullPath, formatName)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var groupingMode = GetOptionalMetaValue(output, "TournamentQualityEvaluationReportGrouping")
            ?? GetOptionalMetaValue(meta, "TournamentQualityEvaluationReportGrouping")
            ?? GetOptionalMetaValue(output, "ExperimentalReportGrouping")
            ?? GetOptionalMetaValue(meta, "ExperimentalReportGrouping")
            ?? "Off";
        var groupingEnabled = groupingMode.Equals("On", StringComparison.OrdinalIgnoreCase) || groupingMode == "2";
        legacyLines.Add(groupingEnabled ? "2" : "1");
        if (groupingEnabled)
        {
            var outcomeValue = GetOptionalMetaValue(output, "TournamentQualityEvaluationReportOutcome")
                ?? GetOptionalMetaValue(meta, "TournamentQualityEvaluationReportOutcome")
                ?? GetOptionalMetaValue(output, "ExperimentalReportOutcome")
                ?? GetOptionalMetaValue(meta, "ExperimentalReportOutcome")
                ?? "Good";
            legacyLines.Add(ParseGoodBadSelection(outcomeValue, "ExperimentalReportOutcome", formatName));
            legacyLines.Add(GetOptionalMetaValue(output, "EvaluationMemo")
                ?? GetOptionalMetaValue(meta, "EvaluationMemo")
                ?? string.Empty);
        }

        var outputPath = GetOptionalMetaValue(output, isSweep ? "SweepOutputPath" : "SummaryOutputPath")
            ?? GetOptionalMetaValue(output, "OutputPath")
            ?? GetOptionalMetaValue(meta, isSweep ? "SweepOutputPath" : "SummaryOutputPath")
            ?? GetOptionalMetaValue(meta, "OutputPath");
        if (string.IsNullOrWhiteSpace(outputPath)) throw new OperationCanceledException($"{formatName} の Output セクションまたは Meta セクションに出力先パスがありません: {fullPath}");

        legacyLines.Add(outputPath);
        return string.Join(Environment.NewLine, legacyLines);
    }

}