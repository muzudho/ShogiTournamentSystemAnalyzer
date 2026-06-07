/*
 * ［アプリケーション　＞　要求ファイル書出　＞　STSAInput］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Compatibility.LegacyRuleProfile;
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
        return request.StepRequests.Count == 1
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
            lines.Add($"RuleProfileMode={LegacyRuleProfileMapper.FormatLabel(request.StepRequests[0].GetRuleProfileAttributes())}");
        }

        AddMetaLines(lines, request.StepRequests[0]);
        lines.Add("#[EndSection]");

        if (useAttributeFormat)
        {
            AddRuleProfileAttributesSection(lines, "RuleProfileAttributes", request.StepRequests[0].GetRuleProfileAttributes());
        }

        AddBodySections(lines, request.StepRequests[0]);
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

        foreach (var stepRequest in request.StepRequests)
        {
            var stepSectionName = GetStepSectionName(stepRequest);
            lines.Add(string.Empty);
            lines.Add($"#[Section] {stepSectionName}");
            if (!useAttributeFormat)
            {
                lines.Add($"RuleProfileMode={LegacyRuleProfileMapper.FormatLabel(stepRequest.GetRuleProfileAttributes())}");
            }

            AddMetaLines(lines, stepRequest);
            lines.Add("#[EndSection]");

            if (useAttributeFormat)
            {
                AddRuleProfileAttributesSection(lines, $"{stepSectionName}.RuleProfileAttributes", stepRequest.GetRuleProfileAttributes());
            }
        }

        foreach (var stepRequest in request.StepRequests)
        {
            var stepSectionName = GetStepSectionName(stepRequest);
            AddBodySections(lines, stepRequest, $"{stepSectionName}.", $"{stepSectionName}.Output");
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

    static void AddMetaLines(List<string> lines, AnalysisStepRequest stepRequest)
    {
        switch (stepRequest)
        {
            case SimulationStepRequest request:
                AddSimulationMetaLines(lines, request);
                break;

            case QualityEvaluationStepRequest request:
                AddQualityMetaLines(lines, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            case DeferredQualityEvaluationStepRequest request:
                AddDeferredQualityMetaLines(lines, request);
                break;

            default:
                throw new InvalidOperationException($"未対応の分析要求です: {stepRequest.GetType().Name}");
        }
    }

    static void AddSimulationMetaLines(List<string> lines, SimulationStepRequest request)
    {
        if (request.TournamentFrameworkInput is not null)
        {
            lines.Add($"TournamentRuleSetMode={request.TournamentFrameworkInput.TournamentRuleSetMode}");
            lines.Add($"FirstPlayerWinRatePercent={FormatNumber(request.FirstPlayerWinRatePercent)}");
            AddOptionalInt(lines, "RandomSeed", request.TournamentFrameworkInput.RandomSeed);
            AddOptionalInt(lines, "SimulationCount", request.SimulationCount);
            return;
        }

        if (request.RuleProfileAttributes.IsEmptyProfile) return;

        if (request.ScheduledMatchesInput is not null && !request.RuleProfileAttributes.UsesFinalStageGrouping)
        {
            lines.Add($"TournamentRuleSetMode={request.ScheduledMatchesInput.TournamentRuleSetMode}");
        }

        lines.Add($"FirstPlayerWinRatePercent={FormatNumber(request.FirstPlayerWinRatePercent)}");
        AddOptionalInt(lines, "SimulationCount", request.SimulationCount);

        if (request.RuleProfileAttributes.UsesFinalStageGrouping)
        {
            lines.Add($"AdditionalApexPlacementMode={request.AdditionalApexPlacement?.AdditionalApexPlacementMode ?? AdditionalApexPlacementMode.Off}");
            lines.Add($"BoundaryRescueMode={request.BoundaryRescue?.BoundaryRescueMode ?? BoundaryRescueMode.Off}");
        }
    }

    static void AddDeferredQualityMetaLines(
        List<string> lines,
        DeferredQualityEvaluationStepRequest request)
    {
        AddQualityExecutionMetaLines(lines, request.ExecutionOptions, request.OutputOptions);
        lines.Add($"TournamentRuleSetMode={request.RuleProfileAttributes.RankingRuleSetMode}");
        if (request.RuleProfileAttributes.UsesFinalStageGrouping)
        {
            lines.Add($"AdditionalApexPlacementMode={(request.RuleProfileAttributes.UsesAdditionalApexPlacement ? AdditionalApexPlacementMode.On : AdditionalApexPlacementMode.Off)}");
            lines.Add($"BoundaryRescueMode={(request.RuleProfileAttributes.UsesBoundaryRescue ? BoundaryRescueMode.On : BoundaryRescueMode.Off)}");
            lines.Add($"VariableTop8Mode={request.DeferredOptions.VariableTop8Mode}");
            lines.Add($"QualityInnovExpectedRankOffsetMode={request.DeferredOptions.InnovExpectedRankOffsetMode}");
        }
        else
        {
            lines.Add("QualityInnovExpectedRankOffsetMode=Off");
        }
    }    static void AddQualityMetaLines(
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
            case SimulationStepRequest request:
                AddSimulationBodySections(lines, sectionPrefix, outputSectionName, request);
                break;

            case QualityEvaluationStepRequest request:
                AddQualityBodySections(lines, sectionPrefix, outputSectionName, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            case DeferredQualityEvaluationStepRequest request:
                AddDeferredQualityBodySections(lines, outputSectionName, request.ExecutionOptions, request.OutputOptions);
                break;
        }
    }

    static void AddSimulationBodySections(
        List<string> lines,
        string sectionPrefix,
        string outputSectionName,
        SimulationStepRequest request)
    {
        if (request.TournamentFrameworkInput is not null)
        {
            AddInputsSection(lines, $"{sectionPrefix}Inputs", request.TournamentFrameworkInput);
            AddOutputSection(lines, outputSectionName, outputPath: request.OutputPath);
            return;
        }

        if (request.RuleProfileAttributes.IsEmptyProfile)
        {
            AddOutputSection(lines, outputSectionName, outputPath: request.OutputPath);
            return;
        }

        var input = request.ScheduledMatchesInput
            ?? throw new InvalidOperationException("シミュレーション要求の対局入力がありません。");

        AddPlayersSection(lines, $"{sectionPrefix}PlayersCsv", input.AllPlayers);
        if (request.RuleProfileAttributes.UsesFinalStageGrouping)
        {
            var grouping = request.FinalStageGrouping
                ?? throw new InvalidOperationException("本戦シミュレーション要求のグループ入力がありません。");
            AddGroupMapSection(lines, $"{sectionPrefix}GroupMapCsv", input.Players, grouping.GroupMap);
            AddOptionalPlayersSection(lines, $"{sectionPrefix}AdditionalApexPlayersCsv", request.AdditionalApexPlacement?.AdditionalApexPlayers ?? Array.Empty<Player>());
        }

        AddMatchesSection(lines, $"{sectionPrefix}MatchesInput", input.Players, input.Matches);
        AddOptionalMatchesSection(lines, $"{sectionPrefix}ReferenceMatchesInput", input.Players, input.ReferenceMatches);
        AddOutputSection(lines, outputSectionName, summaryOutputPath: request.OutputPath);
    }    static void AddDeferredQualityBodySections(
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

    static void AddInputsSection(List<string> lines, string sectionName, TournamentFrameworkSimulationInput request)
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


    static string GetStepSectionName(AnalysisStepRequest stepRequest)
    {
        return $"{GetStepName(stepRequest)}Step";
    }

    static string GetStepName(AnalysisStepRequest stepRequest)
    {
        return stepRequest switch
        {
            SimulationStepRequest => "Simulation",
            QualityEvaluationStepRequest => "QualityEvaluation",
            DeferredQualityEvaluationStepRequest => "QualityEvaluation",
            _ => throw new InvalidOperationException($"未対応の分析要求です: {stepRequest.GetType().Name}"),
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
