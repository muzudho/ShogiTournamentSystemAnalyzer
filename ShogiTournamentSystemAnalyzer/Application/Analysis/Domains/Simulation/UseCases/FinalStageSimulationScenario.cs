/*
 * ［アプリケーション　＞　ユースケース　＞　シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

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

        var finalStageContext = context!;

        plan = new SimulationExecutionPlan(
            RuleProfileMode,
            "FinalStageMainline",
            () =>
            {
                var mainline = new FinalStageSimulationMainline();
                var mainlineResult = mainline.Run(finalStageContext);
                FinalRankingDomain.WriteFinalStageSimulationOutputs(
                    tournamentFinalState: mainlineResult.SimulationResult.TournamentFinalState,
                    firstPlayerWinRatePercent: finalStageContext.FirstPlayerWinRatePercent,
                    finalRankingResult: mainlineResult.FinalRankingResult,
                    outputPathOverride: null,
                    players: finalStageContext.Players,
                    referenceMatches: finalStageContext.ReferenceMatches,
                    writeReferenceMatchesForMarkdown: mainlineResult.Presentation == SimulationMainlineResultPresentation.GroupedOverall);
            });

        return true;
    }
}
