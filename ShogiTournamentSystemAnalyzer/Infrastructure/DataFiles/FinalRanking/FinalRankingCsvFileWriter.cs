/*
 * ［インフラストラクチャー　＞　データファイル　＞　最終順位という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

using ShogiTournamentSystemAnalyzer.Application.DataDefinitions;
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
    /// <param name="outputCsvPath">インターフェースを合わせるためのダミー</param>
    /// <param name="mode"></param>
    /// <param name="firstPlayerWinRatePercent"></param>
    /// <param name="resultRows"></param>
    /// <param name="overviewNote"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal IEnumerable<string> CreateResultCsvLines<TRow>(
        string mode,
        double firstPlayerWinRatePercent,
        IReadOnlyList<TRow> resultRows,
        string? overviewNote = null)
        where TRow : ISimulationResultRow
    {
        var specificHeaderColumns = BuildFinalRankingSpecificHeaderColumns(GetFinalRankingFixedColumns(), resultRows, overviewNote);
        var lines = new List<string>
        {
            string.Join(",", CsvSchemaCommonColumns.BuildHeaderColumns(specificHeaderColumns).Select(CsvOutputHelpers.EscapeCsv))
        };

        foreach (var row in resultRows)
        {
            var specificColumns = BuildFinalRankingSpecificColumns(mode, firstPlayerWinRatePercent, row, overviewNote);
            var columns = CsvSchemaCommonColumns.BuildRowColumns(
                boundaryName: "FinalRanking",
                schemaName: this.Settings.GetSchemaName(),
                rowType: "data",
                specificColumns.ToArray());

            lines.Add(string.Join(",", columns.Select(CsvOutputHelpers.EscapeCsv)));
        }

        return lines;

        // ローカル関数

        /// <summary>
        /// ヘッダー行の各列
        /// </summary>
        /// <typeparam name="THeaderRow"></typeparam>
        /// <param name="fixedColumns"></param>
        /// <param name="resultRows"></param>
        /// <param name="overviewNote"></param>
        /// <returns></returns>
        static List<string> BuildFinalRankingSpecificHeaderColumns<THeaderRow>(
            IReadOnlyList<string> fixedColumns,
            IReadOnlyList<THeaderRow> resultRows,
            string? overviewNote)
            where THeaderRow : ISimulationResultRow
        {
            var specificHeaderColumns = fixedColumns.ToList();
            if (string.IsNullOrWhiteSpace(overviewNote)) specificHeaderColumns.Remove("note");

            if (resultRows.Count > 0)
            {
                AppendPlaceHeaderColumns(specificHeaderColumns, resultRows[0]);
            }

            return specificHeaderColumns;

            // ローカル関数

            static void AppendPlaceHeaderColumns(List<string> specificHeaderColumns, ISimulationResultRow row)
            {
                for (var place = 0; place < row.PlaceProbabilities.Length; place++)
                {
                    specificHeaderColumns.Add($"place{place + 1}Percent");
                    if (row.PlaceCounts is not null)
                    {
                        specificHeaderColumns.Add($"place{place + 1}Count");
                    }
                }
            }
        }

        /// <summary>
        /// データ行の各列
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="firstPlayerWinRatePercent"></param>
        /// <param name="row"></param>
        /// <param name="overviewNote"></param>
        /// <returns></returns>
        static List<string> BuildFinalRankingSpecificColumns(
            string mode,
            double firstPlayerWinRatePercent,
            ISimulationResultRow row,
            string? overviewNote)
        {
            var specificColumns = new List<string>
            {
                mode,
                firstPlayerWinRatePercent.ToString("F2", CultureInfo.InvariantCulture)
            };

            specificColumns.AddRange(row.GetFinalRankingCsvSpecificColumns());

            if (!string.IsNullOrWhiteSpace(overviewNote))
            {
                specificColumns.Add(overviewNote);
            }

            AppendPlaceColumns(specificColumns, row);
            return specificColumns;

            // ローカル関数

            static void AppendPlaceColumns(List<string> specificColumns, ISimulationResultRow row)
            {
                for (var place = 0; place < row.PlaceProbabilities.Length; place++)
                {
                    specificColumns.Add((row.PlaceProbabilities[place] * 100).ToString("F2", CultureInfo.InvariantCulture));
                    if (row.PlaceCounts is not null)
                    {
                        specificColumns.Add(row.PlaceCounts[place].ToString("F3", CultureInfo.InvariantCulture));
                    }
                }
            }
        }
    }

    #endregion

}
