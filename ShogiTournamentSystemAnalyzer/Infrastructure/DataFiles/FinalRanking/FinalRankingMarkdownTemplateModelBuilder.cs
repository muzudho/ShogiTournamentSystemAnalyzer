namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal static class FinalRankingMarkdownTemplateModelBuilder
{
    internal static MarkdownTemplateModel Build(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        if (FinalRankingMarkdownTemplateModelBuilderSupport.HasMetrics(resultRows, "championshipProbability", "averagePlace"))
        {
            return StandardFinalRankingMarkdownTemplateModelBuilder.Build(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                resultRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath);
        }

        if (FinalRankingMarkdownTemplateModelBuilderSupport.HasMetrics(resultRows, "groupPlace1Probability", "groupPlaceAverage", "overallPlace1Probability", "overallPlaceAverage"))
        {
            return FinalStageFinalRankingMarkdownTemplateModelBuilder.Build(
                outputMarkdownPath,
                outputCsvPath,
                mode,
                firstPlayerWinRatePercent,
                resultRows,
                overviewNote,
                representativeRankingMarkdownPath,
                referenceMatchesCsvPath);
        }

        throw new InvalidOperationException("Markdown出力に必要な metric の組み合わせが見つかりません。");
    }
}
