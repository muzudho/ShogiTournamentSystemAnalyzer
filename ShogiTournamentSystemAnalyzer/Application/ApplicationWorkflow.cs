namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;
using System.Text;

internal static class ApplicationWorkflow
{
    public static void Run(string[] args)
    {
        // ●開始

        //┌───┴──┐
        //│オープニング│
        //└───┬──┘
        Opening();

        // （開発用の機能）
        // PowerShell reflection smoke の代わりに、通常の .NET 実行入口として使う。
        if (RequestWriterRoundTripSmoke.TryRun(args)) return;

        //┌───┴──┐
        //│大会利用者域│
        //└───┬──┘
        if (!ApplicationTournamentUser.TryRunTournamentUserDomain(args, out var tournamentUserDomainResult)) return;  // エラー終了

        //┌───┴──┐
        //│分析　　　　│
        //└───┬──┘
        RunAnalysisDomain(
            tournamentUserDomainResult.AnalysisFlowSelection,
            tournamentUserDomainResult.RuleProfileMode,
            tournamentUserDomainResult.AnalysisRequest);

        // ●終了
        return;
    }

    /// <summary>
    /// ［開始］
    /// </summary>
    private static void Opening()
    {
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        InputFromSomewhere.UseConsole();

        // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
        SimulationTimeBudget.BeginApplicationBudget();

        // このプログラムの説明を最初に表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintProgramIntroduction();
    }

    /// <summary>
    /// ［分析］
    /// </summary>
    private static void RunAnalysisDomain(
        AnalysisFlowSelection analysisFlowSelection,
        RuleProfileMode ruleProfileMode,
        AnalysisRequest? analysisRequest)
    {
        // メインライン選択のガイドを表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintSelectedMainline(analysisFlowSelection, ruleProfileMode);

        //      │
        //      ↓
        //      
        Console.WriteLine("■［分析］");
        //TournamentFinalStateBoundary tournamentFinalStateBoundary = new();
        //［シミュレーション域］
        //　　｜
        //　　｜　［大会最終状態という境界］      `TournamentFinalState`
        //　　↓
        //［順位付け域］
        //　　｜
        //　　｜　［最終順位という境界］          `FinalRanking`
        //　　↓
        //［大会品質評価フロー域］                `TournamentQualityEvaluator`
        //　　｜
        //　　｜　［大会品質レポートという境界］  `TournamentQualityReport`
        //　　↓


        // 本処理（選択フロー）
        if (analysisRequest is not null)
        {
            AnalysisRequestDispatcher.Execute(analysisRequest);
            return;
        }

        AnalysisFlowDispatcher.Execute(analysisFlowSelection, ruleProfileMode);

    }
}
