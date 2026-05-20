namespace ShogiTournamentSystemAnalyzer.Infrastructure.Csv;

using System;
using System.Collections.Generic;
using System.Text;

internal static class WriterHelper
{
    internal static void WriteText(
        string outputPath,
        Func<IEnumerable<string>> getLines)
    {
        var directoryPath = Path.GetDirectoryName(outputPath);

        // 出力先のディレクトリが存在しない場合は作成する。
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 内容を取得する。
        var lines = getLines();

        // ファイルに書き込む。
        File.WriteAllLines(outputPath, lines, new UTF8Encoding(false));
    }
}
