using ShogiTournamentSystemAnalyzer.Infrastructure.Csv;

internal static partial class Program
{
    const int DefaultTournamentFrameworkSimulationCount = 200_000;
    const int TournamentFrameworkExactCalculationMatchThreshold = 20;

    static void ExecuteTournamentFrameworkMode(TournamentFrameworkModeContext context)
    {
        var players = ReadPlayerEntriesFromCsvPath(context.PlayersCsvPath);
        var stages = ReadStageEntriesFromCsvPath(context.StagesCsvPath);
        var matchRecords = ReadTournamentMatchRecordsFromCsvPath(context.TournamentMatchRecordsCsvPath);

        TournamentDslDefinition? dslDefinition = null;
        if (!string.IsNullOrWhiteSpace(context.RuleFilePath))
        {
            dslDefinition = ParseTournamentDsl(File.ReadAllText(Path.GetFullPath(context.RuleFilePath)), context.RuleFilePath!);
            Console.WriteLine($"大会ルールDSLを読み込みました: {context.RuleFilePath}");
        }

        var initialState = new TournamentState(0, players, stages, matchRecords);
        IRankingRule rankingRule = context.TournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => TwillTournamentRankingRule.Instance,
            TournamentRuleSetMode.TwillCommonOpponentWeighted => TwillTournamentRankingRule.CommonOpponentWeightedInstance,
            _ => ByFinishedResultsRankingRule.Instance,
        };
        var ruleSet = new TournamentFrameworkRuleSet(
            FixedMatchPairingRule.Instance,
            rankingRule,
            AllMatchesFinishedTerminationRule.Instance,
            new StandardLikeMatchResultResolver(context.FirstPlayerWinRateRating));
        var engine = new TournamentEngine(ruleSet, context.RandomSeed);
        var aggregateResult = ExecuteTournamentFrameworkModeCalculation(engine, initialState, players, context.TournamentRuleSetMode, context.FirstPlayerWinRateRating, context.SimulationCount);
        var executionResult = aggregateResult.RepresentativeExecutionResult;

