/*
 * ［プレゼンテーション　＞　コンソール改］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using System;
using static ShogiTournamentSystemAnalyzer.Application.ApplicationTournamentUser;

internal static class ProgramConsoleGuide
{
    internal static void PrintProgramIntroduction()
    {
        Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！");
        //Console.WriteLine();
        //Console.WriteLine("前提入力: TournamentRule / PlayerList / RankingSettings");
        //Console.WriteLine("主線: TournamentFinalState -> FinalRanking -> TournamentQualityReport\n");
    }

    internal static void PrintSelectedMainline(TournamentUserDomainResult result)
    {
        if (result.AnalysisFlowSelection is null) throw new InvalidOperationException("分析フローが選択されていません。");

        var ruleProfileAttributes = GetSelectedRuleProfileAttributes(result);
        var profileLabel = ruleProfileAttributes.SimulationShape switch
        {
            RuleProfileSimulationShape.ScheduledMatches when ruleProfileAttributes.UsesFinalStageGrouping => "FinalStage",
            RuleProfileSimulationShape.ScheduledMatches => "Standard",
            RuleProfileSimulationShape.FinalStageGrouped => "FinalStage",
            RuleProfileSimulationShape.TournamentFramework => "TournamentFramework",
            RuleProfileSimulationShape.Empty => "Empty",
            _ => ruleProfileAttributes.SimulationShape.ToString(),
        };

        var mainlineLabel = string.Join(" -> ", result.AnalysisFlowSelection.Steps.Select(step => step switch
        {
            AnalysisFlowMode.Simulation => "TournamentFinalState -> FinalRanking",
            AnalysisFlowMode.QualityEvaluation => "TournamentQualityReport",
            _ => step.ToString(),
        }));

        Console.WriteLine($"選択された主線: {profileLabel} / {result.AnalysisFlowSelection.ToPromptLabel()} / {mainlineLabel}\n");
    }

    static RuleProfileAttributes GetSelectedRuleProfileAttributes(TournamentUserDomainResult result)
    {
        if (result.AnalysisRequest is null) return result.RuleProfileAttributes ?? throw new InvalidOperationException("ルールプロファイル属性が選択されていません。");
        if (result.AnalysisRequest.StepRequests.Count == 0) throw new InvalidOperationException("分析要求にステップ要求がありません。");

        return result.AnalysisRequest.StepRequests[0].GetRuleProfileAttributes();
    }
}
