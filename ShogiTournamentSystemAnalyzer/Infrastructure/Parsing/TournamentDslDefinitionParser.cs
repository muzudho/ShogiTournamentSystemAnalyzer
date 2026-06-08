/*
 * ［インフラストラクチャー　＞　パーシング］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;

using ShogiTournamentSystemAnalyzer.Application.Simulation.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal static class TournamentDslDefinitionParser
{
    internal static TournamentDslDefinition ParseTournamentDsl(string text, string sourceLabel)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var stages = new List<StageEntry>();
        var flowSteps = new List<string>();
        var pairingRuleNames = new Dictionary<int, string>();
        var timeAxis = "Tick";
        var defaultMatchResultResolver = "EloFirstPlayerWinRate";
        var overallRankingRuleName = "ByFinishedResults";
        var terminationRuleName = "AllMatchesFinished";
        string? currentSection = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

            if (line.Equals("TournamentRule/1", StringComparison.OrdinalIgnoreCase)) continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line[1..^1].Trim();
                continue;
            }

            switch (currentSection)
            {
                case "Engine":
                    ParseEngineLine(line, sourceLabel, ref timeAxis, ref defaultMatchResultResolver);
                    break;
                case "Stages":
                    stages.Add(ParseStageLine(line, sourceLabel));
                    break;
                case "Flow":
                    if (!line.EndsWith(':'))
                    {
                        flowSteps.Add(line);
                    }
                    break;
                case "PairingRule":
                    ParsePairingRuleLine(line, sourceLabel, pairingRuleNames);
                    break;
                case "RankingRule":
                    ParseRankingRuleLine(line, ref overallRankingRuleName);
                    break;
                case "TerminationRule":
                    terminationRuleName = line;
                    break;
            }
        }

        return new TournamentDslDefinition(
            timeAxis,
            defaultMatchResultResolver,
            stages,
            flowSteps,
            pairingRuleNames,
            overallRankingRuleName,
            terminationRuleName);
    }

    static void ParseEngineLine(string line, string sourceLabel, ref string timeAxis, ref string defaultMatchResultResolver)
    {
        var pair = SplitKeyValue(line, sourceLabel, "Engine");
        if (pair.Key.Equals("TimeAxis", StringComparison.OrdinalIgnoreCase))
        {
            timeAxis = pair.Value;
            return;
        }

        if (pair.Key.Equals("DefaultMatchResultResolver", StringComparison.OrdinalIgnoreCase))
        {
            defaultMatchResultResolver = pair.Value;
        }
    }

    static StageEntry ParseStageLine(string line, string sourceLabel)
    {
        if (!line.StartsWith("Stage(", StringComparison.OrdinalIgnoreCase) || !line.EndsWith(')')) throw new OperationCanceledException($"DSL の Stage 行が不正です: {line} ({sourceLabel})");

        var inner = line[6..^1];
        var columns = InputParsers.SplitCsvLine(inner);
        if (columns.Count < 3) throw new OperationCanceledException($"DSL の Stage 行は 3 項目以上必要です: {line} ({sourceLabel})");

        if (!int.TryParse(columns[0].Trim(), out var stageId)) throw new OperationCanceledException($"DSL の Stage id を整数で入力してください: {line} ({sourceLabel})");

        var stageName = columns[1].Trim().Trim('"');
        var stageType = columns[2].Trim();
        return new StageEntry(stageId, stageName, stageType, null, stageId);
    }

    static void ParsePairingRuleLine(string line, string sourceLabel, Dictionary<int, string> pairingRuleNames)
    {
        if (!line.StartsWith("When ", StringComparison.OrdinalIgnoreCase)) return;

        var parts = line.Split(':', 2);
        if (parts.Length != 2) throw new OperationCanceledException($"DSL の PairingRule 行が不正です: {line} ({sourceLabel})");

        var left = parts[0].Trim();
        var right = parts[1].Trim();
        const string prefix = "When Stage=";
        if (!left.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new OperationCanceledException($"DSL の PairingRule 行が不正です: {line} ({sourceLabel})");

        if (!int.TryParse(left[prefix.Length..].Trim(), out var stageId)) throw new OperationCanceledException($"DSL の PairingRule の stageId を整数で入力してください: {line} ({sourceLabel})");

        pairingRuleNames[stageId] = right;
    }

    static void ParseRankingRuleLine(string line, ref string overallRankingRuleName)
    {
        var pair = SplitKeyValue(line, sourceLabel: "RankingRule", sectionName: "RankingRule");
        if (pair.Key.Equals("OverallRanking", StringComparison.OrdinalIgnoreCase))
        {
            overallRankingRuleName = pair.Value;
        }
    }

    static KeyValuePair<string, string> SplitKeyValue(string line, string sourceLabel, string sectionName)
    {
        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0) throw new OperationCanceledException($"DSL の {sectionName} セクションで key=value 形式ではない行があります: {line} ({sourceLabel})");

        return new KeyValuePair<string, string>(
            line[..separatorIndex].Trim(),
            line[(separatorIndex + 1)..].Trim());
    }
}
