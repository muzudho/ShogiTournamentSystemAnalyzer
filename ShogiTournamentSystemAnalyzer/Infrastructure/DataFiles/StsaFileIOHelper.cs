namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;

using System.Text;

/// <summary>
/// ファイル入出力のヘルパー
/// </summary>
internal static class StsaFileIOHelper
{
    /// <summary>
    /// ファイル書き出し
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="lines"></param>
    internal static void Write(string label, string outputPath, IEnumerable<string> lines)
    {
        var directoryPath = Path.GetDirectoryName(outputPath);

        // ディレクトリが存在しない場合は作成する。
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // （BOM無しUTF-8エンコーディングで）ファイルに書き込む。
        File.WriteAllLines(outputPath, lines, new UTF8Encoding(false));

        Console.WriteLine($"{label}を作成しました: {outputPath}");
    }


    internal static string[] ReadAllLines(string label, string inputFilePath)
    {
        var fullPath = Path.GetFullPath(inputFilePath);
        if (!File.Exists(fullPath)) throw new OperationCanceledException($"{label}が見つかりません: {fullPath}");

        return File.ReadAllLines(fullPath);
    }
}
