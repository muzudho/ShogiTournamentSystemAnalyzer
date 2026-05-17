internal readonly record struct QualityEvaluationExecutionOptions(
    int? SimulationCount,
    QualitySweepOptions SweepOptions,
    double? FirstPlayerWinRatePercent)
{
    internal bool IsSweep => SweepOptions.IsEnabled;
    internal bool UsesSimulation => SimulationCount.HasValue;
}

