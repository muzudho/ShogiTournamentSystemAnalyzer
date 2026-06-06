/*
 * ［アプリケーション　＞　ユースケース　＞　最終順位付け域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Request.PlayerList;
using ShogiTournamentSystemAnalyzer.Domain.Request.RankingSettings;
using ShogiTournamentSystemAnalyzer.Domain.Request.TournamentRule;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class FinalRankingDomain
{
    internal static IReadOnlyList<GeneralSimulationResultRow> BuildStandardResultRows(
        IReadOnlyList<Player> players,
        IReadOnlyList<Match> matches,
        CalculationResult result,
        double firstPlayerWinRatePercent)
    {
        return RankingResultRowBuilder.BuildGeneralResultRows(players, matches, result, firstPlayerWinRatePercent);
    }

    internal static IReadOnlyList<GeneralSimulationResultRow> BuildFinalStageResultRows(
        IReadOnlyList<Player> players,
        IReadOnlyList<Match> matches,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyDictionary<string, FinalStageGroup> groupMap,
        int additionalApexCount)
    {
        return RankingResultRowBuilder.BuildFinalStageGeneralResultRows(players, matches, result, firstPlayerWinRatePercent, groupMap, additionalApexCount);
    }

    internal static IReadOnlyList<RepresentativeExecutionRankRow> BuildRepresentativeExecutionRankRows(
        IReadOnlyList<PlayerEntry> players,
        FinalRankingData finalRankingData)
    {
        var playerNameById = players.ToDictionary(player => player.PlayerId, player => player.Name);

        return finalRankingData.RankRows
            .GroupBy(row => row.Rank)
            .OrderBy(group => group.Key)
            .SelectMany(group =>
            {
                var rows = group.ToArray();
                var lastRank = group.Key + rows.Length - 1;
                var averagePlace = (group.Key + lastRank) / 2.0;
                var rankLabel = rows.Length == 1
                    ? group.Key.ToString()
                    : $"{group.Key}-{lastRank}";
                var firstPlaceProbability = group.Key == 1 ? 1.0 / rows.Length : 0.0;

                return rows
                    .Select(row => new RepresentativeExecutionRankRow(
                        playerNameById[row.PlayerId],
                        row.Points,
                        rankLabel,
                        averagePlace,
                        firstPlaceProbability));
            })
            .OrderBy(row => row.AveragePlace)
            .ThenByDescending(row => row.Points)
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static TournamentFrameworkFinalRankingResult BuildTournamentFrameworkFinalRankingResult(
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyList<StageEntry> stages,
        TournamentState representativeFinalState,
        IReadOnlyList<PlayerRankRow> representativeOverallRanking,
        int representativeTickCount,
        bool representativeCompletedNaturally,
        double[,] placeProbabilities,
        int requestedSimulationCount,
        int completedSimulationCount,
        bool isExactCalculation,
        TournamentRuleSetMode tournamentRuleSetMode,
        double firstPlayerWinRatePercent)
    {
        var playerListData = new PlayerListData(players);
        var tournamentFinalStateData = new TournamentFinalStateData(
            representativeFinalState.MatchRecords,
            representativeFinalState.CurrentTime,
            representativeTickCount,
            representativeCompletedNaturally);
        var finalRankingData = new FinalRankingData(
            representativeOverallRanking,
            IsIntermediate: false,
            Note: "大会進行フレームワークの最終順位データ");

        return BuildTournamentFrameworkFinalRankingResult(
            playerListData,
            stages,
            tournamentFinalStateData,
            finalRankingData,
            placeProbabilities,
            requestedSimulationCount,
            completedSimulationCount,
            isExactCalculation,
            tournamentRuleSetMode,
            firstPlayerWinRatePercent);
    }

    internal static TournamentFrameworkFinalRankingResult BuildTournamentFrameworkFinalRankingResult(
        PlayerListData playerListData,
        IReadOnlyList<StageEntry> stages,
        TournamentFinalStateData tournamentFinalStateData,
        FinalRankingData finalRankingData,
        double[,] placeProbabilities,
        int requestedSimulationCount,
        int completedSimulationCount,
        bool isExactCalculation,
        TournamentRuleSetMode tournamentRuleSetMode,
        double firstPlayerWinRatePercent)
    {
        var standardPlayers = playerListData.Players
            .OrderBy(player => player.PlayerId)
            .Select(player => new Player(player.Name, player.Rating))
            .ToArray();

        var playerIndexById = playerListData.Players
            .OrderBy(player => player.PlayerId)
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);
        var standardMatches = tournamentFinalStateData.MatchRecords
            .Select(match => new Match(playerIndexById[match.FirstPlayerId], playerIndexById[match.SecondPlayerId]))
            .ToArray();

        var representativeExecutionRankRows = BuildRepresentativeExecutionRankRows(playerListData.Players, finalRankingData);
        var aggregateCalculationResult = BuildTournamentFrameworkCalculationResult(
            placeProbabilities,
            requestedSimulationCount,
            completedSimulationCount,
            isExactCalculation,
            tournamentRuleSetMode);
        var aggregateFinalRankingRows = BuildStandardResultRows(standardPlayers, standardMatches, aggregateCalculationResult, firstPlayerWinRatePercent);

        return new TournamentFrameworkFinalRankingResult(
            standardPlayers,
            standardMatches,
            stages,
            playerListData.Players,
            tournamentFinalStateData,
            representativeExecutionRankRows,
            aggregateCalculationResult,
            new FinalRankingResult(aggregateFinalRankingRows));
    }

    static CalculationResult BuildTournamentFrameworkCalculationResult(
        double[,] placeProbabilities,
        int requestedSimulationCount,
        int completedSimulationCount,
        bool isExactCalculation,
        TournamentRuleSetMode tournamentRuleSetMode)
    {
        var ruleSetModeLabel = tournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => "Twill",
            TournamentRuleSetMode.TwillCommonOpponentWeighted => "Twill+CommonOpp",
            _ => "Neutral",
        };
        var modeCoreLabel = isExactCalculation
            ? $"厳密計算 / 大会進行フレームワーク / FixedMatch / {ruleSetModeLabel}"
            : $"大会進行フレームワーク / FixedMatch / {ruleSetModeLabel}";
        if (isExactCalculation)
        {
            var exactModeLabel = completedSimulationCount < requestedSimulationCount
                ? $"{modeCoreLabel} (途中打ち切り, 時間切れ)"
                : modeCoreLabel;
            return new CalculationResult(placeProbabilities, exactModeLabel, null);
        }

        var modeLabel = completedSimulationCount < requestedSimulationCount
            ? $"{modeCoreLabel} ({completedSimulationCount:N0}/{requestedSimulationCount:N0}回, 時間切れ)"
            : $"{modeCoreLabel} ({completedSimulationCount:N0}回)";

        return new CalculationResult(placeProbabilities, modeLabel, completedSimulationCount);
    }

    internal static void PrintTournamentFrameworkSimulationResults(
        TournamentFrameworkFinalRankingResult finalRankingResult,
        TournamentRuleData tournamentRuleData,
        RankingSettingsData rankingSettingsData,
        double defaultFirstPlayerWinRatePercent)
    {
        ConsoleResultPrinter.PrintMatchesCsv(finalRankingResult.StandardPlayers, finalRankingResult.StandardMatches, "大会進行フレームワークで読み込んだ対局CSV:");
        Console.WriteLine("注記: これ以降の順位表は複数回試行の aggregate 結果です。");
        Console.WriteLine("注記: あとで出力する大会最終状態CSV/Markdownは代表実行1件の対局記録です。\n");
        ConsoleResultPrinter.PrintRepresentativeExecutionRanking(finalRankingResult.RepresentativeExecutionRankRows, rankingSettingsData.TournamentRuleSetMode);
        ConsoleResultPrinter.PrintResult(
            finalRankingResult.StandardPlayers.Count,
            finalRankingResult.AggregateCalculationResult,
            tournamentRuleData.FirstPlayerWinRatePercent ?? defaultFirstPlayerWinRatePercent,
            finalRankingResult.AggregateFinalRankingResult.Rows);
        if (finalRankingResult.AggregateCalculationResult.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }
    }

    internal static (string OutputCsvPath, string OutputMarkdownPath) ResolveOutputPaths(string defaultFileName)
    {
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath(defaultFileName);
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(ConsolePromptReaders.ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        return (outputCsvPath, outputMarkdownPath);
    }

    internal static (string OutputCsvPath, string OutputMarkdownPath) ResolveOutputPaths(string defaultFileName, string? outputPathOverride)
    {
        return string.IsNullOrWhiteSpace(outputPathOverride)
            ? ResolveOutputPaths(defaultFileName)
            : ResolveOutputPathsFromOverride(outputPathOverride);
    }

    internal static (string OutputCsvPath, string OutputMarkdownPath) ResolveOutputPathsFromOverride(string outputPath)
    {
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(outputPath);
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        return (outputCsvPath, outputMarkdownPath);
    }

    internal static void PrintOutputCompleted(string outputCsvPath, string outputMarkdownPath)
    {
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");
    }

    internal static void WriteStandardSimulationOutputs(
        CalculationResult tournamentFinalState,
        double firstPlayerWinRatePercent,
        FinalRankingResult finalRankingResult,
        string? outputPathOverride)
    {
        var (outputCsvPath, outputMarkdownPath) = ResolveOutputPaths(
            $"standard_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            outputPathOverride);

        FinalRankingMarkdownFileWriter finalRankingDataFileWriter = new(new FinalRankingDataFileWriterSettings(RuleProfileMode.Standard));
        WriteOutputs(
            finalRankingDataFileWriter,
            outputCsvPath,
            outputMarkdownPath,
            tournamentFinalState,
            firstPlayerWinRatePercent,
            finalRankingResult.Rows);

        PrintOutputCompleted(outputCsvPath, outputMarkdownPath);
    }

    internal static void WriteFinalStageSimulationOutputs(
        CalculationResult tournamentFinalState,
        double firstPlayerWinRatePercent,
        FinalRankingResult finalRankingResult,
        string? outputPathOverride,
        IReadOnlyList<Player> players,
        IReadOnlyList<Match> referenceMatches,
        bool writeReferenceMatchesForMarkdown)
    {
        var (outputCsvPath, outputMarkdownPath) = ResolveOutputPaths(
            $"final_stage_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            outputPathOverride);
        var referenceMatchesCsvPath = referenceMatches.Count > 0
            ? ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"reference_matches_{DateTime.Now:yyyyMMdd_HHmmss}.csv")
            : null;
        var markdownReferenceMatchesCsvPath = writeReferenceMatchesForMarkdown
            ? referenceMatchesCsvPath
            : null;

        FinalRankingMarkdownFileWriter finalRankingDataFileWriter = new(new FinalRankingDataFileWriterSettings(RuleProfileMode.FinalStage));
        WriteOutputs(
            finalRankingDataFileWriter,
            outputCsvPath,
            outputMarkdownPath,
            tournamentFinalState,
            firstPlayerWinRatePercent,
            finalRankingResult.Rows,
            referenceMatchesCsvPath: markdownReferenceMatchesCsvPath);

        PrintOutputCompleted(outputCsvPath, outputMarkdownPath);

        if (referenceMatches.Count == 0) return;

        CsvOutputHelpers.WriteReferenceMatchCsv(referenceMatchesCsvPath!, players, referenceMatches);
        Console.WriteLine($"参考対局CSVを出力しました: {referenceMatchesCsvPath}");
    }

    internal static void WriteTournamentFrameworkSimulationOutputs(
        string? outputPathOverride,
        double defaultFirstPlayerWinRatePercent,
        TournamentRuleData tournamentRuleData,
        RankingSettingsData rankingSettingsData,
        TournamentFrameworkFinalRankingResult finalRankingResult)
    {
        const string AggregateOverviewNoteForCsv = "この順位表は複数回試行の aggregate 結果です。大会最終状態CSVとは 1 対 1 には対応しません。";
        const string AggregateOverviewNoteForMarkdown = "この順位表は複数回試行の aggregate 結果です。下記の大会最終状態テーブルとは 1 対 1 には対応しません。";
        const string RepresentativeOverviewNote = "この順位表は代表実行 1 件の順位です。aggregate 結果の順位表そのものではありません。";
        const string TournamentFinalStateOverviewNote = "この大会最終状態テーブルは代表実行 1 件の対局記録です。順位表の aggregate 結果そのものではありません。";

        var settings = new FinalRankingDataFileWriterSettings(RuleProfileMode.TournamentFramework);
        FinalRankingMarkdownFileWriter finalRankingDataFileWriter = new(settings);
        var firstPlayerWinRatePercent = tournamentRuleData.FirstPlayerWinRatePercent ?? defaultFirstPlayerWinRatePercent;

        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath($"tournament_framework_aggregate_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var requestedOutputPath = string.IsNullOrWhiteSpace(outputPathOverride)
            ? ConsolePromptReaders.ReadTextWithDefault($"\naggregate結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ", defaultOutputCsvPath)
            : outputPathOverride!;
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(requestedOutputPath);
        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        var representativeRankingCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath($"tournament_framework_representative_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var representativeRankingMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(representativeRankingCsvPath, ".md");
        var tournamentMatchRecordsCsvPath = ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"representative_tournament_final_state_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var tournamentMatchRecordsMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(tournamentMatchRecordsCsvPath, ".md");

        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () => new FinalRankingCsvFileWriter(settings).CreateResultCsvLines(
                mode: finalRankingResult.AggregateCalculationResult.Mode,
                firstPlayerWinRatePercent: firstPlayerWinRatePercent,
                resultRows: finalRankingResult.AggregateFinalRankingResult.Rows,
                overviewNote: AggregateOverviewNoteForCsv));

        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () => finalRankingDataFileWriter.CreateResultMarkdownCore(
                outputMarkdownPath: outputMarkdownPath,
                outputCsvPath: outputCsvPath,
                mode: finalRankingResult.AggregateCalculationResult.Mode,
                firstPlayerWinRatePercent: firstPlayerWinRatePercent,
                resultRows: finalRankingResult.AggregateFinalRankingResult.Rows,
                overviewNote: AggregateOverviewNoteForMarkdown,
                representativeRankingMarkdownPath: representativeRankingMarkdownPath));

        WriterHelper.WriteText(
            outputPath: representativeRankingCsvPath,
            getLines: () => RepresentativeExecutionRankFileWriter.CreateCsv(
                rankingSettingsData.TournamentRuleSetMode,
                finalRankingResult.RepresentativeExecutionRankRows,
                overviewNote: RepresentativeOverviewNote));

        WriterHelper.WriteText(
            outputPath: representativeRankingMarkdownPath,
            getLines: () => RepresentativeExecutionRankFileWriter.CreateMarkdown(
                representativeRankingMarkdownPath,
                representativeRankingCsvPath,
                rankingSettingsData.TournamentRuleSetMode,
                finalRankingResult.RepresentativeExecutionRankRows,
                overviewNote: RepresentativeOverviewNote,
                representativeMatchRecordsMarkdownPath: tournamentMatchRecordsMarkdownPath));

        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsCsvPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordCsv(
                finalRankingResult.RepresentativeStages,
                finalRankingResult.RepresentativePlayers,
                finalRankingResult.RepresentativeTournamentFinalState.MatchRecords,
                overviewNote: TournamentFinalStateOverviewNote));

        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsMarkdownPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordMarkdown(
                tournamentMatchRecordsMarkdownPath,
                tournamentMatchRecordsCsvPath,
                finalRankingResult.RepresentativeStages,
                finalRankingResult.RepresentativePlayers,
                finalRankingResult.RepresentativeTournamentFinalState.MatchRecords,
                overviewNote: TournamentFinalStateOverviewNote,
                aggregateResultMarkdownPath: outputMarkdownPath,
                representativeRankingMarkdownPath: representativeRankingMarkdownPath));

        Console.WriteLine($"aggregate結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"aggregate結果Markdownを出力しました: {outputMarkdownPath}");
        Console.WriteLine($"representative順位表CSVを出力しました: {representativeRankingCsvPath}");
        Console.WriteLine($"representative順位表Markdownを出力しました: {representativeRankingMarkdownPath}");
        Console.WriteLine($"representative大会最終状態CSVを出力しました: {tournamentMatchRecordsCsvPath}");
        Console.WriteLine($"representative大会最終状態Markdownを出力しました: {tournamentMatchRecordsMarkdownPath}");
    }

    internal static void WriteOutputFiles(
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

    internal static void WriteOutputs(
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        Func<string, string, double, IReadOnlyList<GeneralSimulationResultRow>, IEnumerable<string>> createCsvLines,
        Func<string, string, string, double, IReadOnlyList<GeneralSimulationResultRow>, IEnumerable<string>> createMarkdownLines)
    {
        WriteOutputFiles(
            outputCsvPath,
            outputMarkdownPath,
            createCsvLines: () => createCsvLines(outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows),
            createMarkdownLines: () => createMarkdownLines(outputMarkdownPath, outputCsvPath, result.Mode, firstPlayerWinRatePercent, resultRows));
    }

    internal static void WriteOutputs(
        FinalRankingMarkdownFileWriter finalRankingDataFileWriter,
        string outputCsvPath,
        string outputMarkdownPath,
        CalculationResult result,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? referenceMatchesCsvPath = null)
    {
        WriteOutputs(
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
