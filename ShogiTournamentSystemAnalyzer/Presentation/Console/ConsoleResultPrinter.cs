/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using System.Globalization;

internal static partial class Program
{
    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データの要約表示だ。
    /// </summary>
    static void PrintTournamentQualityReportSummaryMetrics(TournamentQualityReportSummary summary)
    {
        Console.WriteLine("品質評価サマリー:");
        Console.WriteLine($"- Spearman 相関: {summary.SpearmanCorrelation.ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"- 平均順位ずれ: {summary.MeanAbsoluteRankError.ToString("F3", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"- Elo上位8名の総合上位8位残留人数（平均）: {summary.AverageTop8Retention.ToString("F3", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"- Elo1位の総合1位確率: {FormatPercent(summary.EloTop1OverallTop1Probability)}");
        Console.WriteLine($"- 最大不利益: {summary.MostPenalizedPlayerName} ({summary.MostPenalizedDelta.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture)})");
        Console.WriteLine($"- 最大利益: {summary.MostAdvantagedPlayerName} ({summary.MostAdvantagedDelta.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture)})\n");
    }

    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データの表示だ。
    /// </summary>
    static void PrintTournamentQualityReportSummary(TournamentQualityReportData tournamentQualityReportData)
    {
        PrintTournamentQualityReportSummaryMetrics(tournamentQualityReportData.Summary);
    }

    static void PrintTournamentQualityReportPlayerHighlightsRows(IReadOnlyList<TournamentQualityReportPlayerRow> playerRows)
    {
        Console.WriteLine("品質評価 選手別ハイライト:");
        Console.WriteLine("Elo順位  名前                 期待総合順位   ずれ      総合1位確率   総合上位8確率");

        foreach (var row in playerRows.Take(8))
        {
            Console.WriteLine(
                row.EloRank.ToString(CultureInfo.InvariantCulture).PadLeft(6)
                + "  " + row.Name.PadRight(20)
                + row.ExpectedOverallPlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
                + row.OverallPlaceDeltaFromEloRank.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture).PadLeft(10)
                + FormatPercent(row.OverallTop1Probability).PadLeft(14)
                + FormatPercent(row.OverallTop8Probability).PadLeft(14));
        }

        Console.WriteLine();
    }

    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データの選手別ハイライト表示だ。
    /// </summary>
    static void PrintTournamentQualityReportPlayerHighlights(TournamentQualityReportData tournamentQualityReportData)
    {
        PrintTournamentQualityReportPlayerHighlightsRows(tournamentQualityReportData.PlayerRows);
    }

    /// <summary>
    /// ［読者］域へ渡す［大会品質レポート］境界データのスイープ表示だ。
    /// </summary>
    static void PrintTournamentQualitySweepReportRows(TournamentQualitySweepReportData tournamentQualitySweepReportData)
    {
        PrintTournamentQualitySweepReportTable(tournamentQualitySweepReportData.SweepRows);
    }

    static void PrintResult(int playerCount, CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<ResultRow> resultRows)
    {
        Console.WriteLine($"計算方法: {result.Mode}\n");
        Console.WriteLine($"同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%\n");

        var nameWidth = Math.Max(6, resultRows.Max(x => x.Name.Length) + 2);
        var header = "対局者".PadRight(nameWidth)
            + "元Elo".PadLeft(10)
            + "実効Elo".PadLeft(10)
            + "差分".PadLeft(10)
            + "先手".PadLeft(8)
            + "後手".PadLeft(8)
            + "先手勝率".PadLeft(12)
            + "後手勝率".PadLeft(12)
            + "優勝確率".PadLeft(12)
            + "平均順位".PadLeft(12);

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var row in resultRows)
        {
            var line = row.Name.PadRight(nameWidth)
                + FormatRating(row.OriginalRating).PadLeft(10)
                + FormatRating(row.EffectiveRating).PadLeft(10)
                + FormatSignedRating(row.RatingDelta).PadLeft(10)
                + row.FirstPlayerCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                + row.SecondPlayerCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                + FormatOptionalPercent(row.FirstPlayerWinRate).PadLeft(12)
                + FormatOptionalPercent(row.SecondPlayerWinRate).PadLeft(12)
                + FormatPercent(row.ChampionshipProbability).PadLeft(12)
                + row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12);

            Console.WriteLine(line);
        }
    }

