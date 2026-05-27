/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Helpers;
using ShogiTournamentSystemAnalyzer.Domain.FinalRanking;
using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.RankingSettings;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;

internal static partial class SimulationTournamentFrameworkMode
{
    /// <summary>
    /// ［大会フレームワーク　＞　シミュレーション回数　＞　既定値］
    /// </summary>
    const int DefaultTournamentFrameworkSimulationCount = 200_000;

    /// <summary>
    /// ［大会フレームワーク　＞　明瞭計算対局　＞　閾値］
    /// </summary>
    const int TournamentFrameworkExactCalculationMatchThreshold = 20;

    /// <summary>
    /// ［大会フレームワーク・モード］実行
    /// </summary>
    /// <param name="context"></param>
    static void ExecuteTournamentFrameworkMode(TournamentFrameworkModeContext context)
    {
        // ［選手一覧データ］読込
        var players = TournamentFrameworkCsvParsers.ReadPlayerEntriesFromCsvPath(context.PlayersCsvPath);

        // ［段階マスターデータ］読込
        var stages = TournamentFrameworkCsvParsers.ReadStageEntriesFromCsvPath(context.StagesCsvPath);
        var matchRecords = TournamentFrameworkCsvParsers.ReadTournamentMatchRecordsFromCsvPath(context.TournamentMatchRecordsCsvPath);

        // ［大会ルールＤＳＬ定義］
        TournamentDslDefinition? dslDefinition = null;
        if (!string.IsNullOrWhiteSpace(context.RuleFilePath))
        {
            dslDefinition = TournamentDslDefinitionParser.ParseTournamentDsl(File.ReadAllText(Path.GetFullPath(context.RuleFilePath)), context.RuleFilePath!);
            Console.WriteLine($"大会ルールDSLを読み込みました: {context.RuleFilePath}");
        }

        // ［大会ルールデータ］
        var tournamentRuleData = BoundaryDataBuilders.BuildTournamentRuleBoundaryData(context, dslDefinition);

        // ［選手一覧データ］
        var playerListData = BoundaryDataBuilders.BuildPlayerListBoundaryData(players);

        // ［順位設定データ］
        var rankingSettingsData = BoundaryDataBuilders.BuildRankingSettingsBoundaryData(tournamentRuleData);

        // ［初回状態］
        var initialState = new TournamentState(0, players, stages, matchRecords);

        // ［順位付けの設定］選択
        IRankingRule rankingRule = tournamentRuleData.TournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => TwillTournamentRankingRule.Instance,
            TournamentRuleSetMode.TwillCommonOpponentWeighted => TwillTournamentRankingRule.CommonOpponentWeightedInstance,
            _ => ByFinishedResultsRankingRule.Instance,
        };

        // ［大会ルールセット］
        var ruleSet = new TournamentFrameworkRuleSet(
            FixedMatchPairingRule.Instance,
            rankingRule,
            AllMatchesFinishedTerminationRule.Instance,
            new StandardLikeMatchResultResolver(context.FirstPlayerWinRateRating));

        // ［大会エンジン］
        var engine = new TournamentEngine(ruleSet, tournamentRuleData.RandomSeed);

        // ［集計結果］
        var aggregateResult = ExecuteTournamentFrameworkModeCalculation(engine, initialState, players, tournamentRuleData.TournamentRuleSetMode ?? TournamentRuleSetMode.Neutral, context.FirstPlayerWinRateRating, context.SimulationCount);

        // ［実行結果］
        var executionResult = aggregateResult.RepresentativeExecutionResult;

        // ［大会最終状態データ］
        var tournamentFinalStateData = BoundaryDataBuilders.BuildTournamentFinalStateBoundaryData(executionResult);

        // ［最終順位データ］
        var finalRankingData = BoundaryDataBuilders.BuildFinalRankingBoundaryData(executionResult);

        // ［大会進行フレームワークで使用する標準的な選手・対局表］
        var standardPlayers = playerListData.Players
            .OrderBy(player => player.PlayerId)
            .Select(player => new Player(player.Name, player.Rating))
            .ToArray();

