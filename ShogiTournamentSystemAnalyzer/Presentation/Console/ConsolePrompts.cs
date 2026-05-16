internal static partial class Program
{
    static int ReadMode()
    {
        Console.WriteLine("モードを選んでください。");
        Console.WriteLine("1. 通常モード（総当たり戦分析）");
        Console.WriteLine("2. 本戦専用モード（Apex / Innov 定先戦分析）");
        Console.WriteLine("3. 品質評価モード（本戦ルールの実力反映性評価）\n");

        while (true)
        {
            Console.Write("モード番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return 1;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return 2;
            }

            if (input == "3")
            {
                Console.WriteLine();
                return 3;
            }

            Console.WriteLine("1、2、3 のいずれかを入力してください。\n");
        }
    }

    static string ReadTextWithDefault(string prompt, string defaultValue)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();
        if (input is null)
        {
            throw new OperationCanceledException("文字列入力中に入力ストリームが終了しました。");
        }

        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }

    static int ReadIntWithDefault(string prompt, int defaultValue, int min)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (input is null)
            {
                throw new OperationCanceledException("整数入力中に入力ストリームが終了しました。");
            }

            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            if (int.TryParse(input, out var value) && value >= min)
            {
                return value;
            }

            Console.WriteLine($"{min} 以上の整数を入力してください。");
        }
    }

    static double ReadDoubleWithDefaultInRange(string prompt, double defaultValue, double minInclusive, double maxInclusive)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (input is null)
            {
                throw new OperationCanceledException("数値入力中に入力ストリームが終了しました。");
            }

            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            if (TryParseDouble(input, out var value) && value >= minInclusive && value <= maxInclusive)
            {
                return value;
            }

            Console.WriteLine($"{minInclusive} 以上 {maxInclusive} 以下の数値を入力してください。");
        }
    }
}

