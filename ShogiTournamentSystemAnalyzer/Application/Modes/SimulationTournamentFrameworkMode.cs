/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static void RunTournamentFrameworkMode()
    {
        Console.WriteLine("対局シミュレーション / 大会進行フレームワーク: 一般化した大会進行モデルで大会記録を実行します。\n");
        ConsoleSamplePrinter.PrintSimulationTournamentFrameworkOverview();
        var context = ReadTournamentFrameworkModeContext();
        ExecuteTournamentFrameworkMode(context);
    }
}
