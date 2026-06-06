/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;

/// <summary>
/// ［シミュレーション　＞　大会フレームワークモード］の主フロー
/// </summary>
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

        // ［大会進行フレームワーク実行設定］
        var executionSettings = TournamentFrameworkExecutionSettings.FromContext(context, dslDefinition);

        // ［順位設定データ］

        // ［初回状態］
        var initialState = new TournamentState(0, players, stages, matchRecords);

        // ［順位付けの設定］選択
        IRankingRule rankingRule = executionSettings.TournamentRuleSetMode switch
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
            new StandardLikeMatchResultResolver(executionSettings.FirstPlayerWinRateRating));

        // ［大会エンジン］
        var engine = new TournamentEngine(ruleSet, executionSettings.RandomSeed);

        // ［集計結果］
        var aggregateResult = ExecuteTournamentFrameworkModeCalculation(engine, initialState, players, executionSettings.TournamentRuleSetMode, executionSettings.FirstPlayerWinRateRating, context.SimulationCount);

        // ［実行結果］
        var executionResult = aggregateResult.RepresentativeExecutionResult;

        // ［最終順位付け結果］
        var finalRankingResult = FinalRankingDomain.BuildTournamentFrameworkFinalRankingResult(
            players,
            stages,
            executionResult.FinalState,
            executionResult.OverallRanking,
            executionResult.TickCount,
            executionResult.CompletedNaturally,
            aggregateResult.PlaceProbabilities,
            aggregateResult.RequestedSimulationCount,
            aggregateResult.CompletedSimulationCount,
            aggregateResult.IsExactCalculation,
            aggregateResult.TournamentRuleSetMode,
            executionSettings.FirstPlayerWinRatePercent);

        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(finalRankingResult.TournamentRuleSetMode)}");

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

        Console.WriteLine($"代表実行Tick数: {finalRankingResult.RepresentativeTournamentFinalState.TickCount}");
        Console.WriteLine($"代表実行の自然終了: {(finalRankingResult.RepresentativeTournamentFinalState.CompletedNaturally ? "Yes" : "No")}");
        Console.WriteLine($"ステージ数: {finalRankingResult.RepresentativeStages.Count}");
        Console.WriteLine($"総対局数: {finalRankingResult.RepresentativeTournamentFinalState.MatchRecords.Count}\n");
        if (executionSettings.DslDefinition is not null)
        {
            Console.WriteLine($"DSL TimeAxis: {executionSettings.DslDefinition.TimeAxis}");
            Console.WriteLine($"DSL OverallRanking: {executionSettings.DslDefinition.OverallRankingRuleName}\n");
        }

        FinalRankingDomain.PrintTournamentFrameworkSimulationResults(finalRankingResult);

        FinalRankingDomain.WriteTournamentFrameworkSimulationOutputs(
            context.OutputPath,
            finalRankingResult);
    }

    /// <summary>
    /// ［大会フレームワーク］モード計算
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="initialState"></param>
    /// <param name="players"></param>
    /// <param name="tournamentRuleSetMode"></param>
    /// <param name="firstPlayerWinRateRating"></param>
    /// <param name="requestedSimulationCount"></param>
    /// <returns></returns>
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
            FinalRankingDomain.AccumulateTournamentFrameworkPlaceProbabilities(players, playerIndexById, executionResult.FinalState.MatchRecords, placeProbabilities, tournamentRuleSetMode);
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

    /// <summary>
    /// 厳密な？［大会フレームワーク］を計算
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="initialState"></param>
    /// <param name="players"></param>
    /// <param name="tournamentRuleSetMode"></param>
    /// <param name="firstPlayerWinRateRating"></param>
    /// <returns></returns>
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
        using var exactCalculationBudget = SimulationTimeBudget.BeginSimulationBudget();

        void Explore(int matchIndex, double scenarioProbability)
        {
            if (!SimulationTimeBudget.HasSimulationTimeRemaining()) return;

            if (matchIndex == matches.Length)
            {
                completedScenarioWeight += scenarioProbability;
                var finalState = initialState with
                {
                    MatchRecords = matches
                        .Select(match => match with { Status = MatchStatus.Finished })
                        .ToArray(),
                };
                FinalRankingDomain.AccumulateTournamentFrameworkPlaceProbabilities(players, playerIndexById, finalState.MatchRecords, placeProbabilities, tournamentRuleSetMode, scenarioProbability);
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


    /// <summary>
    /// 集計
    /// </summary>
    /// <param name="PlaceProbabilities"></param>
    /// <param name="RequestedSimulationCount"></param>
    /// <param name="CompletedSimulationCount"></param>
    /// <param name="CompletedNaturallyCount"></param>
    /// <param name="AverageTickCount"></param>
    /// <param name="IsExactCalculation"></param>
    /// <param name="TournamentRuleSetMode"></param>
    /// <param name="RepresentativeExecutionResult"></param>
    sealed record class TournamentFrameworkSimulationAggregate(
        double[,] PlaceProbabilities,
        int RequestedSimulationCount,
        int CompletedSimulationCount,
        int CompletedNaturallyCount,
        double AverageTickCount,
        bool IsExactCalculation,
        TournamentRuleSetMode TournamentRuleSetMode,
        TournamentFrameworkExecutionResult RepresentativeExecutionResult);

    /// <summary>
    /// 標準ルールの何か。
    /// </summary>
    /// <param name="firstPlayerWinRateRating"></param>
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
