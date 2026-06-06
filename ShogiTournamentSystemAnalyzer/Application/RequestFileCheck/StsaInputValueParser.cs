/*
 * ［アプリケーション　＞　入力　＞　STSA入力値パーサー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static class StsaInputValueParser
{
    internal static AnalysisFlowMode ParseAnalysisFlowMode(string value, string formatName)
    {
        if (value.Equals("QualityEvaluation", StringComparison.OrdinalIgnoreCase) || value == "2") return AnalysisFlowMode.QualityEvaluation;

        if (value.Equals("Simulation", StringComparison.OrdinalIgnoreCase) || value == "1") return AnalysisFlowMode.Simulation;

        throw new OperationCanceledException($"{formatName} の AnalysisFlowMode の値が解釈できません: {value}");
    }


    internal static AnalysisFlowSelection ParseAnalysisFlowSteps(string value, string formatName)
    {
        var steps = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(step => ParseAnalysisFlowMode(step, formatName))
            .ToArray();

        if (steps.Length == 0) throw new OperationCanceledException($"{formatName} の AnalysisFlowSteps が空です。");

        return new AnalysisFlowSelection(steps);
    }
    internal static RuleProfileAttributes ParseRuleProfileAttributesFromCompatibilityLabel(string value, string formatName)
    {
        return RuleProfileAttributes.FromCompatibilityLabel(ParseRuleProfileMode(value, formatName));
    }

    static RuleProfileMode ParseRuleProfileMode(string value, string formatName)
    {
        if (value.Equals("Empty", StringComparison.OrdinalIgnoreCase) || value == "4") return RuleProfileMode.Empty;

        if (value.Equals("TournamentFramework", StringComparison.OrdinalIgnoreCase) || value == "3") return RuleProfileMode.TournamentFramework;

        if (value.Equals("FinalStage", StringComparison.OrdinalIgnoreCase) || value == "2") return RuleProfileMode.FinalStage;

        if (value.Equals("Standard", StringComparison.OrdinalIgnoreCase) || value == "1") return RuleProfileMode.Standard;

        throw new OperationCanceledException($"{formatName} の RuleProfileMode の値が解釈できません: {value}");
    }

    internal static RuleProfileSimulationShape ParseRuleProfileSimulationShape(string value, string formatName)
    {
        if (value.Equals("ScheduledMatches", StringComparison.OrdinalIgnoreCase) || value == "1") return RuleProfileSimulationShape.ScheduledMatches;

        if (value.Equals("FinalStageGrouped", StringComparison.OrdinalIgnoreCase) || value == "2") return RuleProfileSimulationShape.FinalStageGrouped;

        if (value.Equals("TournamentFramework", StringComparison.OrdinalIgnoreCase) || value == "3") return RuleProfileSimulationShape.TournamentFramework;

        if (value.Equals("Empty", StringComparison.OrdinalIgnoreCase) || value == "4") return RuleProfileSimulationShape.Empty;

        throw new OperationCanceledException($"{formatName} の RuleProfileAttributes.SimulationShape の値が解釈できません: {value}");
    }

    internal static RuleProfilePairingSource ParseRuleProfilePairingSource(string value, string formatName)
    {
        if (value.Equals("None", StringComparison.OrdinalIgnoreCase) || value == "1") return RuleProfilePairingSource.None;

        if (value.Equals("ScheduledMatches", StringComparison.OrdinalIgnoreCase) || value == "2") return RuleProfilePairingSource.ScheduledMatches;

        if (value.Equals("TournamentFramework", StringComparison.OrdinalIgnoreCase) || value == "3") return RuleProfilePairingSource.TournamentFramework;

        throw new OperationCanceledException($"{formatName} の RuleProfileAttributes.PairingSource の値が解釈できません: {value}");
    }

    internal static TournamentRuleSetMode ParseTournamentRuleSetModeValue(string value, string formatName)
    {
        return ParseTournamentRuleSetSelection(value, formatName) switch
        {
            "2" => TournamentRuleSetMode.Twill,
            "3" => TournamentRuleSetMode.TwillCommonOpponentWeighted,
            _ => TournamentRuleSetMode.Neutral,
        };
    }

    internal static bool ParseOnOffBool(string value, string keyName, string formatName)
    {
        return ParseOffOnSelection(value, offNumber: "1", onNumber: "2", keyName, formatName) == "2";
    }

    internal static string ParseOffOnSelection(string value, string offNumber, string onNumber, string keyName, string formatName)
    {
        if (value.Equals("Off", StringComparison.OrdinalIgnoreCase) || value == offNumber) return offNumber;

        if (value.Equals("On", StringComparison.OrdinalIgnoreCase) || value == onNumber) return onNumber;

        throw new OperationCanceledException($"{formatName} の {keyName} の値が解釈できません: {value}");
    }

    internal static string ParseGoodBadSelection(string value, string keyName, string formatName)
    {
        if (value.Equals("Good", StringComparison.OrdinalIgnoreCase) || value == "1") return "1";

        if (value.Equals("Bad", StringComparison.OrdinalIgnoreCase) || value == "2") return "2";

        throw new OperationCanceledException($"{formatName} の {keyName} の値が解釈できません: {value}");
    }

    internal static string ParseTournamentRuleSetSelection(string value, string formatName)
    {
        if (value.Equals("Neutral", StringComparison.OrdinalIgnoreCase) || value == "1") return "1";

        if (value.Equals("Twill", StringComparison.OrdinalIgnoreCase) || value == "2") return "2";

        if (value.Equals("TwillCommonOpponentWeighted", StringComparison.OrdinalIgnoreCase)
            || value.Equals("TwillCommonOpp", StringComparison.OrdinalIgnoreCase)
            || value == "3") return "3";

        throw new OperationCanceledException($"{formatName} の TournamentRuleSetMode の値が解釈できません: {value}");
    }

}