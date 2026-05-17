internal static partial class Program
{
    static readonly int InputRetryLimit = 10;

    static void ThrowInputRetryLimitExceeded(string targetLabel, string lastErrorMessage)
    {
        throw new OperationCanceledException($"{targetLabel}の入力失敗が {InputRetryLimit} 回に達したため中断しました。最後のエラー: {lastErrorMessage}");
    }

    static AnalysisFlowMode ReadAnalysisFlowMode()
    {
        Console.WriteLine("目的を選んでください。");
        Console.WriteLine("1. 対局シミュレーション");
        Console.WriteLine("2. 品質評価\n");

        var attempt = 0;
        while (true)
        {
            attempt++;
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

            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("目的選択", "1 または 2 以外が入力されました");
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

        var attempt = 0;
        while (true)
        {
            attempt++;
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

            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("対象ルール選択", "1 または 2 以外が入力されました");
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
        var attempt = 0;
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

            attempt++;
            if (int.TryParse(input, out var value) && value >= min)
            {
                return value;
            }

            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("整数入力", $"{min} 以上の整数ではありません");
            }

            Console.WriteLine($"{min} 以上の整数を入力してください。");
        }
    }

    static double ReadDoubleWithDefaultInRange(string prompt, double defaultValue, double minInclusive, double maxInclusive)
    {
        var attempt = 0;
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

            attempt++;
            if (TryParseDouble(input, out var value) && value >= minInclusive && value <= maxInclusive)
            {
                return value;
            }

            if (attempt >= InputRetryLimit)
            {
                ThrowInputRetryLimitExceeded("数値入力", $"{minInclusive} 以上 {maxInclusive} 以下の数値ではありません");
            }

            Console.WriteLine($"{minInclusive} 以上 {maxInclusive} 以下の数値を入力してください。");
        }
    }
}

