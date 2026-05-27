/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.UseCases.Simulation;

using ShogiTournamentSystemAnalyzer.Application.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.Console;

internal sealed class FinalStageSimulationScenario : ISimulationScenario
{
    internal static readonly FinalStageSimulationScenario Instance = new();

    public RuleProfileMode RuleProfileMode => RuleProfileMode.FinalStage;

    public void PrintOverview()
    {
        Console.WriteLine("対局シミュレーション / 本戦ルール: Apex / Innov 分割の定先戦を分析します。\n");

        ConsoleSamplePrinter.PrintSimulationFinalStageOverview();
    }

    public bool TryPrepareExecution(out SimulationExecutionPlan plan)
    {
        if (!SimulationModeInputReaders.TryReadFinalStageModeContext(out var context))
        {
            plan = default;
            return false;
        }

        plan = new SimulationExecutionPlan(
            RuleProfileMode,
            "FinalStageMainline",
            () => FinalStageSimulationMainline.Run(context));

        return true;
    }
}
