internal static partial class Program
{
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
        var ruleSet = new TournamentFrameworkRuleSet(
            FixedMatchPairingRule.Instance,
            ByFinishedResultsRankingRule.Instance,
            AllMatchesFinishedTerminationRule.Instance,
            new StandardLikeMatchResultResolver(context.FirstPlayerWinRateRating));
        var engine = new TournamentEngine(ruleSet, context.RandomSeed);
        var executionResult = engine.Run(initialState);

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

        var result = BuildTournamentFrameworkCalculationResult(players, executionResult.OverallRanking);
        var resultRows = BuildResultRows(standardPlayers, standardMatches, result, context.FirstPlayerWinRatePercent);

        Console.WriteLine($"進行Tick数: {executionResult.TickCount}");
        Console.WriteLine($"自然終了: {(executionResult.CompletedNaturally ? "Yes" : "No")}");
        Console.WriteLine($"ステージ数: {stages.Count}");
        Console.WriteLine($"総対局数: {matchRecords.Count}\n");
        if (dslDefinition is not null)
        {
            Console.WriteLine($"DSL TimeAxis: {dslDefinition.TimeAxis}");
            Console.WriteLine($"DSL OverallRanking: {dslDefinition.OverallRankingRuleName}\n");
        }

        PrintMatchesCsv(standardPlayers, standardMatches, "大会進行フレームワークで読み込んだ対局CSV:");
        PrintResult(standardPlayers.Length, result, context.FirstPlayerWinRatePercent, resultRows);

        var defaultOutputCsvPath = Path.GetFullPath($"tournament_framework_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var requestedOutputPath = string.IsNullOrWhiteSpace(context.OutputPath)
            ? ReadTextWithDefault($"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ", defaultOutputCsvPath)
            : context.OutputPath!;
        var outputCsvPath = ResolveOutputCsvPath(requestedOutputPath);
        WriteResultCsv(outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, resultRows);
        var outputMarkdownPath = ChangeOutputExtension(outputCsvPath, ".md");
        WriteResultMarkdown(outputMarkdownPath, outputCsvPath, result.Mode, context.FirstPlayerWinRatePercent, resultRows);
        var tournamentMatchRecordsCsvPath = BuildSiblingOutputCsvPath(outputCsvPath, "tournament_match_records");
        var tournamentMatchRecordsMarkdownPath = ChangeOutputExtension(tournamentMatchRecordsCsvPath, ".md");
        WriteTournamentMatchRecordCsv(tournamentMatchRecordsCsvPath, stages, players, executionResult.FinalState.MatchRecords);
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

    static CalculationResult BuildTournamentFrameworkCalculationResult(IReadOnlyList<PlayerEntry> players, IReadOnlyList<PlayerRankRow> ranking)
    {
        var placeProbabilities = new double[players.Count, players.Count];
        var playerIndexById = players
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);

        foreach (var row in ranking)
        {
            if (!playerIndexById.TryGetValue(row.PlayerId, out var playerIndex)
                || row.Rank <= 0
                || row.Rank > players.Count)
            {
                continue;
            }

            placeProbabilities[playerIndex, row.Rank - 1] = 1.0;
        }

        return new CalculationResult(placeProbabilities, "大会進行フレームワーク / FixedMatch", null);
    }

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
