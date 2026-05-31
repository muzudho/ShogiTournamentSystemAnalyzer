/*
 * ［アプリケーション　＞　実行　＞　アプリケーション起動準備］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ApplicationStartup
{
    internal static void Start()
    {
        // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
        SimulationTimeBudget.BeginApplicationBudget();

        // このプログラムの説明を最初に表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintProgramIntroduction();
    }
}