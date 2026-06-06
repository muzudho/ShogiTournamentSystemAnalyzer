namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// Runs exact and approximate TournamentFramework simulations.
/// </summary>
static class TournamentFrameworkSimulationCalculator
{
    const int DefaultTournamentFrameworkSimulationCount = 200_000;
    const int TournamentFrameworkExactCalculationMatchThreshold = 20;

    internal static TournamentFrameworkSimulationAggregate Execute(
        TournamentEngine engine,
        TournamentState initialState,
        IReadOnlyList<PlayerEntry> players,
        TournamentRuleSetMode tournamentRuleSetMode,
        double firstPlayerWinRateRating,
        int? requestedSimulationCount)
    {
        if (initialState.MatchRecords.Count <= TournamentFrameworkExactCalculationMatchThreshold)
        {
            return CalculateExactly(engine, initialState, players, tournamentRuleSetMode, firstPlayerWinRateRating);
        }

        var simulationCount = requestedSimulationCount ?? DefaultTournamentFrameworkSimulationCount;
        var placeProbabilities = new double[players.Count, players.Count];
        var playerIndexById = BuildPlayerIndexById(players);
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

    static TournamentFrameworkSimulationAggregate CalculateExactly(
        TournamentEngine engine,
        TournamentState initialState,
        IReadOnlyList<PlayerEntry> players,
        TournamentRuleSetMode tournamentRuleSetMode,
        double firstPlayerWinRateRating)
    {
        var placeProbabilities = new double[players.Count, players.Count];
        var playerIndexById = BuildPlayerIndexById(players);
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

    static IReadOnlyDictionary<int, int> BuildPlayerIndexById(IReadOnlyList<PlayerEntry> players)
    {
        return players
            .OrderBy(player => player.PlayerId)
            .Select((player, index) => new { player.PlayerId, index })
            .ToDictionary(x => x.PlayerId, x => x.index);
    }
}