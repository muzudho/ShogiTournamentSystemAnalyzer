/*
 * ［アプリケーション　＞　入力　＞　STSA品質評価レガシー変換］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

using static ShogiTournamentSystemAnalyzer.Application.Input.StsaInputSectionParser;
using static ShogiTournamentSystemAnalyzer.Application.Input.StsaInputValueParser;
using static ShogiTournamentSystemAnalyzer.Application.Input.LegacyInputLineBuilder;

internal static class StsaQualityEvaluationLegacyConverter
{
    internal static string ConvertFinalStage(
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

    internal static string ConvertStandard(
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