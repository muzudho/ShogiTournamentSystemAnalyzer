/*
 * ［分析　＞　境界　＞　大会最終状態　＞　出力パス］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;

internal static partial class ReportOutputPathBuilder
{
    internal static string BuildTournamentFinalStateDefaultOutputPath(string fileName)
    {
        return BuildOutputFilePath("Simulation", "TournamentFinalState", fileName);
    }
}