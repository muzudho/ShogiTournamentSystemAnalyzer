/*
 * ［分析　＞　境界　＞　最終順位　＞　出力パス］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;

internal static partial class ReportOutputPathBuilder
{
    internal static string BuildFinalRankingDefaultOutputPath(string fileName)
    {
        return BuildOutputFilePath("Ranking", "FinalRanking", fileName);
    }
}
