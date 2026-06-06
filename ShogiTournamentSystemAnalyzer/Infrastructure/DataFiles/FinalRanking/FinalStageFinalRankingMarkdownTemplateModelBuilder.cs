namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using System.Globalization;
using static FinalRankingMarkdownTemplateModelBuilderSupport;

internal static class FinalStageFinalRankingMarkdownTemplateModelBuilder
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
        var apexRows = SelectTopRowsByGroup(resultRows, row => string.Equals(GetFreeColumn(row, "group"), "Apex", StringComparison.OrdinalIgnoreCase), row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), takeCount: 4, row => row.CommonData.Name);
        var innovRows = SelectTopRowsByGroup(resultRows, row => string.Equals(GetFreeColumn(row, "group"), "Innov", StringComparison.OrdinalIgnoreCase), row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), takeCount: 4, row => row.CommonData.Name);
        var hasBestOverallRow = TrySelectBestRow(resultRows, row => GetMetric(row, "overallPlace1Probability"), row => GetMetric(row, "overallPlaceAverage"), row => row.CommonData.Name, out var bestOverallRow);
        var hasBestApexRow = TrySelectBestRow(apexRows, row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), row => row.CommonData.Name, out var bestApexRow);
        var hasBestInnovRow = TrySelectBestRow(innovRows, row => GetMetric(row, "groupPlace1Probability"), row => GetMetric(row, "groupPlaceAverage"), row => row.CommonData.Name, out var bestInnovRow);

        var primaryRows = SelectTopRows(resultRows, row => GetMetric(row, "overallPlace1Probability"), row => GetMetric(row, "overallPlaceAverage"), takeCount: 8, row => row.CommonData.Name);

        var trailingSections = new List<MarkdownTemplateSection>();
        if (apexRows.Length > 0)
        {
            trailingSections.Add(new MarkdownTemplateSection(
                Title: "## Apex 注目候補",
                TableHeader: "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |\n| --- | ---: | ---: | ---: | ---: | ---: |",
                Rows: BuildMarkdownTableRows(apexRows, row =>
                    $"| {row.CommonData.Name} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {(GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "groupPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} | {GetMetric(row, "overallPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} |")));
        }

        if (innovRows.Length > 0)
        {
            trailingSections.Add(new MarkdownTemplateSection(
                Title: "## Innov 注目候補",
                TableHeader: "| 選手 | 元Elo | 実効Elo | グループ1位確率 | グループ平均順位 | 総合平均順位 |\n| --- | ---: | ---: | ---: | ---: | ---: |",
                Rows: BuildMarkdownTableRows(innovRows, row =>
                    $"| {row.CommonData.Name} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {(GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "groupPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} | {GetMetric(row, "overallPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} |")));
        }

        return BuildBaseModel(
            outputMarkdownPath,
            outputCsvPath,
            mode,
            firstPlayerWinRatePercent,
            resultRows.Count,
            editionLabel: "本戦版",
            overviewNote,
            representativeRankingMarkdownPath,
            referenceMatchesCsvPath) with
        {
            AttentionPoints =
            [
                $"総合1位確率が最も高い選手: **{(hasBestOverallRow ? bestOverallRow.CommonData.Name : "該当なし")}**（{((hasBestOverallRow ? GetMetric(bestOverallRow, "overallPlace1Probability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"Apex で最も有力な選手: **{(hasBestApexRow ? bestApexRow.CommonData.Name : "該当なし")}**（グループ1位確率 {((hasBestApexRow ? GetMetric(bestApexRow, "groupPlace1Probability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）",
                $"Innov で最も有力な選手: **{(hasBestInnovRow ? bestInnovRow.CommonData.Name : "該当なし")}**（グループ1位確率 {((hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlace1Probability") : 0) * 100).ToString("F2", CultureInfo.InvariantCulture)}%）"
            ],
            AutoComments =
            [
                $"総合1位候補の強さ: {BuildTop1Comment(hasBestOverallRow ? GetMetric(bestOverallRow, "overallPlace1Probability") : 0)}",
                $"Apex の先頭感: {BuildGroupLeadComment(hasBestApexRow ? GetMetric(bestApexRow, "groupPlace1Probability") : 0, hasBestApexRow ? GetMetric(bestApexRow, "groupPlaceAverage") : 0)}",
                $"Innov の先頭感: {BuildGroupLeadComment(hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlace1Probability") : 0, hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlaceAverage") : 0)}",
                $"Apex / Innov の先頭差: {BuildApexInnovGapComment(hasBestApexRow ? GetMetric(bestApexRow, "groupPlace1Probability") : 0, hasBestInnovRow ? GetMetric(bestInnovRow, "groupPlace1Probability") : 0)}"
            ],
            PrimaryTableHeader = "| 選手 | グループ | 元Elo | 実効Elo | 差分 | グループ1位確率 | 総合1位確率 | 総合平均順位 |",
            PrimaryTableHeaderSeparator = "| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |",
            PrimaryTableRows = BuildMarkdownTableRows(primaryRows, row =>
                $"| {row.CommonData.Name} | {GetFreeColumn(row, "group")} | {SimulationRatingMath.FormatRating(row.CommonData.OriginalRating)} | {SimulationRatingMath.FormatRating(row.CommonData.EffectiveRating)} | {SimulationRatingMath.FormatSignedRating(row.CommonData.RatingDelta)} | {(GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {(GetMetric(row, "overallPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)}% | {GetMetric(row, "overallPlaceAverage").ToString("F3", CultureInfo.InvariantCulture)} |"),
            TrailingSections = trailingSections.ToArray(),
            Charts = BuildChartSpecs(primaryRows,
                row => row.CommonData.Name,
                ("上位候補の総合1位確率", "総合1位確率(%)", "0 --> 100", row => (GetMetric(row, "overallPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)),
                ("上位候補のグループ1位確率", "グループ1位確率(%)", "0 --> 100", row => (GetMetric(row, "groupPlace1Probability") * 100).ToString("F2", CultureInfo.InvariantCulture)))
        };
    }
}
