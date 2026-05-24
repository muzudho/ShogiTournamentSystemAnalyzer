/*
 * ［大会品質評価フロー域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class TournamentQualityEvaluationMode
{
    internal static void Run(RuleProfileMode ruleProfileMode)
    {
        if (ruleProfileMode == RuleProfileMode.Standard)
        {
            Console.WriteLine("品質評価 / 通常ルール: 総当たり戦向けルールの実力反映性を評価します。\n");
            ConsoleSamplePrinter.PrintQualityEvaluationStandardOverview();
        }
        else
        {
            Console.WriteLine("品質評価 / 本戦ルール: 本戦ルールの実力反映性を評価します。\n");
            ConsoleSamplePrinter.PrintQualityEvaluationFinalStageOverview();
        }

        TournamentQualityEvaluationMainline.Run(ruleProfileMode);
    }

}

