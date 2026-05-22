/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;

internal static partial class Program
{
    static void ExecuteStandardMode(StandardModeContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");

        if (context.ExcludedParticipantCount > 0)
        {
            Console.WriteLine($"未対局の選手 {context.ExcludedParticipantCount} 人を結果から除外します。\n");
        }

        PrintMatchesCsv(context.Participants, context.Matches);
        Console.WriteLine($"\n総対局数: {context.Matches.Count}");

        var result = ExecuteStandardModeCalculation(context);
        var resultRows = RankingResultRowBuilder.BuildResultRows(context.Participants, context.Matches, result, context.FirstPlayerWinRatePercent);
        PrintResult(context.Participants.Count, result, context.FirstPlayerWinRatePercent, resultRows);
        if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {Program.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var defaultOutputCsvPath = Path.GetFullPath($"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () => ResultCsvWriter.CreateResultCsv(result.Mode, context.FirstPlayerWinRatePercent, resultRows));

        var outputMarkdownPath = ChangeOutputExtension(outputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () => ResultCsvWriter.CreateResultMarkdown(outputMarkdownPath, outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, resultRows));

        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");
    }

    static CalculationResult ExecuteStandardModeCalculation(StandardModeContext context)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine("厳密計算を行います。\n");
            return CalculateExactly(context.Participants, context.Matches, context.FirstPlayerWinRateRating, context.TournamentRuleSetMode);
        }

        const int defaultSimulationCount = 200_000;
        var simulationCount = ConsolePromptReaders.ReadIntWithDefault(
            $"局数が多いためシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
            defaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return CalculateBySimulation(context.Participants, context.Matches, context.FirstPlayerWinRateRating, simulationCount, context.TournamentRuleSetMode);
    }
}

