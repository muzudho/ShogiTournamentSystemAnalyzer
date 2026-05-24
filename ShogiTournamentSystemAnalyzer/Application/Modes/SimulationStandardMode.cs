/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static void RunStandardMode()
    {
        Console.WriteLine("対局シミュレーション / 通常ルール: 総当たり戦の順位分布を計算します。\n");
        Console.WriteLine("前提: 各対局は先手・後手を持ち、勝率は Elo レーティング差と先手有利率から計算します。\n");

        ConsoleSamplePrinter.PrintSimulationStandardOverview();
        var context = SimulationModeInputReaders.ReadStandardModeContext();
        RunMainlineToFinalRanking(context);
    }

    static void RunMainlineToFinalRanking(StandardModeContext context)
    {
        ExecuteStandardMode(context);
    }
}

