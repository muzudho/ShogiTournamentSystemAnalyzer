/*
 * ［アプリケーション　＞　要求ファイル書出　＞　STSAInput］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using System.Globalization;
using Match = ShogiTournamentSystemAnalyzer.Domain.Simulation.Match;

internal static class StsaInputRequestWriter
{
    internal static IReadOnlyList<string> BuildLines(AnalysisRequest request)
    {
        return BuildLines(request, useAttributeFormat: false);
    }

    internal static IReadOnlyList<string> BuildAttributeLines(AnalysisRequest request)
    {
        return BuildLines(request, useAttributeFormat: true);
    }

    static IReadOnlyList<string> BuildLines(AnalysisRequest request, bool useAttributeFormat)
    {
        return request.Steps.Count == 1
            ? BuildSingleStepLines(request, useAttributeFormat)
            : BuildMultiStepLines(request, useAttributeFormat);
    }

    static IReadOnlyList<string> BuildSingleStepLines(AnalysisRequest request, bool useAttributeFormat)
    {
        var lines = new List<string>
        {
            useAttributeFormat ? "#[Format] STSAInput/5" : "#[Format] STSAInput/4",
            string.Empty,
            "#[Section] Meta",
            $"AnalysisFlowSteps={request.FlowSelection.ToRequestFileValue()}",
        };

        if (!useAttributeFormat)
        {
            lines.Add($"RuleProfileMode={request.Steps[0].GetRuleProfileAttributes().ToCompatibilityLabel()}");
        }

        AddMetaLines(lines, request.Steps[0]);
        lines.Add("#[EndSection]");

        if (useAttributeFormat)
        {
            AddRuleProfileAttributesSection(lines, "RuleProfileAttributes", request.Steps[0].GetRuleProfileAttributes());
        }

        AddBodySections(lines, request.Steps[0]);
        return lines;
    }

    static IReadOnlyList<string> BuildMultiStepLines(AnalysisRequest request, bool useAttributeFormat)
    {
        var lines = new List<string>
        {
            useAttributeFormat ? "#[Format] STSAInput/5" : "#[Format] STSAInput/4",
            string.Empty,
            "#[Section] Meta",
            $"AnalysisFlowSteps={request.FlowSelection.ToRequestFileValue()}",
            "#[EndSection]",
        };

        foreach (var step in request.Steps)
        {
            var stepName = GetStepName(step);
            lines.Add(string.Empty);
            lines.Add($"#[Section] Step.{stepName}");
            if (!useAttributeFormat)
            {
                lines.Add($"RuleProfileMode={step.GetRuleProfileAttributes().ToCompatibilityLabel()}");
            }

            AddMetaLines(lines, step);
            lines.Add("#[EndSection]");

            if (useAttributeFormat)
            {
                AddRuleProfileAttributesSection(lines, $"Step.{stepName}.RuleProfileAttributes", step.GetRuleProfileAttributes());
            }
        }

        foreach (var step in request.Steps)
        {
            var stepName = GetStepName(step);
            AddBodySections(lines, step, $"{stepName}.", $"{stepName}.Output");
        }

        return lines;
    }

    static void AddRuleProfileAttributesSection(
        List<string> lines,
        string sectionName,
        RuleProfileAttributes attributes)
    {
        lines.Add(string.Empty);
        lines.Add($"#[Section] {sectionName}");
        lines.Add($"SimulationShape={attributes.SimulationShape}");
        lines.Add($"UsesFinalStageGrouping={FormatOnOff(attributes.UsesFinalStageGrouping)}");
        lines.Add($"UsesAdditionalApexPlacement={FormatOnOff(attributes.UsesAdditionalApexPlacement)}");
        lines.Add($"UsesBoundaryRescue={FormatOnOff(attributes.UsesBoundaryRescue)}");
        lines.Add($"UsesVariableTop8={FormatOnOff(attributes.UsesVariableTop8)}");
        lines.Add($"RankingRuleSetMode={attributes.RankingRuleSetMode}");
        lines.Add($"HasReferenceMatches={FormatOnOff(attributes.HasReferenceMatches)}");
        lines.Add($"PairingSource={attributes.PairingSource}");
        lines.Add("#[EndSection]");
    }

    static void AddMetaLines(List<string> lines, AnalysisStepRequest step)
    {
        switch (step)
        {
            case StandardSimulationRequest request:
                lines.Add($"TournamentRuleSetMode={request.TournamentRuleSetMode}");
                lines.Add($"FirstPlayerWinRatePercent={FormatNumber(request.FirstPlayerWinRatePercent)}");
                AddOptionalInt(lines, "SimulationCount", request.SimulationCount);
                break;

            case FinalStageSimulationRequest request:
                lines.Add($"FirstPlayerWinRatePercent={FormatNumber(request.FirstPlayerWinRatePercent)}");
                AddOptionalInt(lines, "SimulationCount", request.SimulationCount);
                lines.Add($"AdditionalApexPlacementMode={request.AdditionalApexPlacementMode}");
                lines.Add($"BoundaryRescueMode={request.BoundaryRescueMode}");
                break;

            case TournamentFrameworkSimulationRequest request:
                lines.Add($"TournamentRuleSetMode={request.TournamentRuleSetMode}");
                lines.Add($"FirstPlayerWinRatePercent={FormatNumber(request.FirstPlayerWinRatePercent)}");
                AddOptionalInt(lines, "RandomSeed", request.RandomSeed);
                AddOptionalInt(lines, "SimulationCount", request.SimulationCount);
                break;

            case EmptySimulationRequest:
                break;

            case StandardQualityEvaluationRequest request:
                AddQualityMetaLines(lines, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            case DeferredStandardQualityEvaluationRequest request:
                AddDeferredStandardQualityMetaLines(lines, request.TournamentRuleSetMode, request.ExecutionOptions, request.OutputOptions);
                break;

            case FinalStageQualityEvaluationRequest request:
                AddQualityMetaLines(lines, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            case DeferredFinalStageQualityEvaluationRequest request:
                AddDeferredFinalStageQualityMetaLines(lines, request.VariableTop8Mode, request.InnovExpectedRankOffsetMode, request.ExecutionOptions, request.OutputOptions);
                break;

            default:
                throw new InvalidOperationException($"未対応の分析要求です: {step.GetType().Name}");
        }
    }

    static void AddDeferredStandardQualityMetaLines(
        List<string> lines,
        TournamentRuleSetMode tournamentRuleSetMode,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        AddQualityExecutionMetaLines(lines, executionOptions, outputOptions);
        lines.Add($"TournamentRuleSetMode={tournamentRuleSetMode}");
        lines.Add("QualityInnovExpectedRankOffsetMode=Off");
    }

    static void AddDeferredFinalStageQualityMetaLines(
        List<string> lines,
        VariableTop8Mode variableTop8Mode,
        TournamentQualityEvaluationInnovExpectedRankOffsetMode innovExpectedRankOffsetMode,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        AddQualityExecutionMetaLines(lines, executionOptions, outputOptions);
        lines.Add($"TournamentRuleSetMode={TournamentRuleSetMode.Neutral}");
        lines.Add($"AdditionalApexPlacementMode={AdditionalApexPlacementMode.On}");
        lines.Add($"BoundaryRescueMode={BoundaryRescueMode.On}");
        lines.Add($"VariableTop8Mode={variableTop8Mode}");
        lines.Add($"QualityInnovExpectedRankOffsetMode={innovExpectedRankOffsetMode}");
    }
    static void AddQualityMetaLines(
        List<string> lines,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        AddQualityExecutionMetaLines(lines, executionOptions, outputOptions);
        lines.Add($"TournamentRuleSetMode={ruleDefinition.TournamentRuleSetMode}");

        if (ruleDefinition.UsesFinalStageGrouping)
        {
            lines.Add($"AdditionalApexPlacementMode={ruleDefinition.AdditionalApexPlacementMode}");
            lines.Add($"BoundaryRescueMode={ruleDefinition.BoundaryRescueMode}");
            lines.Add($"VariableTop8Mode={ruleDefinition.VariableTop8Mode}");
            lines.Add($"QualityInnovExpectedRankOffsetMode={input.InnovExpectedRankOffsetMode}");
        }
        else
        {
            lines.Add("QualityInnovExpectedRankOffsetMode=Off");
        }


    }

    static void AddQualityExecutionMetaLines(
        List<string> lines,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        lines.Add($"ExecutionMode={(executionOptions.IsSweep ? "Sweep" : "Single")}");
        if (!executionOptions.IsSweep && executionOptions.FirstPlayerWinRatePercent.HasValue)
        {
            lines.Add($"FirstPlayerWinRatePercent={FormatNumber(executionOptions.FirstPlayerWinRatePercent.Value)}");
        }

        AddOptionalInt(lines, "SimulationCount", executionOptions.SimulationCount);
        if (executionOptions.IsSweep)
        {
            lines.Add($"SweepStartPercent={FormatNumber(executionOptions.SweepOptions.StartPercent)}");
            lines.Add($"SweepEndPercent={FormatNumber(executionOptions.SweepOptions.EndPercent)}");
            lines.Add($"SweepStepPercent={FormatNumber(executionOptions.SweepOptions.StepPercent)}");
        }

        lines.Add($"TournamentQualityEvaluationReportGrouping={FormatOnOff(outputOptions.ReportGroupingOptions.IsEnabled)}");
        if (outputOptions.ReportGroupingOptions.IsEnabled && outputOptions.ReportGroupingOptions.Outcome.HasValue)
        {
            lines.Add($"TournamentQualityEvaluationReportOutcome={outputOptions.ReportGroupingOptions.Outcome.Value}");
        }

        if (!string.IsNullOrWhiteSpace(outputOptions.ReportGroupingOptions.EvaluationMemo))
        {
            lines.Add($"EvaluationMemo={outputOptions.ReportGroupingOptions.EvaluationMemo}");
        }
    }
    static void AddBodySections(
        List<string> lines,
        AnalysisStepRequest step,
        string sectionPrefix = "",
        string outputSectionName = "Output")
    {
        switch (step)
        {
            case StandardSimulationRequest request:
                AddPlayersSection(lines, $"{sectionPrefix}PlayersCsv", request.AllPlayers);
                AddMatchesSection(lines, $"{sectionPrefix}MatchesInput", request.Players, request.Matches);
                AddOutputSection(lines, outputSectionName, summaryOutputPath: request.OutputPath);
                break;

            case FinalStageSimulationRequest request:
                AddPlayersSection(lines, $"{sectionPrefix}PlayersCsv", request.Players);
                AddGroupMapSection(lines, $"{sectionPrefix}GroupMapCsv", request.Players, request.GroupMap);
                AddOptionalPlayersSection(lines, $"{sectionPrefix}AdditionalApexPlayersCsv", request.AdditionalApexPlayers);
                AddMatchesSection(lines, $"{sectionPrefix}MatchesInput", request.Players, request.Matches);
                AddOptionalMatchesSection(lines, $"{sectionPrefix}ReferenceMatchesInput", request.Players, request.ReferenceMatches);
                AddOutputSection(lines, outputSectionName, summaryOutputPath: request.OutputPath);
                break;

            case TournamentFrameworkSimulationRequest request:
                AddInputsSection(lines, $"{sectionPrefix}Inputs", request);
                AddOutputSection(lines, outputSectionName, outputPath: request.OutputPath);
                break;

            case EmptySimulationRequest request:
                AddOutputSection(lines, outputSectionName, outputPath: request.OutputPath);
                break;

            case StandardQualityEvaluationRequest request:
                AddQualityBodySections(lines, sectionPrefix, outputSectionName, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            case DeferredStandardQualityEvaluationRequest request:
                AddDeferredQualityBodySections(lines, outputSectionName, request.ExecutionOptions, request.OutputOptions);
                break;

            case FinalStageQualityEvaluationRequest request:
                AddQualityBodySections(lines, sectionPrefix, outputSectionName, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            case DeferredFinalStageQualityEvaluationRequest request:
                AddDeferredQualityBodySections(lines, outputSectionName, request.ExecutionOptions, request.OutputOptions);
                break;
        }
    }

    static void AddDeferredQualityBodySections(
        List<string> lines,
        string outputSectionName,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        AddOutputSection(
            lines,
            outputSectionName,
            summaryOutputPath: executionOptions.IsSweep ? null : outputOptions.OutputCsvPath,
            sweepOutputPath: executionOptions.IsSweep ? outputOptions.OutputCsvPath : null,
            playerCsvPath: outputOptions.PlayerCsvPath);
    }
    static void AddQualityBodySections(
        List<string> lines,
        string sectionPrefix,
        string outputSectionName,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        AddPlayersSection(lines, $"{sectionPrefix}PlayersCsv", input.Players);
        if (ruleDefinition.UsesFinalStageGrouping)
        {
            AddGroupMapSection(lines, $"{sectionPrefix}GroupMapCsv", input.Players, ruleDefinition.GroupMap!);
            AddOptionalPlayersSection(lines, $"{sectionPrefix}AdditionalApexPlayersCsv", ruleDefinition.AdditionalApexPlayers);
        }

        AddMatchesSection(lines, $"{sectionPrefix}MatchesInput", input.Players, input.Matches);
        AddOptionalMatchesSection(lines, $"{sectionPrefix}ReferenceMatchesInput", input.Players, input.ReferenceMatches);
        AddOutputSection(
            lines,
            outputSectionName,
            summaryOutputPath: executionOptions.IsSweep ? null : outputOptions.OutputCsvPath,
            sweepOutputPath: executionOptions.IsSweep ? outputOptions.OutputCsvPath : null,
            playerCsvPath: outputOptions.PlayerCsvPath);
    }

    static void AddPlayersSection(List<string> lines, string sectionName, IReadOnlyList<Player> players)
    {
        AddSection(lines, sectionName, new[] { "name,elo" }.Concat(players.Select(player => string.Join(",", EscapeCsv(player.Name), FormatNumber(player.Rating)))));
    }

    static void AddOptionalPlayersSection(List<string> lines, string sectionName, IReadOnlyList<Player> players)
    {
        if (players.Count > 0) AddPlayersSection(lines, sectionName, players);
    }

    static void AddGroupMapSection(List<string> lines, string sectionName, IReadOnlyList<Player> players, IReadOnlyDictionary<string, FinalStageGroup> groupMap)
    {
        var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var body = new List<string> { "group,name" };
        foreach (var player in players)
        {
            if (!groupMap.TryGetValue(player.Name, out var group)) continue;

            body.Add(string.Join(",", group, EscapeCsv(player.Name)));
            emitted.Add(player.Name);
        }

        foreach (var item in groupMap.Where(item => !emitted.Contains(item.Key)).OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            body.Add(string.Join(",", item.Value, EscapeCsv(item.Key)));
        }

        AddSection(lines, sectionName, body);
    }

    static void AddMatchesSection(List<string> lines, string sectionName, IReadOnlyList<Player> players, IReadOnlyList<Match> matches)
    {
        var body = new List<string> { "first,second" };
        body.AddRange(matches.Select(match => string.Join(",", EscapeCsv(players[match.FirstPlayer].Name), EscapeCsv(players[match.SecondPlayer].Name))));
        AddSection(lines, sectionName, body);
    }

    static void AddOptionalMatchesSection(List<string> lines, string sectionName, IReadOnlyList<Player> players, IReadOnlyList<Match> matches)
    {
        if (matches.Count > 0) AddMatchesSection(lines, sectionName, players, matches);
    }

    static void AddInputsSection(List<string> lines, string sectionName, TournamentFrameworkSimulationRequest request)
    {
        var body = new List<string>
        {
            $"PlayersCsvPath={request.PlayersCsvPath}",
            $"StagesCsvPath={request.StagesCsvPath}",
            $"TournamentMatchRecordsCsvPath={request.TournamentMatchRecordsCsvPath}",
        };
        AddOptionalString(body, "RuleFilePath", request.RuleFilePath);
        AddSection(lines, sectionName, body);
    }

    static void AddOutputSection(
        List<string> lines,
        string sectionName,
        string? summaryOutputPath = null,
        string? sweepOutputPath = null,
        string? playerCsvPath = null,
        string? outputPath = null)
    {
        var body = new List<string>();
        AddOptionalString(body, "SummaryOutputPath", summaryOutputPath);
        AddOptionalString(body, "SweepOutputPath", sweepOutputPath);
        AddOptionalString(body, "PlayerCsvPath", playerCsvPath);
        AddOptionalString(body, "OutputPath", outputPath);
        if (body.Count > 0) AddSection(lines, sectionName, body);
    }

    static void AddSection(List<string> lines, string sectionName, IEnumerable<string> body)
    {
        lines.Add(string.Empty);
        lines.Add($"#[Section] {sectionName}");
        lines.AddRange(body);
        lines.Add("#[EndSection]");
    }

    static void AddOptionalInt(List<string> lines, string key, int? value)
    {
        if (value.HasValue) lines.Add($"{key}={value.Value.ToString(CultureInfo.InvariantCulture)}");
    }

    static void AddOptionalString(List<string> lines, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) lines.Add($"{key}={value}");
    }


    static string GetStepName(AnalysisStepRequest step)
    {
        return step switch
        {
            StandardSimulationRequest => "Simulation",
            FinalStageSimulationRequest => "Simulation",
            TournamentFrameworkSimulationRequest => "Simulation",
            EmptySimulationRequest => "Simulation",
            StandardQualityEvaluationRequest => "QualityEvaluation",
            DeferredStandardQualityEvaluationRequest => "QualityEvaluation",
            FinalStageQualityEvaluationRequest => "QualityEvaluation",
            DeferredFinalStageQualityEvaluationRequest => "QualityEvaluation",
            _ => throw new InvalidOperationException($"未対応の分析要求です: {step.GetType().Name}"),
        };
    }

    static string FormatOnOff(bool value)
    {
        return value ? "On" : "Off";
    }

    static string FormatNumber(double value)
    {
        return value.ToString("G17", CultureInfo.InvariantCulture);
    }

    static string EscapeCsv(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n')) return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
