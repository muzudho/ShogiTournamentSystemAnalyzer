/*
 * ［プレゼンテーション　＞　オーケストレーション］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static class ProgramConsoleGuide
{
    internal static void PrintProgramIntroduction()
    {
        Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！");
        Console.WriteLine();
        Console.WriteLine("前提入力: TournamentRule / PlayerList / RankingSettings");
        Console.WriteLine("主線: TournamentFinalState -> FinalRanking -> TournamentQualityReport\n");
    }

    internal static void PrintSelectedMainline(AnalysisFlowMode flowMode, RuleProfileMode ruleProfileMode)
    {
        var profileLabel = ruleProfileMode switch
        {
            RuleProfileMode.Standard => "Standard",
            RuleProfileMode.FinalStage => "FinalStage",
            RuleProfileMode.TournamentFramework => "TournamentFramework",
            RuleProfileMode.Empty => "Empty",
            _ => ruleProfileMode.ToString(),
        };

        var mainlineLabel = flowMode switch
        {
            AnalysisFlowMode.Simulation => "TournamentFinalState -> FinalRanking",
            AnalysisFlowMode.QualityEvaluation => "TournamentFinalState -> FinalRanking -> TournamentQualityReport",
            _ => "TournamentFinalState -> FinalRanking"
        };

        Console.WriteLine($"選択された主線: {profileLabel} / {mainlineLabel}\n");
    }
}
