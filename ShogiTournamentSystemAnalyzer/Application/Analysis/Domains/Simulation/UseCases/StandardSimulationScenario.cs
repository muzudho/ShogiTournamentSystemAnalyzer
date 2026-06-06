/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal sealed class StandardSimulationScenario : ISimulationScenario
{
    internal static readonly StandardSimulationScenario Instance = new();

    public RuleProfileAttributes RuleProfileAttributes { get; } = new(
        RuleProfileSimulationShape.ScheduledMatches,
        UsesFinalStageGrouping: false,
        UsesAdditionalApexPlacement: false,
        UsesBoundaryRescue: false,
        UsesVariableTop8: false,
        TournamentRuleSetMode.Neutral,
        HasReferenceMatches: false,
        RuleProfilePairingSource.ScheduledMatches);

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
            "StandardMainline",
            () =>
            {
                var mainline = new StandardSimulationMainline();
                var mainlineResult = mainline.Run(context);
                FinalRankingDomain.WriteStandardSimulationOutputs(
                    mainlineResult.SimulationResult.TournamentFinalState,
                    context.FirstPlayerWinRatePercent,
                    mainlineResult.FinalRankingResult,
                    outputPathOverride: null);
            });
        return true;
    }
}
