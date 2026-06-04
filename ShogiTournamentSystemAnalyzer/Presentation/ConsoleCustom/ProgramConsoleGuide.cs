/*
 * ［プレゼンテーション　＞　コンソール改］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using System;

internal static class ProgramConsoleGuide
{
    internal static void PrintProgramIntroduction()
    {
        Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！");
        Console.WriteLine();
        Console.WriteLine("前提入力: TournamentRule / PlayerList / RankingSettings");
        Console.WriteLine("主線: TournamentFinalState -> FinalRanking -> TournamentQualityReport\n");
    }

    internal static void PrintSelectedMainline(AnalysisFlowSelection flowSelection, RuleProfileMode ruleProfileMode)
    {
        var profileLabel = ruleProfileMode switch
        {
            RuleProfileMode.Standard => "Standard",
            RuleProfileMode.FinalStage => "FinalStage",
            RuleProfileMode.TournamentFramework => "TournamentFramework",
            RuleProfileMode.Empty => "Empty",
            _ => ruleProfileMode.ToString(),
        };

        var mainlineLabel = string.Join(" -> ", flowSelection.Steps.Select(step => step switch
        {
            AnalysisFlowMode.Simulation => "TournamentFinalState -> FinalRanking",
            AnalysisFlowMode.QualityEvaluation => "TournamentQualityReport",
            _ => step.ToString(),
        }));

        Console.WriteLine($"選択された主線: {profileLabel} / {flowSelection.ToPromptLabel()} / {mainlineLabel}\n");
    }
}