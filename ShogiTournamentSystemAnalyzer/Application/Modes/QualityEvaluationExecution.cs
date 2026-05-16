internal static partial class Program
{
    static void RunQualitySweepExperiment(
        QualityEvaluationInput input,
        QualityEvaluationRuleDefinition ruleDefinition,
        QualityEvaluationExecutionOptions executionOptions)
    {
        var sweepRows = new List<QualitySweepRow>();
        using var simulationBudget = executionOptions.SimulationCount.HasValue ? BeginSimulationBudget() : default;
        var stoppedByTimeout = false;
        for (var blackAdvantagePercent = executionOptions.SweepOptions.StartPercent; blackAdvantagePercent <= executionOptions.SweepOptions.EndPercent + 1e-9; blackAdvantagePercent += executionOptions.SweepOptions.StepPercent)
        {
            var qualityEvaluationRun = ExecuteQualityEvaluationRun(
                input,
                ruleDefinition,
                executionOptions with { BlackAdvantagePercent = blackAdvantagePercent });

            sweepRows.Add(new QualitySweepRow(
                blackAdvantagePercent,
                qualityEvaluationRun.Summary.SpearmanCorrelation,
                qualityEvaluationRun.Summary.MeanAbsoluteRankError,
                qualityEvaluationRun.Summary.AverageTop8Retention,
                qualityEvaluationRun.Summary.EloTop1OverallTop1Probability,
                qualityEvaluationRun.Summary.MostPenalizedParticipantName,
                qualityEvaluationRun.Summary.MostPenalizedDelta,
                qualityEvaluationRun.Summary.MostAdvantagedParticipantName,
                qualityEvaluationRun.Summary.MostAdvantagedDelta));

            if (qualityEvaluationRun.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
            {
                stoppedByTimeout = true;
                break;
            }
        }

        PrintQualitySweepRows(sweepRows);
        if (stoppedByTimeout)
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeLimit.TotalMinutes:F0} 分で打ち切ったため、n% スイープは途中で終了しました。\n");
        }

        var outputOptions = ReadQualitySweepOutputOptions(ruleDefinition);
        WriteQualitySweepCsv(outputOptions.OutputCsvPath, sweepRows, outputOptions.ReportGroupingOptions);

        Console.WriteLine($"n%スイープ結果CSVを出力しました: {outputOptions.OutputCsvPath}");
    }

    static QualityEvaluationRun ExecuteQualityEvaluationRun(
        QualityEvaluationInput input,
        QualityEvaluationRuleDefinition ruleDefinition,
        QualityEvaluationExecutionOptions executionOptions)
    {
        var blackAdvantagePercent = executionOptions.BlackAdvantagePercent!.Value;
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
        using var simulationBudget = executionOptions.SimulationCount.HasValue ? BeginSimulationBudget() : default;
        var result = ruleDefinition.GroupingMode == FinalStageGroupingMode.On
            ? executionOptions.SimulationCount.HasValue
                ? CalculateFinalStageBySimulation(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, blackAdvantageRating, executionOptions.SimulationCount.Value, ruleDefinition.PromotedInnovCount)
                : CalculateFinalStageExactly(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, blackAdvantageRating, ruleDefinition.PromotedInnovCount)
            : executionOptions.SimulationCount.HasValue
                ? CalculateBySimulation(input.Participants, input.Matches, blackAdvantageRating, executionOptions.SimulationCount.Value, ruleDefinition.TournamentRuleSetMode)
                : CalculateExactly(input.Participants, input.Matches, blackAdvantageRating, ruleDefinition.TournamentRuleSetMode);

        var resultRows = BuildResultRows(input.Participants, input.Matches, result, blackAdvantagePercent);
        var qualityParticipantRows = BuildQualityParticipantRows(resultRows, ruleDefinition.GroupMap, ruleDefinition.AdditionalApexParticipants, ruleDefinition.AdditionalApexPlacementMode);
        var qualitySummary = BuildQualitySummary(qualityParticipantRows);
        return new QualityEvaluationRun(qualityParticipantRows, qualitySummary, result.Mode);
    }

    static QualityEvaluationExecutionOptions ReadQualityEvaluationExecutionOptions(
        QualityEvaluationInput input,
        QualityEvaluationRuleDefinition ruleDefinition)
    {
        var sweepOptions = ReadQualitySweepOptions();

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
                simulationCount = ReadIntWithDefault(
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
            simulationCount = ReadIntWithDefault(
                $"局数が多いため品質評価用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                defaultSimulationCount,
                min: 1);

            Console.WriteLine();
        }

        return new QualityEvaluationExecutionOptions(simulationCount, sweepOptions, null);
    }

    static QualitySweepOptions ReadQualitySweepOptions()
    {
        Console.WriteLine("品質評価の実行方法を選んでください。");
        Console.WriteLine("1. 単発評価");
        Console.WriteLine("2. n% スイープ実験\n");

        while (true)
        {
            Console.Write("実行方法を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return new QualitySweepOptions(false, 0.0, 0.0, 0.0);
            }

            if (input == "2")
            {
                Console.WriteLine();
                while (true)
                {
                    var startPercent = ReadDoubleWithDefaultInRange("開始する先手勝率(%)を入力してください [50]: ", 50.0, 0.0, 100.0);
                    var endPercent = ReadDoubleWithDefaultInRange("終了する先手勝率(%)を入力してください [55]: ", 55.0, 0.0, 100.0);
                    var stepPercent = ReadDoubleWithDefaultInRange("刻み幅(%)を入力してください [1]: ", 1.0, 0.000001, 100.0);
                    Console.WriteLine();

                    if (endPercent < startPercent)
                    {
                        Console.WriteLine("終了する先手勝率は開始する先手勝率以上で入力してください。\n");
                        continue;
                    }

                    return new QualitySweepOptions(true, startPercent, endPercent, stepPercent);
                }
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }
}
