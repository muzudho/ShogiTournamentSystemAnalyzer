namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
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
        // メインライン選択のガイドを表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintSelectedMainline(tournamentUserDomainResult.AnalysisFlowSelection, tournamentUserDomainResult.RuleProfileMode);

        Console.WriteLine("■［分析］"); // TODO: この出力、消していいかだぜ（＾～＾）？

        // ［要求ファイル］から読んだ STSAInput/4 直通要求は、要求側の Steps リスト構造で実行する。
        if (tournamentUserDomainResult.AnalysisRequest is not null)
        {
            AnalysisRequestDispatcher.Execute(tournamentUserDomainResult.AnalysisRequest);
            return;
        }

        // 手入力などの旧入口は、アプリケーション上の［３大域］固定シーケンスとして実行する。
        new AnalysisDomainSequence(
            tournamentUserDomainResult.AnalysisFlowSelection,
            tournamentUserDomainResult.RuleProfileMode)
            .Execute();

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

}
