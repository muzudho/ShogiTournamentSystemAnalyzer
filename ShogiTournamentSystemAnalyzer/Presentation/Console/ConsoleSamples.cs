/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static class ConsoleSamplePrinter
{
    internal static void PrintPlayersCsvExample()
    {
        Console.WriteLine("例:");
        Console.WriteLine("name,elo");
        Console.WriteLine("Alice,1500");
        Console.WriteLine("Bob,1480\n");
    }

    internal static void PrintOptionalPlayersCsvExample()
    {
        Console.WriteLine("例（省略可）:");
        Console.WriteLine("name,elo");
        Console.WriteLine("Eve,4920");
        Console.WriteLine("Frank,4900");
        Console.WriteLine("空のまま Enter でも省略できます。\n");
    }

    internal static void PrintFinalStageGroupCsvExample()
    {
        Console.WriteLine("例:");
        Console.WriteLine("group,name");
        Console.WriteLine("Apex,Alice");
        Console.WriteLine("Apex,Bob");
        Console.WriteLine("Innov,Carol");
        Console.WriteLine("Innov,Dave\n");
    }

    internal static void PrintMatchesCsvExample()
    {
        Console.WriteLine("例:");
        Console.WriteLine("first,second");
        Console.WriteLine("Alice,Bob");
        Console.WriteLine("Carol,Alice");
        Console.WriteLine("END\n");
    }

    internal static void PrintRoundMatrixExample()
    {
        Console.WriteLine("Round/First-Second/対局記号表 の例:");
        Console.WriteLine("Round");
        Console.WriteLine(" , A, B");
        Console.WriteLine("A, -, 1");
        Console.WriteLine("B, 1, -");
        Console.WriteLine();
        Console.WriteLine("First/Second");
        Console.WriteLine(" , A, B");
        Console.WriteLine("A, -, f");
        Console.WriteLine("B, s, -");
        Console.WriteLine();
        Console.WriteLine("対局記号表");
        Console.WriteLine("A, \"Alice\"");
        Console.WriteLine("B, \"Bob\"");
        Console.WriteLine("END\n");
    }

    internal static void PrintSimulationStandardOverview()
    {
        Console.WriteLine("このモードでは次を順に入力します。");
        Console.WriteLine("1. 順位ルールセット");
        Console.WriteLine("2. 同Elo対局時の先手勝率 (%)");
        Console.WriteLine("3. 選手 / Player 一覧CSV");
        Console.WriteLine("4. 対局CSV または Round/Black-White/対局記号表");
        Console.WriteLine("5. 必要に応じて試行回数");
        Console.WriteLine("6. 結果CSVの出力先\n");
    }

    internal static void PrintSimulationFinalStageOverview()
    {
        Console.WriteLine("このモードでは次を順に入力します。");
        Console.WriteLine("1. 同Elo対局時の先手勝率 (%)");
        Console.WriteLine("2. 選手 / Player 一覧CSV");
        Console.WriteLine("3. グループ対応CSV");
        Console.WriteLine("4. 本戦不出場Apex一覧CSV（省略可）");
        Console.WriteLine("5. 本戦不出場Apexの扱い");
        Console.WriteLine("6. 境界救済戦の有無");
        Console.WriteLine("7. 対局CSV または Round/Black-White/対局記号表");
        Console.WriteLine("8. 参考対局CSV（省略可）");
        Console.WriteLine("9. 必要に応じて試行回数");
        Console.WriteLine("10. 結果CSVの出力先\n");
    }

    internal static void PrintSimulationTournamentFrameworkOverview()
    {
        Console.WriteLine("このモードでは次を順に入力します。");
        Console.WriteLine("1. 同Elo対局時の先手勝率 (%)");
        Console.WriteLine("2. 順位ルールセット");
        Console.WriteLine("3. 選手 / Player 一覧CSVのファイルパス");
        Console.WriteLine("4. ステージ一覧CSVのファイルパス");
        Console.WriteLine("5. 大会対局記録CSVのファイルパス");
        Console.WriteLine("6. 大会ルールDSLファイルのパス（省略可）");
        Console.WriteLine("7. 乱数シード（省略可）");
        Console.WriteLine("8. 必要に応じて試行回数");
        Console.WriteLine("9. 結果CSVの出力先（省略可）\n");
    }

    internal static void PrintSimulationEmptyOverview()
    {
        Console.WriteLine("このモードでは対局入力を行いません。");
        Console.WriteLine("1. 結果CSVの出力先だけを入力します。\n");
    }

    internal static void PrintQualityEvaluationStandardOverview()
    {
        Console.WriteLine("このモードでは次を順に入力します。");
        Console.WriteLine("1. 順位ルールセット");
        Console.WriteLine("2. 同Elo対局時の先手勝率 (%)");
        Console.WriteLine("3. 選手 / Player 一覧CSV");
        Console.WriteLine("4. 対局CSV または Round/Black-White/対局記号表");
        Console.WriteLine("5. 単発評価または n% スイープ実験");
        Console.WriteLine("6. 必要に応じて試行回数");
        Console.WriteLine("7. レポート出力先\n");
    }

    internal static void PrintQualityEvaluationFinalStageOverview()
    {
        Console.WriteLine("このモードでは次を順に入力します。");
        Console.WriteLine("1. 同Elo対局時の先手勝率 (%)");
        Console.WriteLine("2. 選手 / Player 一覧CSV");
        Console.WriteLine("3. グループ対応CSV");
        Console.WriteLine("4. 本戦不出場Apex一覧CSV（省略可）");
        Console.WriteLine("5. 本戦不出場Apexの扱い");
        Console.WriteLine("6. 境界救済戦の有無");
        Console.WriteLine("7. 可変定員8ルールの有無");
        Console.WriteLine("8. 対局CSV または Round/Black-White/対局記号表");
        Console.WriteLine("9. 参考対局CSV（省略可）");
        Console.WriteLine("10. 単発評価または n% スイープ実験");
        Console.WriteLine("11. 必要に応じて試行回数");
        Console.WriteLine("12. レポート出力先\n");
    }
}