        // 大会進行フレームワークの順位ルールで対局結果を反映させるための標準的な対局表を作成する。これをもとに順位ルールのロジックを適用して、代表実行の順位表と同じ形式の順位表を作成する。
        var playerIndexById = playerListData.Players
            .OrderBy(player => player.PlayerId)
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);
        var standardMatches = tournamentFinalStateData.MatchRecords
            .Select(match => new Match(playerIndexById[match.FirstPlayerId], playerIndexById[match.SecondPlayerId]))
            .ToArray();
        var representativeExecutionRankRows = BuildRepresentativeExecutionRankRows(playerListData.Players, finalRankingData.RankRows);

        var result = BuildTournamentFrameworkCalculationResult(aggregateResult);
        var resultRows = RankingResultRowBuilder.BuildResultRows(standardPlayers, standardMatches, result, tournamentRuleData.FirstPlayerWinRatePercent ?? context.FirstPlayerWinRatePercent);

        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(rankingSettingsData.TournamentRuleSetMode)}");

        if (aggregateResult.IsExactCalculation)
        {
            Console.WriteLine("計算種別: 厳密計算");
            Console.WriteLine($"進行Tick数: {aggregateResult.AverageTickCount:F2}");
            Console.WriteLine($"自然終了: {(aggregateResult.CompletedNaturallyCount > 0 ? "Yes" : "No")}");
        }
        else
        {
            Console.WriteLine($"集計試行回数: {aggregateResult.CompletedSimulationCount:N0}");
            Console.WriteLine($"平均進行Tick数: {aggregateResult.AverageTickCount:F2}");
            Console.WriteLine($"自然終了率: {aggregateResult.CompletedNaturallyCount:N0}/{aggregateResult.CompletedSimulationCount:N0}");
        }

        Console.WriteLine($"代表実行Tick数: {tournamentFinalStateData.TickCount}");
        Console.WriteLine($"代表実行の自然終了: {(tournamentFinalStateData.CompletedNaturally ? "Yes" : "No")}");
        Console.WriteLine($"ステージ数: {stages.Count}");
        Console.WriteLine($"総対局数: {matchRecords.Count}\n");
        if (dslDefinition is not null)
        {
            Console.WriteLine($"DSL TimeAxis: {dslDefinition.TimeAxis}");
            Console.WriteLine($"DSL OverallRanking: {dslDefinition.OverallRankingRuleName}\n");
        }

        ConsoleResultPrinter.PrintMatchesCsv(standardPlayers, standardMatches, "大会進行フレームワークで読み込んだ対局CSV:");
        Console.WriteLine("注記: これ以降の順位表は複数回試行の aggregate 結果です。");
        Console.WriteLine("注記: あとで出力する大会最終状態CSV/Markdownは代表実行1件の対局記録です。\n");
        ConsoleResultPrinter.PrintRepresentativeExecutionRanking(representativeExecutionRankRows, rankingSettingsData.TournamentRuleSetMode);
        ConsoleResultPrinter.PrintResult(standardPlayers.Length, result, tournamentRuleData.FirstPlayerWinRatePercent ?? context.FirstPlayerWinRatePercent, resultRows);
        if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        WriteFinalRankingAndTournamentFinalStateOutputsForTournamentFramework(
            context,
            tournamentRuleData,
            rankingSettingsData,
            tournamentFinalStateData,
            stages,
            players,
            representativeExecutionRankRows,
            result,
            resultRows);
    }

    static void WriteFinalRankingAndTournamentFinalStateOutputsForTournamentFramework(
        TournamentFrameworkModeContext context,
        TournamentRuleData tournamentRuleData,
        RankingSettingsData rankingSettingsData,
        TournamentFinalStateData tournamentFinalStateData,
        IReadOnlyList<StageEntry> stages,
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyList<RepresentativeExecutionRankRow> representativeExecutionRankRows,
        CalculationResult finalRankingCalculation,
        IReadOnlyList<ResultRow> finalRankingRows)
    {
        var defaultOutputCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath($"tournament_framework_aggregate_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var requestedOutputPath = string.IsNullOrWhiteSpace(context.OutputPath)
            ? ConsolePromptReaders.ReadTextWithDefault($"\naggregate結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ", defaultOutputCsvPath)
            : context.OutputPath!;
        var outputCsvPath = CsvOutputHelpers.ResolveOutputCsvPath(requestedOutputPath);
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () => FinalRankingDataFileWriter.CreateResultCsv(
                finalRankingCalculation.Mode,
                tournamentRuleData.FirstPlayerWinRatePercent ?? context.FirstPlayerWinRatePercent,
                finalRankingRows,
                overviewNote: "この順位表は複数回試行の aggregate 結果です。大会最終状態CSVとは 1 対 1 には対応しません。"));

        var outputMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(outputCsvPath, ".md");
        var representativeRankingCsvPath = ReportOutputPathBuilder.BuildFinalRankingDefaultOutputPath($"tournament_framework_representative_final_ranking_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var representativeRankingMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(representativeRankingCsvPath, ".md");
        var tournamentMatchRecordsCsvPath = ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"representative_tournament_final_state_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var tournamentMatchRecordsMarkdownPath = CsvOutputHelpers.ChangeOutputExtension(tournamentMatchRecordsCsvPath, ".md");

        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () => FinalRankingDataFileWriter.CreateResultMarkdown(
                outputMarkdownPath,
                outputCsvPath,
                finalRankingCalculation.Mode,
                tournamentRuleData.FirstPlayerWinRatePercent ?? context.FirstPlayerWinRatePercent,
                finalRankingRows,
                overviewNote: "この順位表は複数回試行の aggregate 結果です。下記の大会最終状態テーブルとは 1 対 1 には対応しません。",
                representativeRankingMarkdownPath: representativeRankingMarkdownPath));

        WriterHelper.WriteText(
            outputPath: representativeRankingCsvPath,
            getLines: () => FinalRankingDataFileWriter.CreateRepresentativeExecutionRankCsv(
                rankingSettingsData.TournamentRuleSetMode,
                representativeExecutionRankRows,
                overviewNote: "この順位表は代表実行 1 件の順位です。aggregate 結果の順位表そのものではありません。"));

        WriterHelper.WriteText(
            outputPath: representativeRankingMarkdownPath,
            getLines: () => FinalRankingDataFileWriter.CreateRepresentativeExecutionRankMarkdown(
                representativeRankingMarkdownPath,
                representativeRankingCsvPath,
                rankingSettingsData.TournamentRuleSetMode,
                representativeExecutionRankRows,
                overviewNote: "この順位表は代表実行 1 件の順位です。aggregate 結果の順位表そのものではありません。",
                representativeMatchRecordsMarkdownPath: tournamentMatchRecordsMarkdownPath));

        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsCsvPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordCsv(
                stages,
                players,
                tournamentFinalStateData.MatchRecords,
                overviewNote: "この大会最終状態テーブルは代表実行 1 件の対局記録です。順位表の aggregate 結果そのものではありません。"));

        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsMarkdownPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordMarkdown(
                tournamentMatchRecordsMarkdownPath,
                tournamentMatchRecordsCsvPath,
                stages,
                players,
                tournamentFinalStateData.MatchRecords,
                overviewNote: "この大会最終状態テーブルは代表実行 1 件の対局記録です。順位表の aggregate 結果そのものではありません。",
                aggregateResultMarkdownPath: outputMarkdownPath,
                representativeRankingMarkdownPath: representativeRankingMarkdownPath));

        Console.WriteLine($"aggregate結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"aggregate結果Markdownを出力しました: {outputMarkdownPath}");
        Console.WriteLine($"representative順位表CSVを出力しました: {representativeRankingCsvPath}");
        Console.WriteLine($"representative順位表Markdownを出力しました: {representativeRankingMarkdownPath}");
        Console.WriteLine($"representative大会最終状態CSVを出力しました: {tournamentMatchRecordsCsvPath}");
        Console.WriteLine($"representative大会最終状態Markdownを出力しました: {tournamentMatchRecordsMarkdownPath}");
    }

    static TournamentFrameworkSimulationAggregate ExecuteTournamentFrameworkModeCalculation(
        TournamentEngine engine,
        TournamentState initialState,
        IReadOnlyList<PlayerEntry> players,
        TournamentRuleSetMode tournamentRuleSetMode,
        double firstPlayerWinRateRating,
        int? requestedSimulationCount)
    {
        if (initialState.MatchRecords.Count <= TournamentFrameworkExactCalculationMatchThreshold) return CalculateTournamentFrameworkExactly(engine, initialState, players, tournamentRuleSetMode, firstPlayerWinRateRating);

        var simulationCount = requestedSimulationCount ?? DefaultTournamentFrameworkSimulationCount;
        var placeProbabilities = new double[players.Count, players.Count];
        var playerIndexById = players
            .OrderBy(player => player.PlayerId)
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);
        var completedSimulationCount = 0;
        var completedNaturallyCount = 0;
        long totalTickCount = 0;
        TournamentFrameworkExecutionResult? representativeExecutionResult = null;

        using var simulationBudget = SimulationTimeBudget.BeginSimulationBudget();
        for (var simulation = 0; simulation < simulationCount; simulation++)
        {
            if (!SimulationTimeBudget.HasSimulationTimeRemaining()) break;

            var executionResult = engine.Run(initialState);
            AccumulateTournamentFrameworkPlaceProbabilities(players, playerIndexById, executionResult.FinalState.MatchRecords, placeProbabilities, tournamentRuleSetMode);
            representativeExecutionResult = executionResult;
            totalTickCount += executionResult.TickCount;
            if (executionResult.CompletedNaturally)
            {
                completedNaturallyCount++;
            }

            completedSimulationCount++;
        }

        representativeExecutionResult ??= engine.Run(initialState);

        SimulationTimeBudget.NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

        return new TournamentFrameworkSimulationAggregate(
            placeProbabilities,
            simulationCount,
            completedSimulationCount,
            completedNaturallyCount,
            completedSimulationCount == 0 ? 0.0 : (double)totalTickCount / completedSimulationCount,
            false,
            tournamentRuleSetMode,
            representativeExecutionResult);
    }

    static TournamentFrameworkSimulationAggregate CalculateTournamentFrameworkExactly(
        TournamentEngine engine,
        TournamentState initialState,
        IReadOnlyList<PlayerEntry> players,
        TournamentRuleSetMode tournamentRuleSetMode,
        double firstPlayerWinRateRating)
    {
        var placeProbabilities = new double[players.Count, players.Count];
        var playerIndexById = players
            .OrderBy(player => player.PlayerId)
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);
        var playerById = players.ToDictionary(player => player.PlayerId);
        var matches = initialState.MatchRecords.ToArray();
        var completedScenarioWeight = 0.0;

        void Explore(int matchIndex, double scenarioProbability)
        {
            if (!SimulationTimeBudget.HasApplicationTimeRemaining()) return;

            if (matchIndex == matches.Length)
            {
                completedScenarioWeight += scenarioProbability;
                var finalState = initialState with
                {
                    MatchRecords = matches
                        .Select(match => match with { Status = MatchStatus.Finished })
                        .ToArray(),
                };
                AccumulateTournamentFrameworkPlaceProbabilities(players, playerIndexById, finalState.MatchRecords, placeProbabilities, tournamentRuleSetMode, scenarioProbability);
                return;
            }

            var match = matches[matchIndex];
            if (match.ResultType != MatchResultType.None)
            {
                matches[matchIndex] = match with { Status = MatchStatus.Finished };
                Explore(matchIndex + 1, scenarioProbability);
                matches[matchIndex] = match;
                return;
            }

            var firstPlayerEntry = playerById[match.FirstPlayerId];
            var secondPlayerEntry = playerById[match.SecondPlayerId];
            var firstPlayer = new Player(firstPlayerEntry.Name, firstPlayerEntry.Rating);
            var secondPlayer = new Player(secondPlayerEntry.Name, secondPlayerEntry.Rating);
            var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(firstPlayer, secondPlayer, firstPlayerWinRateRating);

            matches[matchIndex] = match with
            {
                Status = MatchStatus.Finished,
                ResultType = MatchResultType.FirstPlayerWin,
            };
            Explore(matchIndex + 1, scenarioProbability * firstPlayerWinProbability);

            matches[matchIndex] = match with
            {
                Status = MatchStatus.Finished,
                ResultType = MatchResultType.SecondPlayerWin,
            };
            Explore(matchIndex + 1, scenarioProbability * (1.0 - firstPlayerWinProbability));
            matches[matchIndex] = match;
        }

        Explore(0, 1.0);
        SimulationTimeBudget.NormalizePlaceProbabilities(placeProbabilities, completedScenarioWeight);
        var representativeExecutionResult = engine.Run(initialState);
        return new TournamentFrameworkSimulationAggregate(
            placeProbabilities,
            1,
            completedScenarioWeight < 1.0 ? 0 : 1,
            completedScenarioWeight < 1.0 ? 0 : representativeExecutionResult.CompletedNaturally ? 1 : 0,
            representativeExecutionResult.TickCount,
            true,
            tournamentRuleSetMode,
            representativeExecutionResult);
    }

    static void AccumulateTournamentFrameworkPlaceProbabilities(
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyDictionary<int, int> playerIndexById,
        IReadOnlyList<TournamentMatchRecord> matchRecords,
        double[,] placeProbabilities,
        TournamentRuleSetMode tournamentRuleSetMode,
        double weight = 1.0)
    {
        if (tournamentRuleSetMode is TournamentRuleSetMode.Twill or TournamentRuleSetMode.TwillCommonOpponentWeighted)
        {
            AccumulateTournamentFrameworkTwillPlaces(players, playerIndexById, matchRecords, placeProbabilities, tournamentRuleSetMode, weight);
            return;
        }

        AccumulateTournamentFrameworkNeutralPlaces(players, playerIndexById, matchRecords, placeProbabilities, weight);
    }

    static void AccumulateTournamentFrameworkNeutralPlaces(
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyDictionary<int, int> playerIndexById,
        IReadOnlyList<TournamentMatchRecord> matchRecords,
        double[,] placeProbabilities,
        double weight)
    {
        var pointsByPlayerId = BuildTournamentFrameworkPointsByPlayerId(players, matchRecords);

        var ranking = players
            .Select(player => new PlayerScore(playerIndexById[player.PlayerId], pointsByPlayerId[player.PlayerId]))
            .OrderByDescending(score => score.Wins)
            .ThenBy(score => score.PlayerIndex)
            .ToArray();

        var currentPlace = 0;
        while (currentPlace < ranking.Length)
        {
            var groupEnd = currentPlace + 1;
            while (groupEnd < ranking.Length && ranking[groupEnd].Wins == ranking[currentPlace].Wins)
            {
                groupEnd++;
            }

            var groupSize = groupEnd - currentPlace;
            var splitWeight = weight / groupSize;
            for (var i = currentPlace; i < groupEnd; i++)
            {
                var playerIndex = ranking[i].PlayerIndex;
                for (var place = currentPlace; place < groupEnd; place++)
                {
                    placeProbabilities[playerIndex, place] += splitWeight;
                }
            }

            currentPlace = groupEnd;
        }
    }

    static void AccumulateTournamentFrameworkTwillPlaces(
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyDictionary<int, int> playerIndexById,
        IReadOnlyList<TournamentMatchRecord> matchRecords,
        double[,] placeProbabilities,
        TournamentRuleSetMode tournamentRuleSetMode,
        double weight)
    {
        var standardMatches = new Match[matchRecords.Count];
        var blackWins = new bool[matchRecords.Count];

        for (var matchIndex = 0; matchIndex < matchRecords.Count; matchIndex++)
        {
            var match = matchRecords[matchIndex];
            standardMatches[matchIndex] = new Match(
                playerIndexById[match.FirstPlayerId],
                playerIndexById[match.SecondPlayerId]);

            blackWins[matchIndex] = match.ResultType switch
            {
                MatchResultType.FirstPlayerWin or MatchResultType.FirstPlayerForfeitWin => true,
                MatchResultType.SecondPlayerWin or MatchResultType.SecondPlayerForfeitWin => false,
                _ => throw new OperationCanceledException($"Twill 系順位ルールでは未対応の対局結果です: {match.ResultType}"),
            };
        }

        if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
        {
            TwillTournamentRule.AccumulatePlaceProbabilities(standardMatches, blackWins, weight, placeProbabilities);
            return;
        }

        TwillTournamentRule.AccumulatePlaceProbabilitiesWithCommonOpponentWeight(standardMatches, blackWins, weight, placeProbabilities);
    }

    static List<RepresentativeExecutionRankRow> BuildRepresentativeExecutionRankRows(
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyList<PlayerRankRow> ranking)
    {
        var playerNameById = players.ToDictionary(player => player.PlayerId, player => player.Name);

        return ranking
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

    static Dictionary<int, int> BuildTournamentFrameworkPointsByPlayerId(
        IReadOnlyList<PlayerEntry> players,
        IReadOnlyList<TournamentMatchRecord> matchRecords)
    {
        var pointsByPlayerId = players.ToDictionary(player => player.PlayerId, _ => 0);
        foreach (var match in matchRecords.Where(match => match.Status == MatchStatus.Finished))
        {
            switch (match.ResultType)
            {
                case MatchResultType.FirstPlayerWin:
                case MatchResultType.FirstPlayerForfeitWin:
                    pointsByPlayerId[match.FirstPlayerId]++;
                    break;
                case MatchResultType.SecondPlayerWin:
                case MatchResultType.SecondPlayerForfeitWin:
                    pointsByPlayerId[match.SecondPlayerId]++;
                    break;
            }
        }

        return pointsByPlayerId;
    }

    static CalculationResult BuildTournamentFrameworkCalculationResult(TournamentFrameworkSimulationAggregate aggregateResult)
    {
        var ruleSetModeLabel = aggregateResult.TournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => "Twill",
            TournamentRuleSetMode.TwillCommonOpponentWeighted => "Twill+CommonOpp",
            _ => "Neutral",
        };
        var modeCoreLabel = aggregateResult.IsExactCalculation
            ? $"厳密計算 / 大会進行フレームワーク / FixedMatch / {ruleSetModeLabel}"
            : $"大会進行フレームワーク / FixedMatch / {ruleSetModeLabel}";
        if (aggregateResult.IsExactCalculation)
        {
            var exactModeLabel = aggregateResult.CompletedSimulationCount < aggregateResult.RequestedSimulationCount
                ? $"{modeCoreLabel} (途中打ち切り, 時間切れ)"
                : modeCoreLabel;
            return new CalculationResult(aggregateResult.PlaceProbabilities, exactModeLabel, null);
        }

        var modeLabel = aggregateResult.CompletedSimulationCount < aggregateResult.RequestedSimulationCount
            ? $"{modeCoreLabel} ({aggregateResult.CompletedSimulationCount:N0}/{aggregateResult.RequestedSimulationCount:N0}回, 時間切れ)"
            : $"{modeCoreLabel} ({aggregateResult.CompletedSimulationCount:N0}回)";

        return new CalculationResult(aggregateResult.PlaceProbabilities, modeLabel, aggregateResult.CompletedSimulationCount);
    }

    sealed record class TournamentFrameworkSimulationAggregate(
        double[,] PlaceProbabilities,
        int RequestedSimulationCount,
        int CompletedSimulationCount,
        int CompletedNaturallyCount,
        double AverageTickCount,
        bool IsExactCalculation,
        TournamentRuleSetMode TournamentRuleSetMode,
        TournamentFrameworkExecutionResult RepresentativeExecutionResult);

    sealed class StandardLikeMatchResultResolver(double firstPlayerWinRateRating) : IMatchResultResolver
    {
        readonly double _firstPlayerWinRateRating = firstPlayerWinRateRating;

        public TournamentMatchRecord Resolve(TournamentState state, TournamentMatchRecord match, Random random)
        {
            if (match.ResultType != MatchResultType.None) return match;

            var playerMap = state.Players.ToDictionary(player => player.PlayerId);
            var firstPlayerEntry = playerMap[match.FirstPlayerId];
            var secondPlayerEntry = playerMap[match.SecondPlayerId];
            var firstPlayer = new Player(firstPlayerEntry.Name, firstPlayerEntry.Rating);
            var secondPlayer = new Player(secondPlayerEntry.Name, secondPlayerEntry.Rating);
            var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(firstPlayer, secondPlayer, _firstPlayerWinRateRating);
            var resultType = random.NextDouble() < firstPlayerWinProbability
                ? MatchResultType.FirstPlayerWin
                : MatchResultType.SecondPlayerWin;

            return match with { ResultType = resultType };
        }
    }
}
