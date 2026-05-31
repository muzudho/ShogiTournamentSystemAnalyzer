/*
 * ［アプリケーション　＞　分析　＞　分析ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［分析］という責務
/// </summary>
internal static class AnalysisWorkflow
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