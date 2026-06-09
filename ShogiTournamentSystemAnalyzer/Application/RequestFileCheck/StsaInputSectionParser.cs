/*
 * ［アプリケーション　＞　入力　＞　STSA入力セクションパーサー］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

internal static class StsaInputSectionParser
{
    internal static Dictionary<string, List<string>> ParseStsaInputSections(IReadOnlyList<string> rawLines, string fullPath, string formatName)
    {
        var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        List<string>? currentLines = null;
        string? currentSectionName = null;
        var formatFound = false;

        foreach (var rawLine in rawLines)
        {
            var trimmed = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            if (trimmed.Equals($"#[Format] {formatName}", StringComparison.OrdinalIgnoreCase))
            {
                formatFound = true;
                continue;
            }

            if (trimmed.StartsWith("#[Section]", StringComparison.OrdinalIgnoreCase))
            {
                if (currentLines is not null) throw new OperationCanceledException($"{formatName} のセクション '{currentSectionName}' が #[EndSection] で閉じられていません: {fullPath}");

                var sectionName = trimmed[11..].Trim();
                if (string.IsNullOrWhiteSpace(sectionName)) throw new OperationCanceledException($"{formatName} の #[Section] にセクション名がありません: {fullPath}");

                if (sections.ContainsKey(sectionName)) throw new OperationCanceledException($"{formatName} のセクション '{sectionName}' が重複しています: {fullPath}");

                currentSectionName = sectionName;
                currentLines = new List<string>();
                continue;
            }

            if (trimmed.Equals("#[EndSection]", StringComparison.OrdinalIgnoreCase))
            {
                if (currentLines is null || currentSectionName is null) throw new OperationCanceledException($"{formatName} の #[EndSection] に対応する #[Section] がありません: {fullPath}");

                sections[currentSectionName] = currentLines;
                currentLines = null;
                currentSectionName = null;
                continue;
            }

            if (trimmed.StartsWith('#')) continue;

            if (currentLines is null) throw new OperationCanceledException($"{formatName} の制御タグ外に本文があります: {rawLine}");

            currentLines.Add(rawLine);
        }

        if (!formatFound) throw new OperationCanceledException($"{formatName} の #[Format] 宣言が見つかりません: {fullPath}");

        if (currentLines is not null) throw new OperationCanceledException($"{formatName} のセクション '{currentSectionName}' が #[EndSection] で閉じられていません: {fullPath}");

        return sections;
    }

    internal static Dictionary<string, string> ParseSectionKeyValues(IReadOnlyList<string> lines, string sectionName, string fullPath, string formatName)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0) throw new OperationCanceledException($"{formatName} の {sectionName} セクションで key=value 形式ではない行があります: {line} ({fullPath})");

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            if (values.ContainsKey(key)) throw new OperationCanceledException($"{formatName} の {sectionName} セクションでキー '{key}' が重複しています: {fullPath}");

            values[key] = value;
        }

        return values;
    }

    internal static IReadOnlyList<string> GetRequiredSectionLines(Dictionary<string, List<string>> sections, string sectionName, string fullPath, string formatName)
    {
        if (!sections.TryGetValue(sectionName, out var lines)) throw new OperationCanceledException($"{formatName} の必須セクション '{sectionName}' がありません: {fullPath}");

        return lines;
    }

    internal static IReadOnlyList<string> GetOptionalSectionLines(Dictionary<string, List<string>> sections, string sectionName)
    {
        return sections.TryGetValue(sectionName, out var lines)
            ? lines
            : Array.Empty<string>();
    }

    internal static string GetRequiredMetaValue(Dictionary<string, string> meta, string key, string fullPath, string formatName)
    {
        if (!meta.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) throw new OperationCanceledException($"{formatName} の Meta セクションに必須キー '{key}' がありません: {fullPath}");

        return value;
    }

    internal static string? GetOptionalMetaValue(Dictionary<string, string> meta, string key)
    {
        return meta.TryGetValue(key, out var value)
            ? value
            : null;
    }

}
