/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static void RunStandardMode()
    {
        Console.WriteLine("対局シミュレーション / 通常ルール: 総当たり戦の順位分布を計算します。\n");
        Console.WriteLine("前提: 各対局は黒番・白番を持ち、勝率は Elo レーティング差と黒番有利率から計算します。\n");

        PrintSimulationStandardOverview();
        var context = ReadStandardModeContext();
        ExecuteStandardMode(context);
    }
}

