/*
 * ［アプリケーション　＞　入力　＞　STSA入力値パーサー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class StsaInputValueParser
{
    internal static AnalysisFlowMode ParseAnalysisFlowMode(string value, string formatName)
    {
        if (value.Equals("QualityEvaluation", StringComparison.OrdinalIgnoreCase) || value == "2") return AnalysisFlowMode.QualityEvaluation;

        if (value.Equals("Simulation", StringComparison.OrdinalIgnoreCase) || value == "1") return AnalysisFlowMode.Simulation;

        throw new OperationCanceledException($"{formatName} の AnalysisFlowMode の値が解釈できません: {value}");
    }

    internal static RuleProfileMode ParseRuleProfileMode(string value, string formatName)
    {
        if (value.Equals("Empty", StringComparison.OrdinalIgnoreCase) || value == "4") return RuleProfileMode.Empty;

        if (value.Equals("TournamentFramework", StringComparison.OrdinalIgnoreCase) || value == "3") return RuleProfileMode.TournamentFramework;

        if (value.Equals("FinalStage", StringComparison.OrdinalIgnoreCase) || value == "2") return RuleProfileMode.FinalStage;

        if (value.Equals("Standard", StringComparison.OrdinalIgnoreCase) || value == "1") return RuleProfileMode.Standard;

        throw new OperationCanceledException($"{formatName} の RuleProfileMode の値が解釈できません: {value}");
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