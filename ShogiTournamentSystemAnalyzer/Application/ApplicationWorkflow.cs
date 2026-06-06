namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;
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

        // メインライン選択のガイドを表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintSelectedMainline(tournamentUserDomainResult.AnalysisFlowSelection, tournamentUserDomainResult.RuleProfileMode);

        Console.WriteLine("■［分析］"); // TODO: この出力、消していいかだぜ（＾～＾）？

        // ［要求ファイル］から読んだ STSAInput/4 直通要求は、要求側の Steps リスト構造で実行する。
        if (tournamentUserDomainResult.AnalysisRequest is not null)
        {
            AnalysisRequestDispatcher.Execute(tournamentUserDomainResult.AnalysisRequest);
            return;
        }

        //┌───┴─────┐
        //│シミュレーション域│
        //└───┬─────┘
        ExecuteSimulationDomain(tournamentUserDomainResult.AnalysisFlowSelection, tournamentUserDomainResult.RuleProfileMode);

        //┌───┴─────┐
        //│最終順位付け域　　│
        //└───┬─────┘
        ExecuteFinalRankingDomain();

        //┌───┴─────┐
        //│大会品質評価域　　│
        //└───┬─────┘
        ExecuteQualityEvaluationDomain(tournamentUserDomainResult.AnalysisFlowSelection, tournamentUserDomainResult.RuleProfileMode);

        // ローカル関数

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
    /// ［シミュレーション域］実行
    /// </summary>
    /// <param name="flowSelection"></param>
    /// <param name="ruleProfileMode"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ExecuteSimulationDomain(
        AnalysisFlowSelection flowSelection,
        RuleProfileMode ruleProfileMode)
    {
        if (!flowSelection.RunsSimulation) return;
        if (SimulationFlowDispatcher.TryExecute(AnalysisFlowMode.Simulation, ruleProfileMode)) return;

        throw new InvalidOperationException("未対応のシミュレーション域です。");
    }

    /// <summary>
    /// ［最終順位付け域］実行
    /// </summary>
    private static void ExecuteFinalRankingDomain()
    {
        // 現時点の手入力フローでは、最終順位付け域の処理はシミュレーション域の中から呼ばれる。
        // アプリケーション直下の順序としてはここに置き、後続分離時の差し込み位置を固定する。
    }

    /// <summary>
    /// ［大会品質評価域］実行
    /// </summary>
    /// <param name="flowSelection"></param>
    /// <param name="ruleProfileMode"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ExecuteQualityEvaluationDomain(
        AnalysisFlowSelection flowSelection,
        RuleProfileMode ruleProfileMode)
    {
        if (!flowSelection.RunsQualityEvaluation) return;
        if (QualityEvaluationFlowDispatcher.TryExecute(AnalysisFlowMode.QualityEvaluation, ruleProfileMode)) return;

        throw new InvalidOperationException("未対応の大会品質評価域です。");
    }
}
