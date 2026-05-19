using ShogiTournamentSystemAnalyzer.Infrastructure.CodingModels;
using System.Globalization;
using System.Text;

internal static partial class Program
{
    /// <summary>
    /// ［選手一覧］の CSV 形式の入力を解析して、選手のリストを作成します。
    /// </summary>
    /// <param name="lines">CSV の各行を表す文字列のリスト</param>
    /// <param name="players">解析結果として作成される選手のリスト</param>
    /// <param name="err">解析中に発生したエラーメッセージ</param>
    /// <returns>解析が成功した場合は true、失敗した場合は false</returns>
    static bool TryParsePlayers(IReadOnlyList<string> lines, out List<Player> players, out ErrorMessageModel err)
    {
        players = new List<Player>();
        err = ErrorMessageModel.Empty;

        var startIndex = 0;
        var firstColumns = SplitCsvLine(lines[0]);  // 先頭行をカンマで分割して列を取得
        if (IsHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = SplitCsvLine(lines[i]);
            // 👓　2 列以上であることを確認
            if (columns.Count < 2) { err = ErrorMessageModel.FromString($"{i + 1} 行目は 2 列以上必要です。"); return false; }

            var name = columns[0].Trim();
            // 👓　レーティングを数値として解析できることを確認
            if (string.IsNullOrWhiteSpace(name)) { err = ErrorMessageModel.FromString($"{i + 1} 行目の名前が空です。"); return false; }
            if (!TryParseDouble(columns[1], out var rating)) { err = ErrorMessageModel.FromString($"{i + 1} 行目の Elo レーティングを数値で入力してください。"); return false; }

            players.Add(new Player(name, rating));
        }

        // 👓　空でないことを確認
        if (players.Count == 0) { err = ErrorMessageModel.FromString("選手は 1 人以上必要です。"); return false; }

        // 👓　名前の重複がないことを確認
        var duplicateName = players
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateName is not null) { err = ErrorMessageModel.FromString($"選手名 '{duplicateName.Key}' が重複しています。"); return false; }