    static void PrintRepresentativeExecutionRanking(IReadOnlyList<RepresentativeExecutionRankRow> rows, TournamentRuleSetMode tournamentRuleSetMode)
    {
        Console.WriteLine($"代表実行順位（{TournamentRuleSetRule.GetLabel(tournamentRuleSetMode)}）:");
        var nameWidth = Math.Max(6, rows.Max(x => x.Name.Length) + 2);
        var header = "対局者".PadRight(nameWidth)
            + "勝点".PadLeft(8)
            + "順位帯".PadLeft(10)
            + "平均順位".PadLeft(12)
            + "1位確率".PadLeft(12);

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));
        foreach (var row in rows)
        {
            var line = row.Name.PadRight(nameWidth)
                + row.Points.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                + row.RankLabel.PadLeft(10)
                + row.AveragePlace.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
                + FormatPercent(row.FirstPlaceProbability).PadLeft(12);
            Console.WriteLine(line);
        }

        Console.WriteLine();
    }

    static void PrintFinalStageResult(CalculationResult result, double firstPlayerWinRatePercent, IReadOnlyList<FinalStageResultRow> resultRows)
    {
        Console.WriteLine($"計算方法: {result.Mode}\n");
        Console.WriteLine($"同Elo対局時の先手勝率: {firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)}%\n");

        var nameWidth = Math.Max(6, resultRows.Max(x => x.Name.Length) + 2);
        var header = "対局者".PadRight(nameWidth)
            + "群".PadLeft(8)
            + "元Elo".PadLeft(10)
            + "実効Elo".PadLeft(10)
            + "差分".PadLeft(10)
            + "先手".PadLeft(8)
            + "後手".PadLeft(8)
            + "群1位".PadLeft(10)
            + "群平均".PadLeft(10)
            + "総合1位".PadLeft(10)
            + "総合平均".PadLeft(10);

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var row in resultRows)
        {
            var line = row.Name.PadRight(nameWidth)
                + row.Group.PadLeft(8)
                + FormatRating(row.OriginalRating).PadLeft(10)
                + FormatRating(row.EffectiveRating).PadLeft(10)
                + FormatSignedRating(row.RatingDelta).PadLeft(10)
                + row.FirstPlayerCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                + row.SecondPlayerCount.ToString(CultureInfo.InvariantCulture).PadLeft(8)
                + FormatPercent(row.GroupPlace1Probability).PadLeft(10)
                + row.GroupPlaceAverage.ToString("F3", CultureInfo.InvariantCulture).PadLeft(10)
                + FormatPercent(row.OverallPlace1Probability).PadLeft(10)
                + row.OverallPlaceAverage.ToString("F3", CultureInfo.InvariantCulture).PadLeft(10);

            Console.WriteLine(line);
        }
    }

    static void PrintMatchesCsv(IReadOnlyList<Player> players, IReadOnlyList<Match> matches)
    {
        PrintMatchesCsv(players, matches, "生成された対局CSV:");
    }

    static void PrintMatchesCsv(IReadOnlyList<Player> players, IReadOnlyList<Match> matches, string title)
    {
        Console.WriteLine(title);
        Console.WriteLine("first,second");

        foreach (var match in matches)
        {
            Console.WriteLine($"{EscapeCsv(players[match.FirstPlayer].Name)},{EscapeCsv(players[match.SecondPlayer].Name)}");
        }

        Console.WriteLine();
    }
}

