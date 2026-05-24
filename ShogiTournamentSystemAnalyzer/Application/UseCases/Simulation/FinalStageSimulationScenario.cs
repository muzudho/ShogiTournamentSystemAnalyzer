namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class Program
{
    sealed class FinalStageSimulationScenario : ISimulationScenario
    {
        internal static readonly FinalStageSimulationScenario Instance = new();

        public RuleProfileMode RuleProfileMode => RuleProfileMode.FinalStage;

        public void Run()
        {
            Console.WriteLine("対局シミュレーション / 本戦ルール: Apex / Innov 分割の定先戦を分析します。\n");

            ConsoleSamplePrinter.PrintSimulationFinalStageOverview();
            if (!TryReadFinalStageModeContext(out var context)) return;

            var result = ExecuteTournamentFinalStateAndFinalRanking(context, out var standardResultRows, out var finalStageResultRows);
            PrintFinalStageModeContext(context);
            WriteFinalRankingOutputsForFinalStageMode(context, result, standardResultRows, finalStageResultRows);
        }
    }
}
