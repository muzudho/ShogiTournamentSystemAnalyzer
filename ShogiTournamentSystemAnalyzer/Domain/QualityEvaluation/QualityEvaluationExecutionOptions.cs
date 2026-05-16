internal readonly record struct QualityEvaluationExecutionOptions(
    int? SimulationCount,
    QualitySweepOptions SweepOptions,
    double? BlackAdvantagePercent)
{
    internal bool IsSweep => SweepOptions.IsEnabled;
    internal bool UsesSimulation => SimulationCount.HasValue;
}

