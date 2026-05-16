internal static partial class Program
{
    static AnalysisFlowMode ReadAnalysisFlowMode()
    {
        Console.WriteLine("目的を選んでください。");
        Console.WriteLine("1. 対局シミュレーション");
        Console.WriteLine("2. 品質評価\n");

        while (true)
        {
            Console.Write("番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return AnalysisFlowMode.Simulation;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return AnalysisFlowMode.QualityEvaluation;
            }

            Console.WriteLine("1、2 のいずれかを入力してください。\n");
        }
    }

    static RuleProfileMode ReadRuleProfileMode(AnalysisFlowMode flowMode)
    {
        var flowLabel = flowMode == AnalysisFlowMode.Simulation ? "対局シミュレーション" : "品質評価";
        Console.WriteLine($"{flowLabel} の対象ルールを選んでください。");
        Console.WriteLine("1. 通常ルール");
        Console.WriteLine("2. 本戦ルール\n");

        while (true)
        {
            Console.Write("番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return RuleProfileMode.Standard;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return RuleProfileMode.FinalStage;
            }

            Console.WriteLine("1、2 のいずれかを入力してください。\n");
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

