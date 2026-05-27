/*
 * ［アプリケーション　＞　入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class InputSourceConfiguration
{
    internal static void ConfigureInputSource(IReadOnlyList<string> args)
    {
        var inputFilePath = TryGetInputFilePathFromArgs(args);
        if (!string.IsNullOrWhiteSpace(inputFilePath))
        {
            ApplyInputFile(inputFilePath);
            return;
        }

        Console.WriteLine("入力方法を選んでください。");
        Console.WriteLine("1. そのまま入力する");
        Console.WriteLine("2. 入力ファイルを使う\n");

        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write("入力方法を選んでください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (input is null) throw new OperationCanceledException("入力方法の選択中に入力ストリームが終了しました。");

            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return;
            }

            if (input == "2")
            {
                var path = ReadInputFilePath();
                ApplyInputFile(path);
                return;
            }

            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("入力方法選択", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    static string ReadInputFilePath()
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write("入力ファイルのパスを入力してください: ");
            var input = Console.ReadLine()?.Trim();
            if (input is null) throw new OperationCanceledException("入力ファイルパスの入力中に入力ストリームが終了しました。");

            if (string.IsNullOrWhiteSpace(input))
            {
                if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("入力ファイルパス", "空欄のためファイルパスとして扱えません");

                Console.WriteLine("ファイルパスを入力してください。\n");
                continue;
            }

            return input;
        }
    }

    static string? TryGetInputFilePathFromArgs(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            if (arg.Equals("--input-file", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Count) throw new OperationCanceledException("--input-file の後ろにファイルパスを指定してください。");

                return args[index + 1];
            }

            const string inputFilePrefix = "--input-file=";
            if (arg.StartsWith(inputFilePrefix, StringComparison.OrdinalIgnoreCase)) return arg[inputFilePrefix.Length..];
        }

        return null;
    }

    static void ApplyInputFile(string inputFilePath)
    {
        var fullPath = Path.GetFullPath(inputFilePath);
        if (!File.Exists(fullPath)) throw new OperationCanceledException($"入力ファイルが見つかりません: {fullPath}");

        var rawLines = File.ReadAllLines(fullPath);
        var filteredInput = IsStsaInput3(rawLines)
            ? ConvertStsaInput3ToLegacyInput(rawLines, fullPath)
            : IsStsaInput2(rawLines)
                ? ConvertStsaInput2ToLegacyInput(rawLines, fullPath)
                : ConvertLegacyInputFileToFilteredInput(rawLines);

        Console.SetIn(new StringReader(filteredInput));
        Console.WriteLine($"入力ファイルを使います: {fullPath}\n");
    }

    static bool IsStsaInput2(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/2", StringComparison.OrdinalIgnoreCase));
    }

    static bool IsStsaInput3(IReadOnlyList<string> rawLines)
    {
        return rawLines.Any(line => line.Trim().Equals("#[Format] STSAInput/3", StringComparison.OrdinalIgnoreCase));
    }

    static string ConvertLegacyInputFileToFilteredInput(IEnumerable<string> rawLines)
    {
        var filteredLines = rawLines
            .Select(line => line.Trim().Equals("#[Enter]", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : line)
            .Where(line => !line.TrimStart().StartsWith('#'));

        return string.Join(Environment.NewLine, filteredLines);
    }

    static string ConvertStsaInput2ToLegacyInput(IReadOnlyList<string> rawLines, string fullPath)
    {
        return ConvertStsaInputToLegacyInput(rawLines, fullPath, "STSAInput/2");
    }

    static string ConvertStsaInput3ToLegacyInput(IReadOnlyList<string> rawLines, string fullPath)
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

    static Dictionary<string, List<string>> ParseStsaInputSections(IReadOnlyList<string> rawLines, string fullPath, string formatName)
    {
        var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        List<string>? currentLines = null;
        string? currentSectionName = null;
        var formatFound = false;

        foreach (var rawLine in rawLines)
        {
            var trimmed = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                currentLines?.Add(string.Empty);
                continue;
            }

            if (trimmed.Equals($"#[Format] {formatName}", StringComparison.OrdinalIgnoreCase))
            {
                formatFound = true;
                continue;
            }

            if (trimmed.StartsWith("#[Section]", StringComparison.OrdinalIgnoreCase))
            {
                if (currentLines is not null) throw new OperationCanceledException($"{formatName} のセクション '{currentSectionName}' が #[EndSection] で閉じられていません: {fullPath}");

                var sectionName = trimmed[11..].Trim();
                if (string.IsNullOrWhiteSpace(sectionName)) throw new OperationCanceledException($"{formatName} の #[Section] にセクション名がありません: {fullPath}");

                if (sections.ContainsKey(sectionName)) throw new OperationCanceledException($"{formatName} のセクション '{sectionName}' が重複しています: {fullPath}");

                currentSectionName = sectionName;
                currentLines = new List<string>();
                continue;
            }

            if (trimmed.Equals("#[EndSection]", StringComparison.OrdinalIgnoreCase))
            {
                if (currentLines is null || currentSectionName is null) throw new OperationCanceledException($"{formatName} の #[EndSection] に対応する #[Section] がありません: {fullPath}");

                sections[currentSectionName] = currentLines;
                currentLines = null;
                currentSectionName = null;
                continue;
            }

            if (trimmed.StartsWith('#')) continue;

            if (currentLines is null) throw new OperationCanceledException($"{formatName} の制御タグ外に本文があります: {rawLine}");

            currentLines.Add(rawLine);
        }

        if (!formatFound) throw new OperationCanceledException($"{formatName} の #[Format] 宣言が見つかりません: {fullPath}");

        if (currentLines is not null) throw new OperationCanceledException($"{formatName} のセクション '{currentSectionName}' が #[EndSection] で閉じられていません: {fullPath}");

        return sections;
    }

    static Dictionary<string, string> ParseSectionKeyValues(IReadOnlyList<string> lines, string sectionName, string fullPath, string formatName)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0) throw new OperationCanceledException($"{formatName} の {sectionName} セクションで key=value 形式ではない行があります: {line} ({fullPath})");

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            if (values.ContainsKey(key)) throw new OperationCanceledException($"{formatName} の {sectionName} セクションでキー '{key}' が重複しています: {fullPath}");

            values[key] = value;
        }

        return values;
    }

    static IReadOnlyList<string> GetRequiredSectionLines(Dictionary<string, List<string>> sections, string sectionName, string fullPath, string formatName)
    {
        if (!sections.TryGetValue(sectionName, out var lines)) throw new OperationCanceledException($"{formatName} の必須セクション '{sectionName}' がありません: {fullPath}");

        return lines;
    }

    static IReadOnlyList<string> GetOptionalSectionLines(Dictionary<string, List<string>> sections, string sectionName)
    {
        return sections.TryGetValue(sectionName, out var lines)
            ? lines
            : Array.Empty<string>();
    }

    static string GetRequiredMetaValue(Dictionary<string, string> meta, string key, string fullPath, string formatName)
    {
        if (!meta.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) throw new OperationCanceledException($"{formatName} の Meta セクションに必須キー '{key}' がありません: {fullPath}");

        return value;
    }

    static string? GetOptionalMetaValue(Dictionary<string, string> meta, string key)
    {
        return meta.TryGetValue(key, out var value)
            ? value
            : null;
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

    static void AppendDelimitedSection(List<string> destination, IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            destination.Add(line);
        }

        destination.Add(string.Empty);
    }

    static void AppendEndTerminatedSection(List<string> destination, IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            destination.Add(line);
        }

        destination.Add("END");
    }

    static AnalysisFlowMode ParseAnalysisFlowMode(string value, string formatName)
    {
        if (value.Equals("QualityEvaluation", StringComparison.OrdinalIgnoreCase) || value == "2") return AnalysisFlowMode.QualityEvaluation;

        if (value.Equals("Simulation", StringComparison.OrdinalIgnoreCase) || value == "1") return AnalysisFlowMode.Simulation;

        throw new OperationCanceledException($"{formatName} の AnalysisFlowMode の値が解釈できません: {value}");
    }

    static RuleProfileMode ParseRuleProfileMode(string value, string formatName)
    {
        if (value.Equals("Empty", StringComparison.OrdinalIgnoreCase) || value == "4") return RuleProfileMode.Empty;

        if (value.Equals("TournamentFramework", StringComparison.OrdinalIgnoreCase) || value == "3") return RuleProfileMode.TournamentFramework;

        if (value.Equals("FinalStage", StringComparison.OrdinalIgnoreCase) || value == "2") return RuleProfileMode.FinalStage;

        if (value.Equals("Standard", StringComparison.OrdinalIgnoreCase) || value == "1") return RuleProfileMode.Standard;

        throw new OperationCanceledException($"{formatName} の RuleProfileMode の値が解釈できません: {value}");
    }

    static string ParseOffOnSelection(string value, string offNumber, string onNumber, string keyName, string formatName)
    {
        if (value.Equals("Off", StringComparison.OrdinalIgnoreCase) || value == offNumber) return offNumber;

        if (value.Equals("On", StringComparison.OrdinalIgnoreCase) || value == onNumber) return onNumber;

        throw new OperationCanceledException($"{formatName} の {keyName} の値が解釈できません: {value}");
    }

    static string ParseGoodBadSelection(string value, string keyName, string formatName)
    {
        if (value.Equals("Good", StringComparison.OrdinalIgnoreCase) || value == "1") return "1";

        if (value.Equals("Bad", StringComparison.OrdinalIgnoreCase) || value == "2") return "2";

        throw new OperationCanceledException($"{formatName} の {keyName} の値が解釈できません: {value}");
    }

    static string ParseTournamentRuleSetSelection(string value, string formatName)
    {
        if (value.Equals("Neutral", StringComparison.OrdinalIgnoreCase) || value == "1") return "1";

        if (value.Equals("Twill", StringComparison.OrdinalIgnoreCase) || value == "2") return "2";

        if (value.Equals("TwillCommonOpponentWeighted", StringComparison.OrdinalIgnoreCase)
            || value.Equals("TwillCommonOpp", StringComparison.OrdinalIgnoreCase)
            || value == "3") return "3";

        throw new OperationCanceledException($"{formatName} の TournamentRuleSetMode の値が解釈できません: {value}");
    }
}

