/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static partial class Program
{
    static CalculationResult CalculateExactly(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, double firstPlayerWinRateRating, TournamentRuleSetMode tournamentRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        var placeProbabilities = new double[players.Count, players.Count];
        var usesTwillRule = tournamentRuleSetMode is TournamentRuleSetMode.Twill or TournamentRuleSetMode.TwillCommonOpponentWeighted;
        var wins = tournamentRuleSetMode == TournamentRuleSetMode.Neutral ? new int[players.Count] : null;
        var outcomes = usesTwillRule ? new bool[matches.Count] : null;
        var completedScenarioWeight = 0.0;

        void Explore(int matchIndex, double scenarioProbability)
        {
            if (!SimulationTimeBudget.HasApplicationTimeRemaining()) return;

            if (matchIndex == matches.Count)
            {
                completedScenarioWeight += scenarioProbability;
                if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
                {
                    TwillTournamentRule.AccumulatePlaceProbabilities(matches, outcomes!, scenarioProbability, placeProbabilities);
                }
                else if (tournamentRuleSetMode == TournamentRuleSetMode.TwillCommonOpponentWeighted)
                {
                    TwillTournamentRule.AccumulatePlaceProbabilitiesWithCommonOpponentWeight(matches, outcomes!, scenarioProbability, placeProbabilities);
                }
                else
                {
                    AccumulatePlaceProbabilities(wins!, scenarioProbability, placeProbabilities);
                }

                return;
            }

            var match = matches[matchIndex];
            var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(players[match.FirstPlayer], players[match.SecondPlayer], firstPlayerWinRateRating);

            if (usesTwillRule)
            {
                outcomes![matchIndex] = true;
            }
            else
            {
                wins![match.FirstPlayer]++;
            }

            Explore(matchIndex + 1, scenarioProbability * firstPlayerWinProbability);
            if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
            {
                wins![match.FirstPlayer]--;
            }

            if (usesTwillRule)
            {
                outcomes![matchIndex] = false;
            }
            else
            {
                wins![match.SecondPlayer]++;
            }

            Explore(matchIndex + 1, scenarioProbability * (1.0 - firstPlayerWinProbability));
            if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
            {
                wins![match.SecondPlayer]--;
            }
        }

        Explore(0, 1.0);
        SimulationTimeBudget.NormalizePlaceProbabilities(placeProbabilities, completedScenarioWeight);
        var modeLabel = tournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => completedScenarioWeight < 1.0 ? $"厳密計算 / Twill ({(completedScenarioWeight * 100.0).ToString("F2")}%, 時間切れ)" : "厳密計算 / Twill",
            TournamentRuleSetMode.TwillCommonOpponentWeighted => completedScenarioWeight < 1.0 ? $"厳密計算 / Twill+CommonOpp ({(completedScenarioWeight * 100.0).ToString("F2")}%, 時間切れ)" : "厳密計算 / Twill+CommonOpp",
            _ => completedScenarioWeight < 1.0 ? $"厳密計算 ({(completedScenarioWeight * 100.0).ToString("F2")}%, 時間切れ)" : "厳密計算",
        };
        return new CalculationResult(placeProbabilities, modeLabel, null);
    }

    static CalculationResult CalculateBySimulation(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, double firstPlayerWinRateRating, int simulationCount, TournamentRuleSetMode tournamentRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        var placeProbabilities = new double[players.Count, players.Count];
        var usesTwillRule = tournamentRuleSetMode is TournamentRuleSetMode.Twill or TournamentRuleSetMode.TwillCommonOpponentWeighted;
        var wins = tournamentRuleSetMode == TournamentRuleSetMode.Neutral ? new int[players.Count] : null;
        var outcomes = usesTwillRule ? new bool[matches.Count] : null;
        var completedSimulationCount = 0;

        for (var simulation = 0; simulation < simulationCount; simulation++)
        {
            if (!SimulationTimeBudget.HasSimulationTimeRemaining()) break;

            if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
            {
                Array.Clear(wins!);
            }

            for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
            {
                if (!SimulationTimeBudget.HasApplicationTimeRemaining())
                {
                    simulation = simulationCount;
                    break;
                }

                var match = matches[matchIndex];
                var firstPlayerWinProbability = SimulationRatingMath.GetWinProbability(players[match.FirstPlayer], players[match.SecondPlayer], firstPlayerWinRateRating);
                if (Random.Shared.NextDouble() < firstPlayerWinProbability)
                {
                    if (usesTwillRule)
                    {
                        outcomes![matchIndex] = true;
                    }
                    else
                    {
                        wins![match.FirstPlayer]++;
                    }
                }
                else
                {
                    if (usesTwillRule)
                    {
                        outcomes![matchIndex] = false;
                    }
                    else
                    {
                        wins![match.SecondPlayer]++;
                    }
                }
            }

            if (!SimulationTimeBudget.HasApplicationTimeRemaining()) break;

            if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
            {
                TwillTournamentRule.AccumulatePlaceProbabilities(matches, outcomes!, 1.0, placeProbabilities);
            }
            else if (tournamentRuleSetMode == TournamentRuleSetMode.TwillCommonOpponentWeighted)
            {
                TwillTournamentRule.AccumulatePlaceProbabilitiesWithCommonOpponentWeight(matches, outcomes!, 1.0, placeProbabilities);
            }
            else
            {
                AccumulatePlaceProbabilities(wins!, 1.0, placeProbabilities);
            }

            completedSimulationCount++;
        }

        SimulationTimeBudget.NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

        var modeCoreLabel = tournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => "シミュレーション / Twill",
            TournamentRuleSetMode.TwillCommonOpponentWeighted => "シミュレーション / Twill+CommonOpp",
            _ => "シミュレーション",
        };
        var modeLabel = completedSimulationCount < simulationCount
            ? $"{modeCoreLabel} ({completedSimulationCount:N0}/{simulationCount:N0}回, 時間切れ)"
            : $"{modeCoreLabel} ({simulationCount:N0}回)";
        return new CalculationResult(placeProbabilities, modeLabel, completedSimulationCount);
    }

    static void AccumulatePlaceProbabilities(int[] wins, double scenarioProbability, double[,] placeProbabilities)
    {
        var ranking = wins
            .Select((winCount, index) => new PlayerScore(index, winCount))
            .OrderByDescending(x => x.Wins)
            .ThenBy(x => x.PlayerIndex)
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
            var splitProbability = scenarioProbability / groupSize;

            for (var i = currentPlace; i < groupEnd; i++)
            {
                var playerIndex = ranking[i].PlayerIndex;
                for (var place = currentPlace; place < groupEnd; place++)
                {
                    placeProbabilities[playerIndex, place] += splitProbability;
                }
            }

            currentPlace = groupEnd;
        }
    }
}

