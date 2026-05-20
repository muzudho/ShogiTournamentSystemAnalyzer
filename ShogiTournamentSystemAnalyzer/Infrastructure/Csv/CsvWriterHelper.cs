namespace ShogiTournamentSystemAnalyzer.Infrastructure.Csv;

using System;
using System.Collections.Generic;
using System.Text;

internal static class CsvWriterHelper
{
    static void WriteCsv(
        string outputCsvPath,
        Func<IEnumerable<string>> getCsvLines)
    {
        var directoryPath = Path.GetDirectoryName(outputCsvPath);

        // 出力先のディレクトリが存在しない場合は作成する。
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // CSVの内容を取得する。
        var lines = getCsvLines();

        // CSVファイルに書き込む。
        File.WriteAllLines(outputCsvPath, lines, new UTF8Encoding(false));
    }
}
