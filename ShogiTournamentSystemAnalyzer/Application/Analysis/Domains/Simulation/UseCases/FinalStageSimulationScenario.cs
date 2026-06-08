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

internal sealed class FinalStageSimulationScenario : ISimulationScenario
{
    internal static readonly FinalStageSimulationScenario Instance = new();

    public RuleProfileAttributes RuleProfileAttributes { get; } = new(
        RuleProfileSimulationShape.FinalStageGrouped,
        UsesFinalStageGrouping: true,
        UsesAdditionalApexPlacement: true,
        UsesBoundaryRescue: true,
        UsesVariableTop8: true,
        TournamentRuleSetMode.Neutral,
        HasReferenceMatches: true,
        RuleProfilePairingSource.ScheduledMatches);

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
            "FinalStageMainline",
            () =>
            {
                var mainline = new FinalStageSimulationMainline();
                var mainlineResult = mainline.Run(finalStageContext);
                return new SimulationDomainResult(
                    mainlineResult.SimulationResult,
                    mainlineResult.FinalRankingResult,
                    new FinalRankingDomainInput(
                        FinalRankingDomainInputKind.FinalStageSimulation,
                        mainlineResult.SimulationResult.TournamentFinalState,
                        finalStageContext.FirstPlayerWinRatePercent,
                        mainlineResult.FinalRankingResult,
                        null,
                        finalStageContext.Players,
                        finalStageContext.ReferenceMatches,
                        WriteReferenceMatchesForMarkdown: mainlineResult.Presentation == SimulationMainlineResultPresentation.GroupedOverall));
            });

        return true;
    }
}
