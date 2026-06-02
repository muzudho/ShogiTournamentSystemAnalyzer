/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class TournamentQualityEvaluationExecutor
{
    internal static TournamentQualityEvaluationExecutionOptions ReadTournamentQualityEvaluationExecutionOptions(
        TournamentQualityEvaluationInput input,
        TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var sweepOptions = ReadTournamentQualitySweepOptions();

        if (!sweepOptions.IsEnabled)
        {
            var firstPlayerWinRatePercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange(
                "同Elo対局時の先手勝率(%)を入力してください [51]: ",
                51.0,
                0.0,
                100.0);
            Console.WriteLine();

            int? simulationCount = null;
            if (!ruleDefinition.UsesFinalStageGrouping)
            {
                if (input.Matches.Count <= 20)
                {
                    Console.WriteLine($"{TournamentRuleSetRule.GetLabel(ruleDefinition.TournamentRuleSetMode)} の品質評価用厳密計算を行います。\n");
                }
                else
                {
                    const int defaultSimulationCount = 200_000;
                    simulationCount = ConsolePromptReaders.ReadIntWithDefault(
                        $"局数が多いため {TournamentRuleSetRule.GetLabel(ruleDefinition.TournamentRuleSetMode)} の品質評価用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                        defaultSimulationCount,
                        min: 1);

                    Console.WriteLine();
                }
            }
            else if (input.Matches.Count <= 20)
            {
                Console.WriteLine("品質評価用の厳密計算を行います。\n");
            }
            else
            {
                const int defaultSimulationCount = 200_000;
                simulationCount = ConsolePromptReaders.ReadIntWithDefault(
                    $"局数が多いため品質評価用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                    defaultSimulationCount,
                    min: 1);

                Console.WriteLine();
            }

            return new TournamentQualityEvaluationExecutionOptions(simulationCount, sweepOptions, firstPlayerWinRatePercent);
        }

        return new TournamentQualityEvaluationExecutionOptions(null, sweepOptions, null);
    }

    static TournamentQualitySweepOptions ReadTournamentQualitySweepOptions()
    {
        Console.WriteLine("品質評価の実行方法を選んでください。");
        Console.WriteLine("単発評価は現在の条件だけを評価し、n% スイープ実験は先手勝率を範囲で振って比較します。");
        Console.WriteLine("1. 単発評価");
        Console.WriteLine("2. n% スイープ実験\n");

        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write("実行方法を入力してください [1]: ");
            var input = ConsoleInput.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return new TournamentQualitySweepOptions(false, 0.0, 0.0, 0.0);
            }

            if (input == "2")
            {
                Console.WriteLine();
                var sweepAttempt = 0;
                while (true)
                {
                    sweepAttempt++;
                    Console.WriteLine("補足: 例として 50 → 55 を 1 刻みで指定すると、50, 51, 52, 53, 54, 55 を順に評価します。\n");
                    var startPercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("開始する先手勝率(%)を入力してください [50]: ", 50.0, 0.0, 100.0);
                    var endPercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("終了する先手勝率(%)を入力してください [55]: ", 55.0, 0.0, 100.0);
                    var stepPercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("刻み幅(%)を入力してください [1]: ", 1.0, 0.000001, 100.0);
                    Console.WriteLine();

                    if (endPercent < startPercent)
                    {
                        if (sweepAttempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("n% スイープ範囲", "終了する先手勝率が開始する先手勝率未満です");

                        Console.WriteLine("終了する先手勝率は開始する先手勝率以上で入力してください。\n");
                        continue;
                    }

                    return new TournamentQualitySweepOptions(true, startPercent, endPercent, stepPercent);
                }
            }

            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("品質評価の実行方法", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }
}

