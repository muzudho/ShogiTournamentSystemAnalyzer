internal static partial class Program
{
    static CalculationResult CalculateExactly(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, double blackAdvantageRating, TournamentRuleSetMode tournamentRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        var placeProbabilities = new double[players.Count, players.Count];
        var wins = tournamentRuleSetMode == TournamentRuleSetMode.Neutral ? new int[players.Count] : null;
        var outcomes = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? new bool[matches.Count] : null;

        void Explore(int matchIndex, double scenarioProbability)
        {
            if (matchIndex == matches.Count)
            {
                if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
                {
                    TwillTournamentRule.AccumulatePlaceProbabilities(matches, outcomes!, scenarioProbability, placeProbabilities);
                }
                else
                {
                    AccumulatePlaceProbabilities(wins!, scenarioProbability, placeProbabilities);
                }

                return;
            }

            var match = matches[matchIndex];
            var blackWinsProbability = GetWinProbability(players[match.Black], players[match.White], blackAdvantageRating);

            if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
            {
                outcomes![matchIndex] = true;
            }
            else
            {
                wins![match.Black]++;
            }

            Explore(matchIndex + 1, scenarioProbability * blackWinsProbability);
            if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
            {
                wins![match.Black]--;
            }

            if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
            {
                outcomes![matchIndex] = false;
            }
            else
            {
                wins![match.White]++;
            }

            Explore(matchIndex + 1, scenarioProbability * (1.0 - blackWinsProbability));
            if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
            {
                wins![match.White]--;
            }
        }

        Explore(0, 1.0);
        var modeLabel = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "厳密計算 / Twill" : "厳密計算";
        return new CalculationResult(placeProbabilities, modeLabel, null);
    }

    static CalculationResult CalculateBySimulation(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, double blackAdvantageRating, int simulationCount, TournamentRuleSetMode tournamentRuleSetMode = TournamentRuleSetMode.Neutral)
    {
        var placeProbabilities = new double[players.Count, players.Count];
        var wins = tournamentRuleSetMode == TournamentRuleSetMode.Neutral ? new int[players.Count] : null;
        var outcomes = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? new bool[matches.Count] : null;
        var completedSimulationCount = 0;

        for (var simulation = 0; simulation < simulationCount; simulation++)
        {
            if (!HasSimulationTimeRemaining())
            {
                break;
            }

            if (tournamentRuleSetMode == TournamentRuleSetMode.Neutral)
            {
                Array.Clear(wins!);
            }

            for (var matchIndex = 0; matchIndex < matches.Count; matchIndex++)
            {
                var match = matches[matchIndex];
                var blackWinsProbability = GetWinProbability(players[match.Black], players[match.White], blackAdvantageRating);
                if (Random.Shared.NextDouble() < blackWinsProbability)
                {
                    if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
                    {
                        outcomes![matchIndex] = true;
                    }
                    else
                    {
                        wins![match.Black]++;
                    }
                }
                else
                {
                    if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
                    {
                        outcomes![matchIndex] = false;
                    }
                    else
                    {
                        wins![match.White]++;
                    }
                }
            }

            if (tournamentRuleSetMode == TournamentRuleSetMode.Twill)
            {
                TwillTournamentRule.AccumulatePlaceProbabilities(matches, outcomes!, 1.0, placeProbabilities);
            }
            else
            {
                AccumulatePlaceProbabilities(wins!, 1.0, placeProbabilities);
            }

            completedSimulationCount++;
        }

        NormalizePlaceProbabilities(placeProbabilities, completedSimulationCount);

        var modeCoreLabel = tournamentRuleSetMode == TournamentRuleSetMode.Twill
            ? "シミュレーション / Twill"
            : "シミュレーション";
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

    static double GetWinProbability(Player black, Player white, double blackAdvantageRating)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (white.Rating - (black.Rating + blackAdvantageRating)) / 400.0));
    }
}

