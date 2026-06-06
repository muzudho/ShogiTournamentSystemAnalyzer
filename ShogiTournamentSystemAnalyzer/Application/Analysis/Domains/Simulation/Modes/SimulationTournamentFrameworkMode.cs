/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static partial class SimulationTournamentFrameworkMode
{
    internal static void Run()
    {
        Console.WriteLine("対局シミュレーション / 大会進行フレームワーク: 一般化した大会進行モデルで大会記録を実行します。\n");
        ConsoleSamplePrinter.PrintSimulationTournamentFrameworkOverview();
        var context = SimulationModeInputReaders.ReadTournamentFrameworkModeContext();
        RunMainlineToTournamentFinalStateAndFinalRanking(context);
    }

    internal static void Run(TournamentFrameworkModeContext context)
    {
        Console.WriteLine("対局シミュレーション / 大会進行フレームワーク: 一般化した大会進行モデルで大会記録を実行します。\n");
        RunMainlineToTournamentFinalStateAndFinalRanking(context);
    }

    static void RunMainlineToTournamentFinalStateAndFinalRanking(TournamentFrameworkModeContext context)
    {
        ExecuteTournamentFrameworkMode(context);
    }
}
