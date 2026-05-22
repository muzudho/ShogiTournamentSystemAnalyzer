/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static void RunFinalStageMode()
    {
        Console.WriteLine("対局シミュレーション / 本戦ルール: Apex / Innov 分割の定先戦を分析します。\n");

        ConsoleSamplePrinter.PrintSimulationFinalStageOverview();
        if (!TryReadFinalStageModeContext(out var context)) return;

        var result = ExecuteFinalStageMode(context, out var standardResultRows, out var finalStageResultRows);
        PrintFinalStageModeContext(context);
        WriteFinalStageModeOutputs(context, result, standardResultRows, finalStageResultRows);
    }
}

