/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

/// <summary>
/// ［シミュレーション域］の主線
/// </summary>
internal abstract class AbstractSimulationMainline
{
    /// <summary>
    /// 実行
    /// </summary>
    /// <param name="context"></param>
    public void Run(AbstractSimulationContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");
        BeforeExecuteSimulationContext(context);

        var executionResult = ExecuteSimulation(context);

        AfterExecuteSimulationContext(context, executionResult);
        PrintSimulationResult(context, executionResult);
        PrintTimeLimitIfNeeded(executionResult.Result);
        WriteSimulationOutputs(context, executionResult);
    }

    protected virtual void BeforeExecuteSimulationContext(AbstractSimulationContext context)
    {
    }

    protected virtual void AfterExecuteSimulationContext(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult)
    {
    }

    protected abstract SimulationMainlineExecutionResult ExecuteSimulation(AbstractSimulationContext context);

    protected abstract void PrintSimulationResult(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult);

    protected abstract void WriteSimulationOutputs(AbstractSimulationContext context, SimulationMainlineExecutionResult executionResult);

    protected static void PrintMatchesAndCount(AbstractSimulationContext context, string matchCountLabel)
    {
        ConsoleResultPrinter.PrintMatchesCsv(context.Players, context.Matches);
        Console.WriteLine($"\n{matchCountLabel}: {context.Matches.Count}");
    }

    protected static void PrintCommonSimulationContext(AbstractSimulationContext context, string matchCountLabel)
    {
        PrintMatchesAndCount(context, matchCountLabel);
    }

    protected static void PrintReferenceMatchesIfAny(IReadOnlyList<Player> players, IReadOnlyList<Match> referenceMatches)
    {
        if (referenceMatches.Count == 0) return;

        ConsoleResultPrinter.PrintMatchesCsv(players, referenceMatches, "参考対局CSV:");
        Console.WriteLine($"参考対局数: {referenceMatches.Count}");
        Console.WriteLine("参考対局は順位計算に含めません。\n");
    }

    protected static void PrintTimeLimitIfNeeded(CalculationResult result)
    {
        if (!result.Mode.Contains("時間切れ", StringComparison.Ordinal)) return;

        Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
    }

    protected static CalculationResult ExecuteStandardTournamentFinalState(
        AbstractSimulationContext context,
        string exactCalculationMessage,
        string simulationPrompt)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine(exactCalculationMessage);
            return StandardCalculationEngine.CalculateExactly(context.Players, context.Matches, context.FirstPlayerWinRateRating, context.TournamentRuleSetMode);
        }

        const int defaultSimulationCount = 200_000;
        var simulationCount = ConsolePromptReaders.ReadIntWithDefault(
            simulationPrompt,
            defaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        return StandardCalculationEngine.CalculateBySimulation(context.Players, context.Matches, context.FirstPlayerWinRateRating, simulationCount, context.TournamentRuleSetMode);
    }

    protected static IReadOnlyList<StandardResultRow> BuildStandardResultRows(AbstractSimulationContext context, CalculationResult result)
    {
        return RankingResultRowBuilder.BuildResultRows(context.Players, context.Matches, result, context.FirstPlayerWinRatePercent);
    }

    protected static (string OutputCsvPath, string OutputMarkdownPath) ResolveFinalRankingOutputPaths(string defaultFileName)
    {
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath(defaultFileName);
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        return (outputCsvPath, outputMarkdownPath);
    }

    protected static void PrintFinalRankingOutputCompleted(string outputCsvPath, string outputMarkdownPath)
    {
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");
    }

    protected static void WriteOutputFiles(
        string outputCsvPath,
        string outputMarkdownPath,
        Func<IEnumerable<string>> createCsvLines,
        Func<IEnumerable<string>> createMarkdownLines)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: createCsvLines);

        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: createMarkdownLines);
    }

    protected static void WriteFinalRankingOutputs<TRow>(
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        Func<string, string, double, IReadOnlyList<TRow>, IEnumerable<string>> createCsvLines,
        Func<string, string, string, double, IReadOnlyList<TRow>, IEnumerable<string>> createMarkdownLines)
    {
        WriteOutputFiles(
            outputCsvPath,
            outputMarkdownPath,
            createCsvLines: () => createCsvLines(outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows),
            createMarkdownLines: () => createMarkdownLines(outputMarkdownPath, outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows));
    }

    protected static void WriteFinalRankingOutputs<TRow>(
        FinalRankingMarkdownFileWriter finalRankingDataFileWriter,
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        string? referenceMatchesCsvPath = null)
        where TRow : ISimulationResultRow, IGeneralSimulationResultRowSource
    {
        WriteFinalRankingOutputs(
            outputCsvPath,
            outputMarkdownPath,
            result,
            firstPlayerWinRatePercent,
            resultRows,
            createCsvLines: (outputCsvPath, mode, firstPlayerWinRatePercent, resultRows) => new FinalRankingCsvFileWriter(finalRankingDataFileWriter.Settings).CreateResultCsvLines(
                mode,
                firstPlayerWinRatePercent,
                resultRows),
            createMarkdownLines: (outputMarkdownPath, outputCsvPath, mode, firstPlayerWinRatePercent, resultRows) => finalRankingDataFileWriter.CreateResultMarkdownCore(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                resultRows,
                referenceMatchesCsvPath: referenceMatchesCsvPath));
    }
}

internal abstract record SimulationMainlineExecutionResult(CalculationResult Result);

internal sealed record SimulationMainlineExecutionResult<TRow>(
    CalculationResult Result,
    IReadOnlyList<TRow> ResultRows)
    : SimulationMainlineExecutionResult(Result)
    where TRow : ISimulationResultRow;
