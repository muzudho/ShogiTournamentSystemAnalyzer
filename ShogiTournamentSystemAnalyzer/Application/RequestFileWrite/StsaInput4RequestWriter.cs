/*
 * ［アプリケーション　＞　要求ファイル書出　＞　STSAInput/4］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using System.Globalization;
using Match = ShogiTournamentSystemAnalyzer.Domain.Simulation.Match;

internal static class StsaInput4RequestWriter
{
    internal static IReadOnlyList<string> BuildLines(AnalysisRequest request)
    {
        if (request.Steps.Count != 1) throw new InvalidOperationException("STSAInput/4 の手入力保存は単一分析ステップだけ対応しています。");

        var lines = new List<string>
        {
            "#[Format] STSAInput/4",
            string.Empty,
            "#[Section] Meta",
            $"AnalysisFlowSteps={request.FlowSelection.ToRequestFileValue()}",
            $"RuleProfileMode={request.RuleProfileMode}",
        };

        AddMetaLines(lines, request.Steps[0]);
        lines.Add("#[EndSection]");

        AddBodySections(lines, request.Steps[0]);
        return lines;
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

            case FinalStageQualityEvaluationRequest request:
                AddQualityMetaLines(lines, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            default:
                throw new InvalidOperationException($"未対応の分析要求です: {step.GetType().Name}");
        }
    }

    static void AddQualityMetaLines(
        List<string> lines,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        lines.Add($"ExecutionMode={(executionOptions.IsSweep ? "Sweep" : "Single")}");
        lines.Add($"TournamentRuleSetMode={ruleDefinition.TournamentRuleSetMode}");
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

    static void AddBodySections(List<string> lines, AnalysisStepRequest step)
    {
        switch (step)
        {
            case StandardSimulationRequest request:
                AddPlayersSection(lines, "PlayersCsv", request.AllPlayers);
                AddMatchesSection(lines, "MatchesInput", request.Players, request.Matches);
                AddOutputSection(lines, summaryOutputPath: request.OutputPath);
                break;

            case FinalStageSimulationRequest request:
                AddPlayersSection(lines, "PlayersCsv", request.Players);
                AddGroupMapSection(lines, request.Players, request.GroupMap);
                AddOptionalPlayersSection(lines, "AdditionalApexPlayersCsv", request.AdditionalApexPlayers);
                AddMatchesSection(lines, "MatchesInput", request.Players, request.Matches);
                AddOptionalMatchesSection(lines, "ReferenceMatchesInput", request.Players, request.ReferenceMatches);
                AddOutputSection(lines, summaryOutputPath: request.OutputPath);
                break;

            case TournamentFrameworkSimulationRequest request:
                AddInputsSection(lines, request);
                AddOutputSection(lines, outputPath: request.OutputPath);
                break;

            case EmptySimulationRequest request:
                AddOutputSection(lines, outputPath: request.OutputPath);
                break;

            case StandardQualityEvaluationRequest request:
                AddQualityBodySections(lines, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;

            case FinalStageQualityEvaluationRequest request:
                AddQualityBodySections(lines, request.RuleDefinition, request.Input, request.ExecutionOptions, request.OutputOptions);
                break;
        }
    }

    static void AddQualityBodySections(
        List<string> lines,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationExecutionOptions executionOptions,
        TournamentQualityEvaluationOutputOptions outputOptions)
    {
        AddPlayersSection(lines, "PlayersCsv", input.Players);
        if (ruleDefinition.UsesFinalStageGrouping)
        {
            AddGroupMapSection(lines, input.Players, ruleDefinition.GroupMap!);
            AddOptionalPlayersSection(lines, "AdditionalApexPlayersCsv", ruleDefinition.AdditionalApexPlayers);
        }

        AddMatchesSection(lines, "MatchesInput", input.Players, input.Matches);
        AddOptionalMatchesSection(lines, "ReferenceMatchesInput", input.Players, input.ReferenceMatches);
        AddOutputSection(
            lines,
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

    static void AddGroupMapSection(List<string> lines, IReadOnlyList<Player> players, IReadOnlyDictionary<string, FinalStageGroup> groupMap)
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

        AddSection(lines, "GroupMapCsv", body);
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

    static void AddInputsSection(List<string> lines, TournamentFrameworkSimulationRequest request)
    {
        var body = new List<string>
        {
            $"PlayersCsvPath={request.PlayersCsvPath}",
            $"StagesCsvPath={request.StagesCsvPath}",
            $"TournamentMatchRecordsCsvPath={request.TournamentMatchRecordsCsvPath}",
        };
        AddOptionalString(body, "RuleFilePath", request.RuleFilePath);
        AddSection(lines, "Inputs", body);
    }

    static void AddOutputSection(
        List<string> lines,
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
        if (body.Count > 0) AddSection(lines, "Output", body);
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
