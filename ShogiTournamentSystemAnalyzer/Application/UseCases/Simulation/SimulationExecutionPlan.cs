namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

internal static partial class Program
{
    readonly record struct SimulationExecutionPlan(
        RuleProfileMode RuleProfileMode,
        string ExecutionLabel,
        Action Execute);
}
