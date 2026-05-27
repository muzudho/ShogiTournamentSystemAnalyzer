/*
 * ［アプリケーション　＞　モード　＞　シミュレーション主線］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes.SimulationMainline;

using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Paths;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal abstract class AbstractSimulationMainline
{
    public void RunDynamic(AbstractSimulationContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");

        RunDynamicCore(context);
    }

    public void RunStatic(AbstractSimulationContext context)
    {
        BeforeExecuteSimulationContext(context);

        var executionResult = ExecuteSimulation(context);

        AfterExecuteSimulationContext(context, executionResult);
        PrintSimulationResult(context, executionResult);
        PrintTimeLimitIfNeeded(executionResult.Result);
        WriteSimulationOutputs(context, executionResult);
    }

    protected virtual void RunDynamicCore(AbstractSimulationContext context)
    {
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

    protected static IReadOnlyList<ResultRow> BuildStandardResultRows(AbstractSimulationContext context, CalculationResult result)
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

    protected static void WriteStandardFinalRankingOutputs(
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<ResultRow> resultRows)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            // XXX: なんで［標準版］と［本戦版］で出力内容が違うんだ（＾～＾）？
            getLines: () => FinalRankingDataFileWriter.CreateResultCsv(
                result.Mode, firstPlayerWinRatePercent, resultRows  // ここは共通だ（＾▽＾）
                ));

        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            // XXX: なんで［標準版］と［本戦版］で出力内容が違うんだ（＾～＾）？
            getLines: () => FinalRankingDataFileWriter.CreateResultMarkdown(
                outputMarkdownPath, outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows   // ここは共通だ（＾▽＾）
                ));
    }

    protected static void WriteFinalStageFinalRankingOutputs(
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<FinalStageResultRow> resultRows,
        string? referenceMatchesCsvPath)
    {
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            // XXX: なんで［標準版］と［本戦版］で出力内容が違うんだ（＾～＾）？
            getLines: () => FinalRankingDataFileWriter.CreateFinalStageResultCsv(
                outputCsvPath,  // XXX: ［本戦版］はCSVに出力パスを入れる
                result.Mode, firstPlayerWinRatePercent, resultRows  // ここは共通だ（＾▽＾）
                ));

        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            // XXX: なんで［標準版］と［本戦版］で出力内容が違うんだ（＾～＾）？
            getLines: () => FinalRankingDataFileWriter.CreateFinalStageResultMarkdown(
                outputMarkdownPath, outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows,  // ここは共通だ（＾▽＾）
                referenceMatchesCsvPath // XXX: これが［本戦版］にだけある（＾～＾）？
                ));
    }
}

internal abstract record SimulationMainlineExecutionResult(CalculationResult Result);

internal sealed record StandardSimulationExecutionResult(
    CalculationResult Result,
    IReadOnlyList<ResultRow> ResultRows)
    : SimulationMainlineExecutionResult(Result);

internal sealed record FinalStageSimulationExecutionResult(
    CalculationResult Result,
    IReadOnlyList<FinalStageResultRow> ResultRows)
    : SimulationMainlineExecutionResult(Result);
