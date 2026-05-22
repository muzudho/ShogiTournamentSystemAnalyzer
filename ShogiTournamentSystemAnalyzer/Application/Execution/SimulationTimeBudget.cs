/*
 * ［シミュレーション時間管理］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

/// <summary>
/// シミュレーションの実行時間を管理するためのクラスです。
/// </summary>
internal static class SimulationTimeBudget
{
    internal static SimulationBudgetScope BeginSimulationBudget()
    {
        var ownsBudget = !Program._simulationDeadlineUtc.HasValue;
        if (ownsBudget)
        {
            Program._simulationDeadlineUtc = DateTime.UtcNow + Program.SimulationTimeLimit;
        }

        return new SimulationBudgetScope(ownsBudget);
    }

    internal static bool HasSimulationTimeRemaining()
    {
        return !Program._simulationDeadlineUtc.HasValue || DateTime.UtcNow < Program._simulationDeadlineUtc.Value;
    }

    internal static void NormalizePlaceProbabilities(double[,] placeProbabilities, int sampleCount)
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

    internal readonly record struct SimulationBudgetScope(bool OwnsBudget) : IDisposable
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

