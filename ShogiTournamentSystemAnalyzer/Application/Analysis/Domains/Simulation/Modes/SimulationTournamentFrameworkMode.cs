/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// シミュレーション域の大会進行フレームワーク実行モード。
/// </summary>
internal static partial class SimulationTournamentFrameworkMode
{
    internal static FinalRankingDomainInput Run()
    {
        Console.WriteLine("対局シミュレーション / 大会進行フレームワーク: 一般化した大会進行モデルで大会記録を実行します。\n");
        ConsoleSamplePrinter.PrintSimulationTournamentFrameworkOverview();
        var context = SimulationModeInputReaders.ReadTournamentFrameworkModeContext();
        return RunMainlineToTournamentFinalStateAndFinalRanking(context);
    }

    internal static FinalRankingDomainInput Run(TournamentFrameworkModeContext context)
    {
        Console.WriteLine("対局シミュレーション / 大会進行フレームワーク: 一般化した大会進行モデルで大会記録を実行します。\n");
        return RunMainlineToTournamentFinalStateAndFinalRanking(context);
    }

    static FinalRankingDomainInput RunMainlineToTournamentFinalStateAndFinalRanking(TournamentFrameworkModeContext context)
    {
        return ExecuteTournamentFrameworkMode(context);
    }
}
