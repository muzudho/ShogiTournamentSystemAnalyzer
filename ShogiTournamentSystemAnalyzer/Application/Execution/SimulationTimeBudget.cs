/*
 * ［シミュレーション時間管理］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

/// <summary>
/// シミュレーションの実行時間を管理するためのクラスです。
/// </summary>
internal static class SimulationTimeBudget
{
    /// <summary>
    /// シミュレーションは最大ｎ分までにするぜ（＾▽＾）！　あまり長くなりすぎると、結果が出る前に心が折れちゃうからな（＾～＾）！
    /// </summary>
    internal static readonly TimeSpan SimulationTimeLimit = TimeSpan.FromMinutes(3);
    static DateTime? _simulationDeadlineUtc;

    internal static SimulationBudgetScope BeginSimulationBudget()
    {
        var ownsBudget = !_simulationDeadlineUtc.HasValue;
        if (ownsBudget)
        {
            _simulationDeadlineUtc = DateTime.UtcNow + SimulationTimeLimit;
        }

        return new SimulationBudgetScope(ownsBudget);
    }

    internal static bool HasSimulationTimeRemaining()
    {
        return !_simulationDeadlineUtc.HasValue || DateTime.UtcNow < _simulationDeadlineUtc.Value;
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
                _simulationDeadlineUtc = null;
            }
        }
    }
}

