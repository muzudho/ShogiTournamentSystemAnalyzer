/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static SimulationBudgetScope BeginSimulationBudget()
    {
        var ownsBudget = !Program._simulationDeadlineUtc.HasValue;
        if (ownsBudget)
        {
            Program._simulationDeadlineUtc = DateTime.UtcNow + Program.SimulationTimeLimit;
        }

        return new SimulationBudgetScope(ownsBudget);
    }

    static bool HasSimulationTimeRemaining()
    {
        return !Program._simulationDeadlineUtc.HasValue || DateTime.UtcNow < Program._simulationDeadlineUtc.Value;
    }

    static void NormalizePlaceProbabilities(double[,] placeProbabilities, int sampleCount)
    {
        if (sampleCount <= 0) return;

        for (var row = 0; row < placeProbabilities.GetLength(0); row++)
        {
            for (var column = 0; column < placeProbabilities.GetLength(1); column++)
            {
                placeProbabilities[row, column] /= sampleCount;
            }
        }
    }

    readonly record struct SimulationBudgetScope(bool OwnsBudget) : IDisposable
    {
        public void Dispose()
        {
            if (OwnsBudget)
            {
                Program._simulationDeadlineUtc = null;
            }
        }
    }
}

