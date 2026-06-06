/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
///     <pre>
/// ［シミュレーション　＞　標準モード］の主フロー
/// 
/// TODO: 今は［標準版］とか［本戦版］とかに分かれているが、将来的には［ルールを一覧から選択する］ということを撤廃して、処理を主線１本に統合して、DSL ファイルを入力するようにしたいぜ（＾▽＾）！
///     </pre>
/// </summary>
internal class StandardSimulationMainline
    : AbstractSimulationMainline
{
    string? outputPathOverride;
    int? requestedSimulationCount;

    internal void Run(StandardModeSimulationContext context, string? outputPathOverride, int? requestedSimulationCount = null)
    {
        this.outputPathOverride = outputPathOverride;
        this.requestedSimulationCount = requestedSimulationCount;
        try
        {
            Run(context);
        }
        finally
        {
            this.outputPathOverride = null;
            this.requestedSimulationCount = null;
        }
    }

    protected override void BeforeExecuteSimulationContext(AbstractSimulationContext context)
    {
        PrintStandardModeContext((StandardModeSimulationContext)context);
    }

    protected override SimulationMainlineResult ExecuteSimulation(AbstractSimulationContext context)
    {
        var standardContext = (StandardModeSimulationContext)context;
        var tournamentFinalState = ExecuteTournamentFinalState(standardContext);
        var finalRankingRows = BuildStandardResultRows(standardContext, tournamentFinalState);
        return new SimulationMainlineResult(tournamentFinalState, finalRankingRows, SimulationMainlineResultPresentation.Championship);
    }

    protected override void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
    {
        var standardContext = (StandardModeSimulationContext)context;
        ConsoleResultPrinter.PrintResult(
            standardContext.Players.Count,
            mainlineResult.SimulationResult.TournamentFinalState,
            standardContext.FirstPlayerWinRatePercent,
            mainlineResult.FinalRankingResult.Rows);
    }

    protected override void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineResult mainlineResult)
    {
        var standardContext = (StandardModeSimulationContext)context;
        WriteFinalRankingOutputsForStandardMode(standardContext, mainlineResult.SimulationResult.TournamentFinalState, mainlineResult.FinalRankingResult, outputPathOverride);
    }

    CalculationResult ExecuteTournamentFinalState(StandardModeSimulationContext context)
    {
        return ExecuteStandardTournamentFinalState(
            context,
            exactCalculationMessage: "厳密計算を行います。\n",
            simulationPrompt: "局数が多いためシミュレーションで近似します。試行回数を入力してください [200000]: ",
            requestedSimulationCount: requestedSimulationCount);
    }

    static void PrintStandardModeContext(StandardModeSimulationContext context)
    {
        if (context.ExcludedPlayerCount > 0)
        {
            Console.WriteLine($"未対局の選手 {context.ExcludedPlayerCount} 人を結果から除外します。\n");
        }

        PrintCommonSimulationContext(context, "総対局数");
    }

    static void WriteFinalRankingOutputsForStandardMode(
        StandardModeSimulationContext context,
        CalculationResult tournamentFinalState,
        FinalRankingResult<GeneralSimulationResultRow> finalRankingResult,
        string? outputPathOverride)
    {
        var (outputCsvPath, outputMarkdownPath) = FinalRankingDomain.ResolveOutputPaths(
            $"standard_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            outputPathOverride);

        FinalRankingMarkdownFileWriter finalRankingDataFileWriter = new(new FinalRankingDataFileWriterSettings(RuleProfileMode.Standard));
        FinalRankingDomain.WriteOutputs(
            finalRankingDataFileWriter,
            outputCsvPath,
            outputMarkdownPath,
            tournamentFinalState,
            context.FirstPlayerWinRatePercent,
            finalRankingResult.Rows);

        FinalRankingDomain.PrintOutputCompleted(outputCsvPath, outputMarkdownPath);
    }
}
