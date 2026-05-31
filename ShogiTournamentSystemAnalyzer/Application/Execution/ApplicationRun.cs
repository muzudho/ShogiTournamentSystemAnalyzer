/*
 * ［アプリケーション　＞　実行　＞　アプリケーション実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

using ShogiTournamentSystemAnalyzer.Application.Input;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ApplicationRun
{
    internal static void Run(IReadOnlyList<string> args)
    {
        // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
        SimulationTimeBudget.BeginApplicationBudget();

        // このプログラムの説明を最初に表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintProgramIntroduction();

        // ［依頼］が［要求ファイル］からか、［対話か］に応じて、入力導線を準備するぜ（＾▽＾）！
        using var inputSource = InputSourceConfiguration.ConfigureInputSource(args);

        // ［依頼］を受け取って分析を始めるぜ（＾▽＾）！
        Analysis.Run();

        // 入力セッションの後片付けや完了処理を行うぜ（＾▽＾）！
        inputSource.Complete();
    }
}