        var standardPlayers = players
            .OrderBy(player => player.PlayerId)
            .Select(player => new Player(player.Name, player.Rating))
            .ToArray();
        var playerIndexById = players
            .OrderBy(player => player.PlayerId)
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);
        var standardMatches = executionResult.FinalState.MatchRecords
            .Select(match => new Match(playerIndexById[match.FirstPlayerId], playerIndexById[match.SecondPlayerId]))
            .ToArray();
        var representativeExecutionRankRows = BuildRepresentativeExecutionRankRows(players, executionResult.OverallRanking);

        var result = BuildTournamentFrameworkCalculationResult(aggregateResult);
        var resultRows = BuildResultRows(standardPlayers, standardMatches, result, context.FirstPlayerWinRatePercent);

        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}");

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

        Console.WriteLine($"代表実行Tick数: {executionResult.TickCount}");
        Console.WriteLine($"代表実行の自然終了: {(executionResult.CompletedNaturally ? "Yes" : "No")}");
        Console.WriteLine($"ステージ数: {stages.Count}");
        Console.WriteLine($"総対局数: {matchRecords.Count}\n");
        if (dslDefinition is not null)
        {
            Console.WriteLine($"DSL TimeAxis: {dslDefinition.TimeAxis}");
            Console.WriteLine($"DSL OverallRanking: {dslDefinition.OverallRankingRuleName}\n");
        }

        PrintMatchesCsv(standardPlayers, standardMatches, "大会進行フレームワークで読み込んだ対局CSV:");
        Console.WriteLine("注記: これ以降の順位表は複数回試行の aggregate 結果です。");
        Console.WriteLine("注記: あとで出力する大会結果CSV/Markdownは代表実行1件の対局記録です。\n");
        PrintRepresentativeExecutionRanking(representativeExecutionRankRows, context.TournamentRuleSetMode);
        PrintResult(standardPlayers.Length, result, context.FirstPlayerWinRatePercent, resultRows);
        if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var defaultOutputCsvPath = Path.GetFullPath($"tournament_framework_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var requestedOutputPath = string.IsNullOrWhiteSpace(context.OutputPath)
            ? ReadTextWithDefault($"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ", defaultOutputCsvPath)
            : context.OutputPath!;
        var outputCsvPath = ResolveOutputCsvPath(requestedOutputPath);
        WriterHelper.WriteText(
            outputPath: outputCsvPath,
            getLines: () => CreateResultCsv(result.Mode, context.FirstPlayerWinRatePercent, resultRows));

        var outputMarkdownPath = ChangeOutputExtension(outputCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: outputMarkdownPath,
            getLines: () => CreateResultMarkdown(
                outputMarkdownPath,
                outputCsvPath,
                result.Mode,
                context.FirstPlayerWinRatePercent,
                resultRows,
                overviewNote: "この順位表は複数回試行の aggregate 結果です。下記の大会結果テーブルとは 1 対 1 には対応しません。"));

        var tournamentMatchRecordsCsvPath = BuildSiblingOutputCsvPath(outputCsvPath, "tournament_match_records");
        var tournamentMatchRecordsMarkdownPath = ChangeOutputExtension(tournamentMatchRecordsCsvPath, ".md");
        WriterHelper.WriteText(
            outputPath: tournamentMatchRecordsCsvPath,
            getLines: () => CreateTournamentMatchRecordCsv(tournamentMatchRecordsCsvPath, stages, players, executionResult.FinalState.MatchRecords));

        WriteTournamentMatchRecordMarkdown(tournamentMatchRecordsMarkdownPath, tournamentMatchRecordsCsvPath, stages, players, executionResult.FinalState.MatchRecords);
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
        Console.WriteLine($"結果Markdownを出力しました: {outputMarkdownPath}");
        Console.WriteLine($"大会結果CSVを出力しました: {tournamentMatchRecordsCsvPath}");
        Console.WriteLine($"大会結果Markdownを出力しました: {tournamentMatchRecordsMarkdownPath}");
    }

    static List<PlayerEntry> ReadPlayerEntriesFromCsvPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var lines = File.ReadAllLines(fullPath);
        if (!TryParsePlayerEntries(lines, out var players, out var errorMessage))
        {
            throw new OperationCanceledException($"選手一覧CSVの読み取りに失敗しました: {errorMessage} ({fullPath})");
        }

        return players;
    }

    static List<StageEntry> ReadStageEntriesFromCsvPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var lines = File.ReadAllLines(fullPath);
        if (!TryParseStageEntries(lines, out var stages, out var errorMessage))
        {
            throw new OperationCanceledException($"ステージ一覧CSVの読み取りに失敗しました: {errorMessage} ({fullPath})");
        }

        return stages;
    }

    static List<TournamentMatchRecord> ReadTournamentMatchRecordsFromCsvPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var lines = File.ReadAllLines(fullPath);
        if (!TryParseTournamentMatchRecords(lines, out var matches, out var errorMessage))
        {
            throw new OperationCanceledException($"大会対局記録CSVの読み取りに失敗しました: {errorMessage} ({fullPath})");
        }

        return matches;
    }

    static TournamentFrameworkSimulationAggregate ExecuteTournamentFrameworkModeCalculation(
        TournamentEngine engine,
        TournamentState initialState,
        IReadOnlyList<PlayerEntry> players,
        TournamentRuleSetMode tournamentRuleSetMode,
        double firstPlayerWinRateRating,
        int? requestedSimulationCount)
    {
        if (initialState.MatchRecords.Count <= TournamentFrameworkExactCalculationMatchThreshold)
        {
            return CalculateTournamentFrameworkExactly(engine, initialState, players, tournamentRuleSetMode, firstPlayerWinRateRating);
        }

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

        using var simulationBudget = BeginSimulationBudget();
        for (var simulation = 0; simulation < simulationCount; simulation++)
        {
            if (!HasSimulationTimeRemaining())
            {
                break;
            }

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

        if (representativeExecutionResult is null)
        {
            throw new OperationCanceledException("大会進行フレームワークのシミュレーションを 1 回も実行できませんでした。");
        }

        NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

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

        void Explore(int matchIndex, double scenarioProbability)
        {
            if (matchIndex == matches.Length)
            {
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
            var firstPlayerWinProbability = GetWinProbability(firstPlayer, secondPlayer, firstPlayerWinRateRating);

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
        var representativeExecutionResult = engine.Run(initialState);
        return new TournamentFrameworkSimulationAggregate(
            placeProbabilities,
            1,
            1,
            representativeExecutionResult.CompletedNaturally ? 1 : 0,
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
            return new CalculationResult(aggregateResult.PlaceProbabilities, modeCoreLabel, null);
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

    readonly record struct RepresentativeExecutionRankRow(
        string Name,
        int Points,
        string RankLabel,
        double AveragePlace,
        double FirstPlaceProbability);

    sealed class StandardLikeMatchResultResolver(double firstPlayerWinRateRating) : IMatchResultResolver
    {
        readonly double _firstPlayerWinRateRating = firstPlayerWinRateRating;

        public TournamentMatchRecord Resolve(TournamentState state, TournamentMatchRecord match, Random random)
        {
            if (match.ResultType != MatchResultType.None)
            {
                return match;
            }

            var playerMap = state.Players.ToDictionary(player => player.PlayerId);
            var firstPlayerEntry = playerMap[match.FirstPlayerId];
            var secondPlayerEntry = playerMap[match.SecondPlayerId];
            var firstPlayer = new Player(firstPlayerEntry.Name, firstPlayerEntry.Rating);
            var secondPlayer = new Player(secondPlayerEntry.Name, secondPlayerEntry.Rating);
            var firstPlayerWinProbability = GetWinProbability(firstPlayer, secondPlayer, _firstPlayerWinRateRating);
            var resultType = random.NextDouble() < firstPlayerWinProbability
                ? MatchResultType.FirstPlayerWin
                : MatchResultType.SecondPlayerWin;

            return match with { ResultType = resultType };
        }
    }
}
