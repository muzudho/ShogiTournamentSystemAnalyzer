/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries.FinalRanking.DataDefinitions;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using System.Globalization;

/// <summary>
/// ［最終順位］境界のCSV形式データファイル書き出し処理
/// </summary>
internal class FinalRankingCsvFileWriter
{


    // ========================================
    // 生成
    // ========================================


    public FinalRankingCsvFileWriter(FinalRankingDataFileWriterSettings settings)
    {
        this.Settings = settings;
    }


    // ========================================
    // 構成
    // ========================================


    #region ［最終順位という境界］の設定

    /// <summary>
    /// ［最終順位という境界］の設定
    /// </summary>
    internal FinalRankingDataFileWriterSettings Settings { get; init; }

    #endregion

    #region ［列名の一覧］

    /// <summary>
    /// ［列名の一覧］取得
    /// </summary>
    /// <returns></returns>
    protected IReadOnlyList<string> GetFinalRankingFixedColumns()
    {
        return finalRankingFixedColumns ??= LoadFixedColumns(this.Settings.GetFinalRankingTableTypeFileName());

        // ローカル関数

        /// <summary>
        /// ［固定列一覧］読込
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static IReadOnlyList<string> LoadFixedColumns(string fileName)
        {
            return TableTypeDefinitionReader.Load(fileName)
                .Data
                .Select(column => column.Name)
                .ToArray();
        }
    }
    static IReadOnlyList<string>? finalRankingFixedColumns;

    #endregion

    #region ［最終順位という境界］のCSV形式データ

    /// <summary>
    /// ［最終順位という境界］のCSV形式データを作成する。
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="overviewNote"></param>
    /// <returns></returns>
    internal IEnumerable<string> CreateResultCsvLines<TRow>(
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        string? overviewNote = null)
        where TRow : ISimulationResultRow
    {
        var generalResultRows = resultRows
            .Select(row => row.ToGeneralResultRow())
            .ToList();

        return CreateGeneralResultCsvLines(
            mode,
            firstPlayerWinRatePercent,
            generalResultRows,
            overviewNote);
    }

    /// <summary>
    /// ［最終順位という境界］のCSV形式データを、共通部分と自由形式部分に分けた行から作成する。
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="overviewNote"></param>
    /// <returns></returns>
    internal IEnumerable<string> CreateGeneralResultCsvLines(
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<GeneralSimulationResultRow> resultRows,
        string? overviewNote = null)
    {
        var fixedColumns = GetFinalRankingFixedColumns().ToList();
        if (string.IsNullOrWhiteSpace(overviewNote)) fixedColumns.Remove("note");

        var specificHeaderColumns = fixedColumns.ToList();
        if (resultRows.Count > 0)
        {
            AppendPlaceHeaderColumns(specificHeaderColumns, resultRows[0]);
        }

        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(specificHeaderColumns).Select(CsvOutputHelpers.EscapeCsv))
        };

        foreach (var row in resultRows)
        {
            var specificColumns = BuildGeneralFinalRankingSpecificColumns(fixedColumns, mode, firstPlayerWinRatePercent, row, overviewNote);
            var columns = CsvSchemaCommonColumns.BuildRowColumns(
                boundaryName: "FinalRanking",
                schemaName: this.Settings.GetSchemaName(),
                rowType: "data",
                specificColumns.ToArray());

            lines.Add(string.Join(",", columns.Select(CsvOutputHelpers.EscapeCsv)));
        }

        return lines;

        static void AppendPlaceHeaderColumns(List<string> specificHeaderColumns, GeneralSimulationResultRow row)
        {
            for (var place = 0; place < row.CommonData.PlaceProbabilities.Length; place++)
            {
                specificHeaderColumns.Add($"place{place + 1}Percent");
                if (row.CommonData.PlaceCounts is not null)
                {
                    specificHeaderColumns.Add($"place{place + 1}Count");
                }
            }
        }

        static List<string> BuildGeneralFinalRankingSpecificColumns(
            IReadOnlyList<string> fixedColumns,
            string mode,
            double firstPlayerWinRatePercent,
            GeneralSimulationResultRow row,
            string? overviewNote)
        {
            var specificColumns = new List<string>(fixedColumns.Count + row.CommonData.PlaceProbabilities.Length);
            var freeColumns = row.FreeColumns.ToDictionary(column => column.Key, StringComparer.Ordinal);

            for (var columnIndex = 0; columnIndex < fixedColumns.Count; columnIndex++)
            {
                var columnName = fixedColumns[columnIndex];
                specificColumns.Add(GetFixedColumnValue(columnIndex, columnName, mode, firstPlayerWinRatePercent, row, freeColumns, overviewNote));
            }

            AppendPlaceColumns(specificColumns, row);
            return specificColumns;
        }

        static string GetFixedColumnValue(
            int columnIndex,
            string columnName,
            string mode,
            double firstPlayerWinRatePercent,
            GeneralSimulationResultRow row,
            IReadOnlyDictionary<string, SimulationResultFreeColumn> freeColumns,
            string? overviewNote)
        {
            var commonData = row.CommonData;
            return columnName switch
            {
                "calculationMode" => mode,
                "sameEloFirstPlayerWinRatePercent" => firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture),
                "playerName" => commonData.Name,
                "originalElo" => SimulationRatingMath.FormatRating(commonData.OriginalRating),
                "effectiveElo" => SimulationRatingMath.FormatRating(commonData.EffectiveRating),
                "eloDelta" => SimulationRatingMath.FormatSignedRating(commonData.RatingDelta),
                "firstPlayerCount" => commonData.FirstPlayerCount.ToString(CultureInfo.InvariantCulture),
                "secondPlayerCount" => commonData.SecondPlayerCount.ToString(CultureInfo.InvariantCulture),
                "firstPlayerWinRatePercent" => SimulationRatingMath.FormatOptionalPercentValue(commonData.FirstPlayerWinRate),
                "secondPlayerWinRatePercent" => SimulationRatingMath.FormatOptionalPercentValue(commonData.SecondPlayerWinRate),
                "note" => overviewNote ?? string.Empty,
                _ when freeColumns.TryGetValue(columnName, out var freeColumn) => freeColumn.CsvValue,
                _ => throw new InvalidOperationException($"最終順位CSVの列に対応する値がありません: {columnName}")
            };
        }

        static void AppendPlaceColumns(List<string> specificColumns, GeneralSimulationResultRow row)
        {
            for (var place = 0; place < row.CommonData.PlaceProbabilities.Length; place++)
            {
                specificColumns.Add((row.CommonData.PlaceProbabilities[place] * 100).ToString("F2", CultureInfo.InvariantCulture));
                if (row.CommonData.PlaceCounts is not null)
                {
                    specificColumns.Add(row.CommonData.PlaceCounts[place].ToString("F3", CultureInfo.InvariantCulture));
                }
            }
        }
    }


    #endregion

}
