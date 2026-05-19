/// <summary>
/// ［品質評価］の実行オプションを表す構造体。
/// </summary>
/// <param name="SimulationCount">シミュレーション回数</param>
/// <param name="SweepOptions">スイープオプション</param>
/// <param name="FirstPlayerWinRatePercent">先手勝率（％）</param>
internal readonly record struct QualityEvaluationExecutionOptions(
    int? SimulationCount,
    QualitySweepOptions SweepOptions,
    double? FirstPlayerWinRatePercent)
{
    /// <summary>
    /// ［スイープ］か。
    /// </summary>
    internal bool IsSweep => SweepOptions.IsEnabled;

    /// <summary>
    /// ［シミュレーション］を使用するか。
    /// </summary>
    internal bool UsesSimulation => SimulationCount.HasValue;
}

