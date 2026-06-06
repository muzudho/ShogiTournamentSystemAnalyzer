namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using System.Globalization;
using static FinalRankingMarkdownTemplateModelBuilderSupport;

internal static class StandardFinalRankingMarkdownTemplateModelBuilder
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
        var topChampionshipRows = SelectTopRows(resultRows, row => GetMetric(row, "championshipProbability"), row => GetMetric(row, "averagePlace"), takeCount: 8, row => row.CommonData.Name);
        var hasBestChampionshipRow = TrySelectBestRow(resultRows, row => GetMetric(row, "championshipProbability"), row => GetMetric(row, "averagePlace"), row => row.CommonData.Name, out var bestChampionshipRow);
        var hasBestAveragePlaceRow = TrySelectBestRow(resultRows, row => -GetMetric(row, "averagePlace"), row => -GetMetric(row, "championshipProbability"), row => row.CommonData.Name, out var bestAveragePlaceRow);
        var hasBiggestBoostRow = TrySelectBestRow(resultRows, row => row.CommonData.RatingDelta, row => 0, row => row.CommonData.Name, out var biggestBoostRow);
        var hasBiggestDropRow = TrySelectBestRow(resultRows, row => -row.CommonData.RatingDelta, row => 0, row => row.CommonData.Name, out var biggestDropRow);

        return BuildBaseModel(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            resultRows.Count,
            editionLabel: "標準版",
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath) with
        {
            AttentionPoints =
            [
                $"優勝確率が最も高い選手: **{(hasBestChampionshipRow ? bestChampionshipRow.CommonData.Name : "該当なし")}**（{((hasBestChampionshipRow ? GetMetric(bestChampionshipRow, "championshipProbability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"平均順位が最も良い選手: **{(hasBestAveragePlaceRow ? bestAveragePlaceRow.CommonData.Name : "該当なし")}**（{(hasBestAveragePlaceRow ? GetMetric(bestAveragePlaceRow, "averagePlace") : 0).ToString("F3", CultureInfo.InvariantCulture)}）",
                $"実効Elo差分が最も大きくプラスの選手: **{(hasBiggestBoostRow ? biggestBoostRow.CommonData.Name : "該当なし")}**（{SimulationRatingMath.FormatSignedRating(hasBiggestBoostRow ? biggestBoostRow.CommonData.RatingDelta : 0)}）",
                $"実効Elo差分が最も大きくマイナスの選手: **{(hasBiggestDropRow ? biggestDropRow.CommonData.Name : "該当なし")}**（{SimulationRatingMath.FormatSignedRating(hasBiggestDropRow ? biggestDropRow.CommonData.RatingDelta : 0)}）"
            ],
            AutoComments =
            [
                $"優勝候補の強さ: {BuildTop1Comment(hasBestChampionshipRow ? GetMetric(bestChampionshipRow, "championshipProbability") : 0)}",
                $"先頭の平均順位: {BuildAveragePlaceComment(hasBestAveragePlaceRow ? GetMetric(bestAveragePlaceRow, "averagePlace") : 0)}",
                $"実効Eloの押し上げ: {BuildRatingDeltaComment(hasBiggestBoostRow ? biggestBoostRow.CommonData.RatingDelta : 0, hasBiggestDropRow ? biggestDropRow.CommonData.RatingDelta : 0)}"
            ],
            PrimaryTableHeader = "| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |",
            PrimaryTableHeaderSeparator = "| --- | ---: | ---: | ---: | ---: | ---: |",
            PrimaryTableRows = BuildMarkdownTableRows(topChampionshipRows, row =>
                $"| {row.CommonData.Name} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.CommonData.RatingDelta)} | {(GetMetric(row, "championshipProbability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "averagePlace").ToString("F3", CultureInfo.InvariantCulture)} |"),
            TrailingSections = [],
            Charts = BuildChartSpecs(topChampionshipRows,
                row => row.CommonData.Name,
                ("上位候補の優勝確率", "優勝確率(%)", "0 --> 100", row => (GetMetric(row, "championshipProbability") * 100).ToString("F2", CultureInfo.InvariantCulture)),
                ("上位候補の平均順位", "平均順位", "1 --> " + Math.Max(2, resultRows.Count).ToString(CultureInfo.InvariantCulture), row => GetMetric(row, "averagePlace").ToString("F3", CultureInfo.InvariantCulture)))
        };
    }
}
