/*
 * ［アプリケーション　＞　実行　＞　分析］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class Analysis
{
    internal static void Run()
    {
        Console.WriteLine("■［分析］");

        // 前提入力は TournamentRule / PlayerList / RankingSettings の３境界だぜ（＾▽＾）！
        // 主線は TournamentFinalState → FinalRanking → TournamentQualityReport に寄せていくぜ（＾▽＾）！
        var flowMode = ConsolePromptReaders.ReadAnalysisFlowMode();

        // 対象［大会ルール］を選ばせるぜ（＾▽＾）！
        var ruleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(flowMode);

        // メインライン選択のガイドを表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintSelectedMainline(flowMode, ruleProfileMode);

        // 選択フロー
        AnalysisFlowDispatcher.Execute(flowMode, ruleProfileMode);
    }
}