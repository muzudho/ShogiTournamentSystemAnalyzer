/*
 * ［プレゼンテーション　＞　コンソール改］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using System;
using System.Globalization;

internal static class ConsolePromptReaders
{
    internal static readonly int InputRetryLimit = 10;

    /// <summary>
    /// 無限ループに陥るのを防ぐぜ（＾▽＾）
    /// </summary>
    /// <param name="targetLabel"></param>
    /// <param name="lastErrorMessage"></param>
    /// <exception cref="OperationCanceledException"></exception>
    internal static void ThrowInputRetryLimitExceeded(string targetLabel, string lastErrorMessage)
    {
        throw new OperationCanceledException($"{targetLabel}の入力失敗が {InputRetryLimit} 回に達したため中断しました。最後のエラー: {lastErrorMessage}");
    }

    /// <summary>
    /// TODO: ［シミュレーション］か、［品質評価］のどちらかを選ばせるのではなく、フローで直列に並べたいぜ（＾～＾）
    /// </summary>
    /// <returns></returns>
    internal static AnalysisFlowMode ReadAnalysisFlowMode()
    {
        Console.WriteLine("目的を選んでください。");
        Console.WriteLine("1. 対局シミュレーション");
        Console.WriteLine("2. 品質評価\n");

        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("目的選択");
            attempt++;
            Console.Write("番号を入力してください [1]: ");
            var input = ConsoleInput.ReadLine()?.Trim();
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

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("目的選択", "1 または 2 以外が入力されました");

            Console.WriteLine("1、2 のいずれかを入力してください。\n");
        }
    }

    /// <summary>
    ///     <pre>
    /// 対象［大会ルール］を選ばせるぜ（＾▽＾）！
    /// これは、外部の設定ファイルで編集できるようにしたいぜ（＾▽＾）！
    ///     </pre>
    /// </summary>
    /// <param name="flowMode"></param>
    /// <returns></returns>
    internal static RuleProfileMode ReadRuleProfileMode(AnalysisFlowMode flowMode)
    {
        var flowLabel = flowMode == AnalysisFlowMode.Simulation ? "対局シミュレーション" : "品質評価";
        Console.WriteLine($"{flowLabel} の対象ルールを選んでください。");
        Console.WriteLine("1. 通常ルール");
        Console.WriteLine("2. 本戦ルール");
        if (flowMode == AnalysisFlowMode.Simulation)
        {
            Console.WriteLine("3. 大会進行フレームワーク");
            Console.WriteLine("4. 空ルール");
        }

        Console.WriteLine();

        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("対象ルール選択");
            attempt++;
            Console.Write("番号を入力してください [1]: ");
            var input = ConsoleInput.ReadLine()?.Trim();
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

            if (flowMode == AnalysisFlowMode.Simulation && input == "3")
            {
                Console.WriteLine();
                return RuleProfileMode.TournamentFramework;
            }

            if (flowMode == AnalysisFlowMode.Simulation && input == "4")
            {
                Console.WriteLine();
                return RuleProfileMode.Empty;
            }

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("対象ルール選択", flowMode == AnalysisFlowMode.Simulation ? "1、2、3、4 のいずれでもありません" : "1 または 2 以外が入力されました");

            Console.WriteLine(flowMode == AnalysisFlowMode.Simulation
                ? "1、2、3、4 のいずれかを入力してください。\n"
                : "1、2 のいずれかを入力してください。\n");
        }
    }

    internal static string ReadTextWithDefault(string prompt, string defaultValue)
    {
        if (!SimulationTimeBudget.HasApplicationTimeRemaining())
        {
            Console.WriteLine(prompt + defaultValue);
            return defaultValue;
        }

        Console.Write(prompt);
        var input = ConsoleInput.ReadLine()?.Trim();
        if (input is null) throw new OperationCanceledException("文字列入力中に入力ストリームが終了しました。");

        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }

    internal static int ReadIntWithDefault(string prompt, int defaultValue, int min)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("整数入力");
            Console.Write(prompt);
            var input = ConsoleInput.ReadLine()?.Trim();
            if (input is null) throw new OperationCanceledException("整数入力中に入力ストリームが終了しました。");

            if (string.IsNullOrEmpty(input)) return defaultValue;

            attempt++;
            if (int.TryParse(input, out var value) && value >= min) return value;

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("整数入力", $"{min} 以上の整数ではありません");

            Console.WriteLine($"{min} 以上の整数を入力してください。");
        }
    }

    internal static double ReadDoubleWithDefaultInRange(string prompt, double defaultValue, double minInclusive, double maxInclusive)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("数値入力");
            Console.Write(prompt);
            var input = ConsoleInput.ReadLine()?.Trim();
            if (input is null) throw new OperationCanceledException("数値入力中に入力ストリームが終了しました。");

            if (string.IsNullOrEmpty(input)) return defaultValue;

            attempt++;
            if ((double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var value)
                || double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                && value >= minInclusive
                && value <= maxInclusive) return value;

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("数値入力", $"{minInclusive} 以上 {maxInclusive} 以下の数値ではありません");

            Console.WriteLine($"{minInclusive} 以上 {maxInclusive} 以下の数値を入力してください。");
        }
    }
}

