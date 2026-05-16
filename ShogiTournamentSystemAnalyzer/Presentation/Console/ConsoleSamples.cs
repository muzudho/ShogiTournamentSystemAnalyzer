internal static partial class Program
{
    static void PrintInputSample()
    {
        Console.WriteLine("入力形式:");
        Console.WriteLine("1. 黒番有利率 (%)");
        Console.WriteLine("2. 選手一覧CSV (1列目=名前, 2列目=Elo レーティング)");
        Console.WriteLine("3. 対局CSV (1列目=黒番, 2列目=白番) または Round/Black-White/対局記号表");
        Console.WriteLine("選手一覧CSVは空行で入力終了、対局入力は END 行で入力終了です。\n");
        Console.WriteLine("入力サンプル:");
        Console.WriteLine("黒番有利率(%): 51\n");
        Console.WriteLine("選手一覧CSV:");
        Console.WriteLine("name,elo");
        Console.WriteLine("Alice,1500");
        Console.WriteLine("Bob,1650");
        Console.WriteLine("Carol,1420");
        Console.WriteLine("Dave,1800\n");
        Console.WriteLine("対局CSV:");
        Console.WriteLine("black,white");
        Console.WriteLine("Alice,Bob");
        Console.WriteLine("Carol,Alice");
        Console.WriteLine("Dave,Alice");
        Console.WriteLine("Bob,Carol");
        Console.WriteLine("Bob,Dave");
        Console.WriteLine("Dave,Carol");
        Console.WriteLine("END\n");
        Console.WriteLine("Round/Black-White 表の例:");
        Console.WriteLine("Round");
        Console.WriteLine(" , A, B, C, D");
        Console.WriteLine("A, -, 3, 2, 1");
        Console.WriteLine("B, 3, -, 1, 2");
        Console.WriteLine("C, 2, 1, -, 3");
        Console.WriteLine("D, 1, 2, 3, -");
        Console.WriteLine();
        Console.WriteLine("Black/White");
        Console.WriteLine(" , A, B, C, D");
        Console.WriteLine("A, -, b, b, b");
        Console.WriteLine("B, w, -, b, b");
        Console.WriteLine("C, w, w, -, b");
        Console.WriteLine("D, w, w, w, -");
        Console.WriteLine();
        Console.WriteLine("対局記号表");
        Console.WriteLine("A, \"Alice\"");
        Console.WriteLine("B, \"Bob\"");
        Console.WriteLine("C, \"Carol\"");
        Console.WriteLine("D, \"Dave\"");
        Console.WriteLine("END\n");
    }

    static void PrintFinalStageInputSample()
    {
        Console.WriteLine("本戦専用モードの入力形式:");
        Console.WriteLine("1. 選手一覧CSV");
        Console.WriteLine("2. グループ対応CSV");
        Console.WriteLine("3. 本戦不出場Apex一覧CSV（省略可）");
        Console.WriteLine("4. 対局CSV または Round/Black-White/対局記号表\n");
        Console.WriteLine("選手一覧CSVの例:");
        Console.WriteLine("name,elo");
        Console.WriteLine("Alice,5000");
        Console.WriteLine("Bob,4980");
        Console.WriteLine("Carol,4960");
        Console.WriteLine("Dave,4940\n");
        Console.WriteLine("グループ対応CSVの例:");
        Console.WriteLine("group,name");
        Console.WriteLine("Apex,Alice");
        Console.WriteLine("Apex,Bob");
        Console.WriteLine("Innov,Carol");
        Console.WriteLine("Innov,Dave\n");
        Console.WriteLine("本戦不出場Apex一覧の例（省略可）:");
        Console.WriteLine("name,elo");
        Console.WriteLine("Eve,4920");
        Console.WriteLine("Frank,4900\n");
        Console.WriteLine("対局CSVの例:");
        Console.WriteLine("black,white");
        Console.WriteLine("Carol,Alice");
        Console.WriteLine("Dave,Bob");
        Console.WriteLine("END\n");
    }
}