        return true;
    }

    static bool TryParseFinalStageGroups(IReadOnlyList<string> lines, out Dictionary<string, FinalStageGroup> groupMap, out ErrorMessageModel err)
    {
        groupMap = new Dictionary<string, FinalStageGroup>(StringComparer.OrdinalIgnoreCase);
        err = ErrorMessageModel.Empty;

        var startIndex = 0;
        var firstColumns = SplitCsvLine(lines[0]);
        if (IsFinalStageGroupHeaderRow(firstColumns))
        {
            startIndex = 1;
        }

        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = SplitCsvLine(lines[i]);
            // 👓　2 列以上であることを確認
            if (columns.Count < 2) { err = ErrorMessageModel.FromString($"{i + 1} 行目は 2 列以上必要です。"); return false; }

            var groupValue = columns[0].Trim();
            var name = columns[1].Trim();
            // 👓　グループも選手名も空でないことを確認
            if (string.IsNullOrWhiteSpace(groupValue) || string.IsNullOrWhiteSpace(name)) { err = ErrorMessageModel.FromString($"{i + 1} 行目のグループまたは選手名が空です。"); return false; }
            // 👓　グループが Apex または Innov であることを確認
            if (!TryParseFinalStageGroup(groupValue, out var group)) { err = ErrorMessageModel.FromString($"{i + 1} 行目のグループ '{groupValue}' は Apex または Innov で入力してください。"); return false; }
            // 👓　同じ選手名が複数のグループに割り当てられていないことを確認
            if (groupMap.ContainsKey(name)) { err = ErrorMessageModel.FromString($"選手名 '{name}' が重複しています。"); return false; }

            groupMap.Add(name, group);
        }

        // 👓　空でないことを確認
        if (groupMap.Count == 0) { err = ErrorMessageModel.FromString("グループ対応は 1 行以上必要です。"); return false; }

        return true;
    }

    static bool TryParseMatches(IReadOnlyList<string> lines, IReadOnlyList<Player> players, out List<Match> matches, out ErrorMessageModel err)
    {
        matches = new List<Match>();
        err = ErrorMessageModel.Empty;

        // 👓　空でないことを確認
        if (lines.Count == 0) { err = ErrorMessageModel.FromString("対局入力がありません。"); return false; }

        if (LooksLikeRoundMatrixInput(lines)) return TryParseMatchesFromRoundMatrix(lines, players, out matches, out err);

        var startIndex = 0;
        var firstColumns = SplitCsvLine(lines[0]);
        if (IsMatchHeaderRow(firstColumns)) { startIndex = 1; }

        var playerIndexes = players
            .Select((player, index) => new { player.Name, index })
            .ToDictionary(x => x.Name, x => x.index, StringComparer.OrdinalIgnoreCase);

        var seenPairs = new HashSet<Match>();
        for (var i = startIndex; i < lines.Count; i++)
        {
            var columns = SplitCsvLine(lines[i]);
            // 👓　2 列以上であることを確認
            if (columns.Count < 2) { err = ErrorMessageModel.FromString($"{i + 1} 行目は 2 列以上必要です。"); return false; }

            var firstPlayerName = columns[0].Trim();
            var secondPlayerName = columns[1].Trim();
            // 👓　先手と後手の名前が空でないことを確認
            if (string.IsNullOrWhiteSpace(firstPlayerName) || string.IsNullOrWhiteSpace(secondPlayerName)) { err = ErrorMessageModel.FromString($"{i + 1} 行目の先手または後手が空です。"); return false; }
            // 👓　先手と後手が選手一覧に存在することを確認
            if (!playerIndexes.TryGetValue(firstPlayerName, out var firstPlayerIndex)) { err = ErrorMessageModel.FromString($"{i + 1} 行目の先手 '{firstPlayerName}' が選手一覧にありません。"); return false; }
            if (!playerIndexes.TryGetValue(secondPlayerName, out var secondPlayerIndex)) { err = ErrorMessageModel.FromString($"{i + 1} 行目の後手 '{secondPlayerName}' が選手一覧にありません。"); return false; }
            // 👓　先手と後手が同じ選手でないことを確認
            if (firstPlayerIndex == secondPlayerIndex) { err = ErrorMessageModel.FromString($"{i + 1} 行目は同じ選手同士の対局です。"); return false; }

            var match = new Match(firstPlayerIndex, secondPlayerIndex);
            // 👓　同じ組み合わせの対局が複数回入力されていないことを確認
            if (!seenPairs.Add(match)) { err = ErrorMessageModel.FromString($"{i + 1} 行目の対局 '{firstPlayerName} vs {secondPlayerName}' が重複しています。"); return false; }

            matches.Add(match);
        }

        // 👓　空でないことを確認
        if (matches.Count == 0) { err = ErrorMessageModel.FromString("対局は 1 局以上必要です。"); return false; }

        return true;
    }

    static bool TryParseMatchesFromRoundMatrix(IReadOnlyList<string> lines, IReadOnlyList<Player> players, out List<Match> matches, out ErrorMessageModel err)
    {
        matches = new List<Match>();
        err = ErrorMessageModel.Empty;

        var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        List<string>? currentSectionLines = null;
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Equals("Round", StringComparison.OrdinalIgnoreCase)
                || line.Equals("Black/White", StringComparison.OrdinalIgnoreCase)
                || line.Equals("First/Second", StringComparison.OrdinalIgnoreCase)
                || IsPlayerAliasSectionHeader(line))
            {
                currentSectionLines = new List<string>();
                sections[line] = currentSectionLines;
                continue;
            }

            if (currentSectionLines is not null && !string.IsNullOrWhiteSpace(rawLine))
            {
                currentSectionLines.Add(rawLine);
            }
        }

        // 👓　Round セクションと First/Second（または Black/White）セクションが存在することを確認
        if (!sections.TryGetValue("Round", out var roundLines)) { err = ErrorMessageModel.FromString("Round セクションがありません。"); return false; }
        if (!sections.TryGetValue("Black/White", out var colorLines)
            && !sections.TryGetValue("First/Second", out colorLines)) { err = ErrorMessageModel.FromString("First/Second セクションがありません。"); return false; }

        // 👓　Round セクションと First/Second（または Black/White）セクションの形式が正しいことを確認
        if (!TryParseSquareMatrix(roundLines, "Round", out var roundNames, out var roundValues, out err)) return false;
        if (!TryParseSquareMatrix(colorLines, "First/Second", out var colorNames, out var colorValues, out err)) return false;
        if (roundNames.Count != colorNames.Count || !roundNames.SequenceEqual(colorNames, StringComparer.OrdinalIgnoreCase)) { err = ErrorMessageModel.FromString("Round セクションと First/Second セクションの見出しが一致していません。"); return false; }

        var resolvedNames = roundNames;
        if (sections.TryGetValue("Players", out var aliasLines)
            || sections.TryGetValue("対局記号表", out aliasLines))
        {
            // 👓　対局記号表セクションがある場合は、Round セクションの見出しを選手名に変換する
            if (!TryParsePlayerAliases(aliasLines, roundNames, out resolvedNames, out err)) return false;
        }

        var playerIndexes = players
            .Select((player, index) => new { player.Name, index })
            .ToDictionary(x => x.Name, x => x.index, StringComparer.OrdinalIgnoreCase);

        var orderedMatches = new List<(int Round, Match Match, int Order)>();
        for (var i = 0; i < roundNames.Count; i++)
        {
            // 👓　Round セクションの見出し（または対局記号表で解決された名前）が選手一覧に存在することを確認
            if (!playerIndexes.ContainsKey(resolvedNames[i])) { err = ErrorMessageModel.FromString($"対局記号表セクションの選手名 '{resolvedNames[i]}' が選手一覧にありません。"); return false; }

            for (var j = i + 1; j < roundNames.Count; j++)
            {
                var roundForward = NormalizeMatrixCell(roundValues[i, j]);
                var roundBackward = NormalizeMatrixCell(roundValues[j, i]);

                // 👓　Round セクションの両方向のセルが両方とも空でない場合は、値が一致していることを確認し、1 以上の整数であることを確認
                if (string.IsNullOrEmpty(roundForward) && string.IsNullOrEmpty(roundBackward)) continue;
                if (!string.Equals(roundForward, roundBackward, StringComparison.OrdinalIgnoreCase)) { err = ErrorMessageModel.FromString($"Round 表の '{roundNames[i]}' と '{roundNames[j]}' の値が一致していません。"); return false; }
                if (!int.TryParse(roundForward, NumberStyles.Integer, CultureInfo.InvariantCulture, out var roundNumber) || roundNumber <= 0) { err = ErrorMessageModel.FromString($"Round 表の '{roundNames[i]}' と '{roundNames[j]}' の値は 1 以上の整数で入力してください。"); return false; }

                var colorForward = NormalizeMatrixCell(colorValues[i, j]).ToLowerInvariant();
                var colorBackward = NormalizeMatrixCell(colorValues[j, i]).ToLowerInvariant();

                Match match;
                if ((colorForward == "b" || colorForward == "f") && (colorBackward == "w" || colorBackward == "s"))
                {
                    match = new Match(playerIndexes[resolvedNames[i]], playerIndexes[resolvedNames[j]]);
                }
                else if ((colorForward == "w" || colorForward == "s") && (colorBackward == "b" || colorBackward == "f"))
                {
                    match = new Match(playerIndexes[resolvedNames[j]], playerIndexes[resolvedNames[i]]);
                }
                // 👓　Round セクションの両方向のセルが両方とも空でない場合は、片方が f/s（互換で b/w も可）で、もう片方が w/b（互換で s/f も可）であることを確認
                else { err = ErrorMessageModel.FromString($"First/Second 表の '{roundNames[i]}' と '{roundNames[j]}' は f/s（互換で b/w も可）の組み合わせで入力してください。"); return false; }

                orderedMatches.Add((roundNumber, match, orderedMatches.Count));
            }
        }

        // 👓　対局が空でないことを確認
        if (orderedMatches.Count == 0) { err = ErrorMessageModel.FromString("対局は 1 局以上必要です。"); return false; }

        matches = orderedMatches
            .OrderBy(x => x.Round)
            .ThenBy(x => x.Order)
            .Select(x => x.Match)
            .ToList();

        return true;
    }

    static bool TryParsePlayerAliases(IReadOnlyList<string> lines, IReadOnlyList<string> aliases, out List<string> resolvedNames, out ErrorMessageModel err)
    {
        resolvedNames = new List<string>();
        err = ErrorMessageModel.FromString(string.Empty);

        // 👓　空でないことを確認
        if (lines.Count == 0) { err = ErrorMessageModel.FromString("対局記号表セクションの内容がありません。"); return false; }

        var aliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var columns = SplitCsvLine(line);
            // 👓　2 列以上であることを確認
            if (columns.Count < 2) { err = ErrorMessageModel.FromString("対局記号表セクションは 2 列以上で入力してください。"); return false; }

            var alias = columns[0].Trim();
            var playerName = columns[1].Trim();

            // 👓　記号も選手名も空でないことを確認
            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(playerName)) { err = ErrorMessageModel.FromString("対局記号表セクションの記号または選手名が空です。"); return false; }
            if (aliasMap.ContainsKey(alias)) { err = ErrorMessageModel.FromString($"対局記号表セクションの記号 '{alias}' が重複しています。"); return false; }

            aliasMap.Add(alias, playerName);
        }

        foreach (var alias in aliases)
        {
            // 👓　Round セクションの見出しが対局記号表セクションの記号に存在することを確認
            if (!aliasMap.TryGetValue(alias, out var playerName)) { err = ErrorMessageModel.FromString($"対局記号表セクションに記号 '{alias}' の対応表がありません。"); return false; }

            resolvedNames.Add(playerName);
        }

        return true;
    }

    static bool TryParseDouble(string? input, out double value)
    {
        return double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value)
            || double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
    }

    static bool TryParseSquareMatrix(IReadOnlyList<string> lines, string sectionName, out List<string> names, out string[,] values, out ErrorMessageModel err)
    {
        names = new List<string>();
        values = new string[0, 0];
        err = ErrorMessageModel.FromString(string.Empty);

        // 👓　空でないことを確認
        if (lines.Count < 2) { err = ErrorMessageModel.FromString($"{sectionName} セクションの行数が不足しています。"); return false; }

        var headerColumns = SplitCsvLine(lines[0]).Select(x => x.Trim()).ToList();
        // 👓　ヘッダー行が 2 列以上であることを確認
        if (headerColumns.Count < 2) { err = ErrorMessageModel.FromString($"{sectionName} セクションのヘッダーが不正です。"); return false; }

        names = headerColumns.Skip(1).ToList();
        // 👓　見出しが空でないことと重複がないことを確認
        if (names.Any(string.IsNullOrWhiteSpace) || names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count) { err = ErrorMessageModel.FromString($"{sectionName} セクションの見出しが不正です。"); return false; }

        var nameToRowIndex = names
            .Select((name, index) => new { name, index })
            .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

        values = new string[names.Count, names.Count];
        var seenRows = new bool[names.Count];

        for (var lineIndex = 1; lineIndex < lines.Count; lineIndex++)
        {
            var columns = SplitCsvLine(lines[lineIndex]);
            // 👓　各行が見出しの数 + 1 列以上であることを確認
            if (columns.Count < names.Count + 1) { err = ErrorMessageModel.FromString($"{sectionName} セクションの {lineIndex + 1} 行目の列数が不足しています。"); return false; }

            var rowName = columns[0].Trim();
            // 👓　行の見出しがヘッダーの見出しのいずれかに一致することを確認
            if (!nameToRowIndex.TryGetValue(rowName, out var rowIndex)) { err = ErrorMessageModel.FromString($"{sectionName} セクションの {lineIndex + 1} 行目の記号 '{rowName}' がヘッダーにありません。"); return false; }
            if (seenRows[rowIndex]) { err = ErrorMessageModel.FromString($"{sectionName} セクションの行 '{rowName}' が重複しています。"); return false; }

            seenRows[rowIndex] = true;
            for (var columnIndex = 0; columnIndex < names.Count; columnIndex++)
            {
                values[rowIndex, columnIndex] = columns[columnIndex + 1].Trim();
            }
        }

        // 👓　すべての行が見られていることを確認
        if (seenRows.Any(x => !x)) { err = ErrorMessageModel.FromString($"{sectionName} セクションに不足している行があります。"); return false; }

        return true;
    }

    static string NormalizeMatrixCell(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized == "-" ? string.Empty : normalized;
    }

    static List<string> SplitCsvLine(string line)
    {
        var columns = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                columns.Add(field.ToString());
                field.Clear();
                continue;
            }

            field.Append(ch);
        }

        columns.Add(field.ToString());
        return columns;
    }

    /// <summary>
    /// ［ヘッダー行］か。
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    static bool IsHeaderRow(IReadOnlyList<string> columns)
    {
        // 👓　2 列以上であることを確認
        if (columns.Count < 2) return false;

        var first = columns[0].Trim();  // 先頭列
        var second = columns[1].Trim(); // 2 列目

        // 👓　先頭列が "name" または "名前" で、2 列目が "elo", "rating", "eloRating", "eloレーティング", または "レーティング" であることを確認
        return first.Equals("name", StringComparison.OrdinalIgnoreCase)
            || first.Equals("名前", StringComparison.OrdinalIgnoreCase)
            || second.Equals("elo", StringComparison.OrdinalIgnoreCase)
            || second.Equals("rating", StringComparison.OrdinalIgnoreCase)
            || second.Equals("eloRating", StringComparison.OrdinalIgnoreCase)
            || second.Equals("eloレーティング", StringComparison.OrdinalIgnoreCase)
            || second.Equals("レーティング", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsFinalStageGroupHeaderRow(IReadOnlyList<string> columns)
    {
        //👓　先頭列が "group" または "グループ" で、2 列目が "name", "名前", "participantName", または "選手名" であることを確認
        if (columns.Count < 2) return false;

        var first = columns[0].Trim();
        var second = columns[1].Trim();

        return (first.Equals("group", StringComparison.OrdinalIgnoreCase)
                || first.Equals("グループ", StringComparison.OrdinalIgnoreCase))
            && (second.Equals("name", StringComparison.OrdinalIgnoreCase)
                || second.Equals("名前", StringComparison.OrdinalIgnoreCase)
                || second.Equals("participantName", StringComparison.OrdinalIgnoreCase)
                || second.Equals("選手名", StringComparison.OrdinalIgnoreCase));
    }

    static bool TryParseFinalStageGroup(string value, out FinalStageGroup group)
    {
        // 👓　グループが Apex または Innov であることを確認
        if (value.Equals("Apex", StringComparison.OrdinalIgnoreCase)) { group = FinalStageGroup.Apex; return true; }
        if (value.Equals("Innov", StringComparison.OrdinalIgnoreCase)) { group = FinalStageGroup.Innov; return true; }

        group = default;
        return false;
    }

    static bool IsMatchHeaderRow(IReadOnlyList<string> columns)
    {
        // 👓　先頭列が "black", "先手", "first", "white", "後手", または "second" であることを確認
        if (columns.Count < 2) { return false; }

        var first = columns[0].Trim();
        var second = columns[1].Trim();

        return first.Equals("black", StringComparison.OrdinalIgnoreCase)
            || first.Equals("黒番", StringComparison.OrdinalIgnoreCase)
            || second.Equals("white", StringComparison.OrdinalIgnoreCase)
            || second.Equals("白番", StringComparison.OrdinalIgnoreCase);
    }

    static bool LooksLikeRoundMatrixInput(IReadOnlyList<string> lines)
    {
        var firstNonEmptyLine = lines.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return firstNonEmptyLine is not null
            && firstNonEmptyLine.Trim().Equals("Round", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsPlayerAliasSectionHeader(string line)
    {
        var header = line.Trim();
        return header.Equals("Players", StringComparison.OrdinalIgnoreCase)
            || header.Equals("対局記号表", StringComparison.OrdinalIgnoreCase);
    }
}
