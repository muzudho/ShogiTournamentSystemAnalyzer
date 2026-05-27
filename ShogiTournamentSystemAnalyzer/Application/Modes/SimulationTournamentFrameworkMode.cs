/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes;

using ShogiTournamentSystemAnalyzer.Presentation.Console;

internal static partial class SimulationTournamentFrameworkMode
{
    internal static void Run()
    {
        Console.WriteLine("対局シミュレーション / 大会進行フレームワーク: 一般化した大会進行モデルで大会記録を実行します。\n");
        ConsoleSamplePrinter.PrintSimulationTournamentFrameworkOverview();
        var context = SimulationModeInputReaders.ReadTournamentFrameworkModeContext();
        RunMainlineToTournamentFinalStateAndFinalRanking(context);
    }

    static void RunMainlineToTournamentFinalStateAndFinalRanking(TournamentFrameworkModeContext context)
    {
        ExecuteTournamentFrameworkMode(context);
    }
}
