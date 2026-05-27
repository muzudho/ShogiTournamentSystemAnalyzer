/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;

using ShogiTournamentSystemAnalyzer.Application.Modes;
using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal sealed class StandardSimulationScenario : ISimulationScenario
{
    internal static readonly StandardSimulationScenario Instance = new();

    public RuleProfileMode RuleProfileMode => RuleProfileMode.Standard;

    public void PrintOverview()
    {
        Console.WriteLine("対局シミュレーション / 通常ルール: 総当たり戦の順位分布を計算します。\n");
        Console.WriteLine("前提: 各対局は先手・後手を持ち、勝率は Elo レーティング差と先手有利率から計算します。\n");

        ConsoleSamplePrinter.PrintSimulationStandardOverview();
    }

    public bool TryPrepareExecution(out SimulationExecutionPlan plan)
    {
        var context = SimulationModeInputReaders.ReadStandardModeContext();
        plan = new SimulationExecutionPlan(
            RuleProfileMode,
            "StandardMainline",
            () => StandardSimulationMainline.Run(context));
        return true;
    }
}
