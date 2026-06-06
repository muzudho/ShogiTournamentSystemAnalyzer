namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System.Globalization;

internal static class FinalRankingMarkdownTemplateModelBuilderSupport
{
    internal static MarkdownTemplateModel BuildBaseModel(
        string outputMarkdownPath,
        string outputCsvPath,
        string mode,
        double firstPlayerWinRatePercent,
        int playerCount,
        string editionLabel,
        string? overviewNote,
        string? representativeRankingMarkdownPath,
        string? referenceMatchesCsvPath)
    {
        return new MarkdownTemplateModel
        {
            OutputCsvLink = MarkdownOutputHelpers.BuildMarkdownFileLink(outputMarkdownPath, outputCsvPath),
            EditionLabel = editionLabel,
            Mode = mode,
            FirstPlayerWinRatePercent = firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
            PlayerCount = playerCount,
            OverviewNote = overviewNote,
            RepresentativeRankingMarkdownLink = BuildOptionalMarkdownLink(outputMarkdownPath, representativeRankingMarkdownPath),
            ReferenceMatchesCsvLink = BuildOptionalMarkdownLink(outputMarkdownPath, referenceMatchesCsvPath),
            AttentionPoints = [],
            AutoComments = [],
            PrimaryTableHeader = string.Empty,
            PrimaryTableHeaderSeparator = string.Empty,
            PrimaryTableRows = [],
            TrailingSections = [],
            Charts = []
        };
    }

    internal static GeneralSimulationResultRow[] SelectTopRows(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, double> primaryDescending,
        Func<GeneralSimulationResultRow, double> secondaryAscending,
        int takeCount,
        Func<GeneralSimulationResultRow, string> nameSelector)
    {
        return rows
            .OrderByDescending(primaryDescending)
            .ThenBy(secondaryAscending)
            .ThenBy(nameSelector, StringComparer.OrdinalIgnoreCase)
            .Take(takeCount)
            .ToArray();
    }

    internal static bool TrySelectBestRow(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, double> primaryDescending,
        Func<GeneralSimulationResultRow, double> secondaryAscending,
        Func<GeneralSimulationResultRow, string> nameSelector,
        out GeneralSimulationResultRow bestRow)
    {
        using var enumerator = rows
            .OrderByDescending(primaryDescending)
            .ThenBy(secondaryAscending)
            .ThenBy(nameSelector, StringComparer.OrdinalIgnoreCase)
            .GetEnumerator();

        if (!enumerator.MoveNext())
        {
            bestRow = default!;
            return false;
        }

        bestRow = enumerator.Current;
        return true;
    }

    internal static GeneralSimulationResultRow[] SelectTopRowsByGroup(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, bool> predicate,
        Func<GeneralSimulationResultRow, double> primaryDescending,
        Func<GeneralSimulationResultRow, double> secondaryAscending,
        int takeCount,
        Func<GeneralSimulationResultRow, string> nameSelector)
    {
        return SelectTopRows(rows.Where(predicate), primaryDescending, secondaryAscending, takeCount, nameSelector);
    }

    internal static bool HasMetrics(IReadOnlyList<GeneralSimulationResultRow> rows, params string[] metricKeys)
    {
        return rows.Count > 0 && rows.All(row => metricKeys.All(row.Metrics.ContainsKey));
    }

    internal static double GetMetric(GeneralSimulationResultRow row, string key)
    {
        if (row.Metrics.TryGetValue(key, out var metric))
        {
            return metric.Value;
        }

        throw new InvalidOperationException($"シミュレーション結果行に必要な metric がありません: {key}");
    }

    internal static string GetFreeColumn(GeneralSimulationResultRow row, string key)
    {
        var column = row.FreeColumns.FirstOrDefault(column => string.Equals(column.Key, key, StringComparison.Ordinal));
        if (!string.IsNullOrEmpty(column.Key))
        {
            return column.DisplayValue;
        }

        throw new InvalidOperationException($"シミュレーション結果行に必要な自由形式列がありません: {key}");
    }

    internal static IEnumerable<string> BuildMarkdownTableRows(
        IEnumerable<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, string> formatter)
    {
        return rows.Select(formatter);
    }

    internal static FinalRankingMarkdownChartSpec[] BuildChartSpecs(
        IReadOnlyList<GeneralSimulationResultRow> rows,
        Func<GeneralSimulationResultRow, string> categorySelector,
        params (string Title, string YAxisLabel, string YAxisRange, Func<GeneralSimulationResultRow, string> ValueSelector)[] chartDefinitions)
    {
        if (rows.Count == 0) return [];

        return chartDefinitions
            .Select(chart => new FinalRankingMarkdownChartSpec(
                chart.Title,
                rows.Select(categorySelector),
                chart.YAxisLabel,
                chart.YAxisRange,
                rows.Select(chart.ValueSelector)))
            .ToArray();
    }

    internal static string BuildTop1Comment(double top1Probability)
    {
        var percent = top1Probability * 100;
        return percent switch
        {
            >= 30.0 => "かなり強いです。",
            >= 20.0 => "そこそこ確保されています。",
            >= 10.0 => "やや弱めです。",
            _ => "かなり弱めです。",
        };
    }

    internal static string BuildAveragePlaceComment(double averagePlace)
    {
        return averagePlace switch
        {
            <= 2.0 => "かなり前寄りです。",
            <= 3.5 => "比較的前寄りです。",
            <= 5.0 => "中位上側です。",
            _ => "まだ混戦気味です。",
        };
    }

    internal static string BuildRatingDeltaComment(double biggestBoost, double biggestDrop)
    {
        var spread = biggestBoost - biggestDrop;
        return spread switch
        {
            >= 80.0 => "割り当てや対戦構成の影響がかなり大きいです。",
            >= 40.0 => "割り当てや対戦構成の影響が見えてきます。",
            >= 15.0 => "割り当てや対戦構成の影響は比較的小さめです。",
            _ => "割り当てや対戦構成の影響はかなり小さめです。",
        };
    }

    internal static string BuildGroupLeadComment(double groupPlace1Probability, double groupPlaceAverage)
    {
        var percent = groupPlace1Probability * 100;
        if (percent >= 35.0 && groupPlaceAverage <= 2.0) return "先頭がかなりはっきりしています。";
        if (percent >= 20.0 && groupPlaceAverage <= 3.0) return "先頭候補が見えています。";
        return "まだ横並び気味です。";
    }

    internal static string BuildApexInnovGapComment(double apexTopProbability, double innovTopProbability)
    {
        var gapPercent = (apexTopProbability - innovTopProbability) * 100;
        return gapPercent switch
        {
            >= 15.0 => "Apex 側の先頭がかなり優勢です。",
            >= 5.0 => "Apex 側の先頭がやや優勢です。",
            > -5.0 => "両グループの先頭感は近めです。",
            _ => "Innov 側の先頭感もかなり強いです。",
        };
    }

    static string? BuildOptionalMarkdownLink(string markdownPath, string? targetPath)
    {
        return string.IsNullOrWhiteSpace(targetPath)
            ? null
            : MarkdownOutputHelpers.BuildMarkdownFileLink(markdownPath, targetPath);
    }
}
