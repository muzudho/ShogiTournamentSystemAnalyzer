/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

/// <summary>
/// シミュレーションの実行時間を管理するためのクラスです。
/// </summary>
internal static class SimulationTimeBudget
{
    /// <summary>
    /// アプリ全体も最大ｎ分で打ち切るぜ（＾～＾）！
    /// </summary>
    internal static readonly TimeSpan ApplicationTimeLimit = TimeSpan.FromMinutes(3);

    /// <summary>
    /// シミュレーションは最大ｎ分までにするぜ（＾▽＾）！　あまり長くなりすぎると、結果が出る前に心が折れちゃうからな（＾～＾）！
    /// </summary>
    internal static readonly TimeSpan SimulationTimeLimit = TimeSpan.FromMinutes(3);
    static DateTime? _applicationDeadlineUtc;
    static DateTime? _simulationDeadlineUtc;

    internal static void BeginApplicationBudget()
    {
        _applicationDeadlineUtc = DateTime.UtcNow + ApplicationTimeLimit;
    }

    internal static bool HasApplicationTimeRemaining()
    {
        return !_applicationDeadlineUtc.HasValue || DateTime.UtcNow < _applicationDeadlineUtc.Value;
    }

    internal static void ThrowIfApplicationTimeExpired(string operationLabel)
    {
        if (HasApplicationTimeRemaining()) return;

        throw new OperationCanceledException($"プログラム開始から {ApplicationTimeLimit.TotalMinutes:F0} 分を超えたため、{operationLabel}を打ち切りました。");
    }

    internal static SimulationBudgetScope BeginSimulationBudget()
    {
        var ownsBudget = !_simulationDeadlineUtc.HasValue;
        if (ownsBudget && HasApplicationTimeRemaining())
        {
            _simulationDeadlineUtc = DateTime.UtcNow + SimulationTimeLimit;
        }

        return new SimulationBudgetScope(ownsBudget);
    }

    internal static bool HasSimulationTimeRemaining()
    {
        return HasApplicationTimeRemaining()
            && (!_simulationDeadlineUtc.HasValue || DateTime.UtcNow < _simulationDeadlineUtc.Value);
    }

    internal static void NormalizePlaceProbabilities(double[,] placeProbabilities, int sampleCount)
    {
        if (sampleCount <= 0) return;

        NormalizePlaceProbabilities(placeProbabilities, (double)sampleCount);
    }

    internal static void NormalizePlaceProbabilities(double[,] placeProbabilities, double totalWeight)
    {
        if (totalWeight <= 0.0) return;

        for (var row = 0; row < placeProbabilities.GetLength(0); row++)
        {
            for (var column = 0; column < placeProbabilities.GetLength(1); column++)
            {
                placeProbabilities[row, column] /= totalWeight;
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

