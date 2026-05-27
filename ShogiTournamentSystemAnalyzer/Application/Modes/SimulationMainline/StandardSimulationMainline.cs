/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Paths;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［シミュレーション　＞　標準モード］の主フロー
/// </summary>
internal class StandardSimulationMainline
    : AbstractSimulationMainline
{
    internal static void RunStatic(StandardModeSimulationContext context)
    {

        if (context.ExcludedPlayerCount > 0)
        {
            Console.WriteLine($"未対局の選手 {context.ExcludedPlayerCount} 人を結果から除外します。\n");
        }

        PrintMatchesAndCount(context, "総対局数");

        var tournamentFinalState = ExecuteTournamentFinalState();
        var finalRankingRows = BuildFinalRankingRows(tournamentFinalState);
        ConsoleResultPrinter.PrintResult(context.Players.Count, tournamentFinalState, context.FirstPlayerWinRatePercent, finalRankingRows);
        PrintTimeLimitIfNeeded(tournamentFinalState);

        WriteFinalRankingOutputsForStandardMode(context, tournamentFinalState, finalRankingRows);

        return;

        // 以下、ローカル関数

        CalculationResult ExecuteTournamentFinalState()
        {
            if (context.Matches.Count <= 20)
            {
                Console.WriteLine("厳密計算を行います。\n");
                return StandardCalculationEngine.CalculateExactly(context.Players, context.Matches, context.FirstPlayerWinRateRating, context.TournamentRuleSetMode);
            }

            const int defaultSimulationCount = 200_000;
            var simulationCount = ConsolePromptReaders.ReadIntWithDefault(
                $"局数が多いためシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                defaultSimulationCount,
                min: 1);

            Console.WriteLine();
            using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
            return StandardCalculationEngine.CalculateBySimulation(context.Players, context.Matches, context.FirstPlayerWinRateRating, simulationCount, context.TournamentRuleSetMode);
        }

        IReadOnlyList<ResultRow> BuildFinalRankingRows(CalculationResult tournamentFinalState)
        {
            return RankingResultRowBuilder.BuildResultRows(context.Players, context.Matches, tournamentFinalState, context.FirstPlayerWinRatePercent);
        }
    }

    static void WriteFinalRankingOutputsForStandardMode(StandardModeSimulationContext context, CalculationResult tournamentFinalState, IReadOnlyList<ResultRow> finalRankingRows)
    {
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath($"standard_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () => FinalRankingDataFileWriter.CreateResultCsv(tournamentFinalState.Mode, context.FirstPlayerWinRatePercent, finalRankingRows));

        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () => FinalRankingDataFileWriter.CreateResultMarkdown(outputMarkdownPath, outputCsvPath, tournamentFinalState.Mode, context.FirstPlayerWinRatePercent, finalRankingRows));

        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");
    }
}

