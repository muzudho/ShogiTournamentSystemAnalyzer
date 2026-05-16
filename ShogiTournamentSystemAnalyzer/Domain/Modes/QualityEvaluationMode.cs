using System.Globalization;

internal static partial class Program
{
    static void RunQualityEvaluationMode()
    {
        Console.WriteLine("品質評価モード: 本戦ルールの実力反映性を評価します。\n");

        PrintFinalStageInputSample();

        var participants = ReadParticipantsFromCsv();
        Console.WriteLine();

        if (!TryReadQualityEvaluationRuleDefinition(participants, out var ruleDefinition))
        {
            return;
        }

        if (!TryReadQualityEvaluationInput(participants, ruleDefinition, out var input))
        {
            return;
        }

        var executionOptions = ReadQualityEvaluationExecutionOptions(input, ruleDefinition);
        PrintQualityEvaluationContext(input, ruleDefinition);

        if (executionOptions.IsSweep)
        {
            RunQualitySweepExperiment(
                input,
                ruleDefinition,
                executionOptions.SimulationCount,
                executionOptions.SweepOptions);
            return;
        }

        var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        Console.WriteLine();
        executionOptions = executionOptions with { BlackAdvantagePercent = blackAdvantagePercent };

        var qualityEvaluationRun = ExecuteQualityEvaluationRun(
            input,
            ruleDefinition,
            executionOptions.BlackAdvantagePercent!.Value,
            executionOptions.SimulationCount);

        PrintQualitySummary(qualityEvaluationRun.Summary);
        PrintQualityParticipantHighlights(qualityEvaluationRun.ParticipantRows);
        if (qualityEvaluationRun.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var outputOptions = ReadQualitySummaryOutputOptions(ruleDefinition);
        WriteQualityEvaluationOutputs(qualityEvaluationRun, outputOptions);
    }

    static void RunQualitySweepExperiment(
        QualityEvaluationInput input,
        QualityEvaluationRuleDefinition ruleDefinition,
        int? simulationCount,
        QualitySweepOptions sweepOptions)
    {
        var sweepRows = new List<QualitySweepRow>();
        using var simulationBudget = simulationCount.HasValue ? BeginSimulationBudget() : default;
        var stoppedByTimeout = false;
        for (var blackAdvantagePercent = sweepOptions.StartPercent; blackAdvantagePercent <= sweepOptions.EndPercent + 1e-9; blackAdvantagePercent += sweepOptions.StepPercent)
        {
            var qualityEvaluationRun = ExecuteQualityEvaluationRun(
                input,
                ruleDefinition,
                blackAdvantagePercent,
                simulationCount);

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
        double blackAdvantagePercent,
        int? simulationCount)
    {
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
        using var simulationBudget = simulationCount.HasValue ? BeginSimulationBudget() : default;
        var result = ruleDefinition.GroupingMode == FinalStageGroupingMode.On
            ? simulationCount.HasValue
                ? CalculateFinalStageBySimulation(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, blackAdvantageRating, simulationCount.Value, ruleDefinition.PromotedInnovCount)
                : CalculateFinalStageExactly(input.Participants, input.Matches, ruleDefinition.GroupMap!, ruleDefinition.EffectiveAdditionalApexCount, ruleDefinition.BoundaryRescueMode, blackAdvantageRating, ruleDefinition.PromotedInnovCount)
            : simulationCount.HasValue
                ? CalculateBySimulation(input.Participants, input.Matches, blackAdvantageRating, simulationCount.Value, ruleDefinition.TournamentRuleSetMode)
                : CalculateExactly(input.Participants, input.Matches, blackAdvantageRating, ruleDefinition.TournamentRuleSetMode);

        var resultRows = BuildResultRows(input.Participants, input.Matches, result, blackAdvantagePercent);
        var qualityParticipantRows = BuildQualityParticipantRows(resultRows, ruleDefinition.GroupMap, ruleDefinition.AdditionalApexParticipants, ruleDefinition.AdditionalApexPlacementMode);
        var qualitySummary = BuildQualitySummary(qualityParticipantRows);
        return new QualityEvaluationRun(qualityParticipantRows, qualitySummary, result.Mode);
    }

    static bool TryReadQualityEvaluationRuleDefinition(
        IReadOnlyList<Participant> participants,
        out QualityEvaluationRuleDefinition ruleDefinition)
    {
        var groupingMode = FinalStageGroupingRule.ReadMode();
        var tournamentRuleSetMode = groupingMode == FinalStageGroupingMode.Off
            ? TournamentRuleSetRule.ReadMode()
            : TournamentRuleSetMode.Neutral;
        var groupMap = ReadOptionalFinalStageGroupMap(groupingMode, participants);

        var participantsAreValid = groupingMode == FinalStageGroupingMode.On
            ? ValidateFinalStageParticipants(participants, groupMap!, out var errorMessage)
            : ValidateFinalStageParticipants(participants, out errorMessage);
        if (!participantsAreValid)
        {
            Console.WriteLine($"本戦参加者の検証に失敗しました: {errorMessage}\n");
            ruleDefinition = default;
            return false;
        }

        List<Participant> additionalApexParticipants;
        var additionalApexPlacementMode = AdditionalApexPlacementMode.Off;
        var effectiveAdditionalApexCount = 0;
        var boundaryRescueMode = BoundaryRescueMode.Off;
        var variableTop8Mode = VariableTop8Mode.Off;
        var promotedInnovCount = 0;
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine();
            additionalApexParticipants = ReadOptionalParticipantsFromCsv("本戦不出場Apex一覧CSVを貼り付けてください。");
            if (!ValidateAdditionalApexParticipants(participants, groupMap!, additionalApexParticipants, out errorMessage))
            {
                Console.WriteLine($"本戦不出場Apex一覧の検証に失敗しました: {errorMessage}\n");
                ruleDefinition = default;
                return false;
            }

            additionalApexPlacementMode = AdditionalApexPlacementRule.ReadMode();
            effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexParticipants.Count, additionalApexPlacementMode);
            boundaryRescueMode = BoundaryRescueRule.ReadMode();
            variableTop8Mode = VariableTop8Rule.ReadMode();
            promotedInnovCount = VariableTop8Rule.GetPromotedInnovCount(variableTop8Mode, additionalApexParticipants.Count);
        }
        else
        {
            additionalApexParticipants = new List<Participant>();
        }

        ruleDefinition = new QualityEvaluationRuleDefinition(
            groupingMode,
            tournamentRuleSetMode,
            groupMap,
            additionalApexParticipants,
            additionalApexPlacementMode,
            effectiveAdditionalApexCount,
            boundaryRescueMode,
            variableTop8Mode,
            promotedInnovCount);
        return true;
    }

    static bool TryReadQualityEvaluationInput(
        IReadOnlyList<Participant> participants,
        QualityEvaluationRuleDefinition ruleDefinition,
        out QualityEvaluationInput input)
    {
        var matches = ReadMatchesFromCsv(participants);
        var matchesAreValid = ruleDefinition.UsesFinalStageGrouping
            ? ValidateFinalStageMatches(participants, ruleDefinition.GroupMap!, matches, out var errorMessage)
            : ValidateFinalStageMatches(participants, matches, out errorMessage);
        if (!matchesAreValid)
        {
            Console.WriteLine($"本戦対局の検証に失敗しました: {errorMessage}\n");
            input = default;
            return false;
        }

        Console.WriteLine();
        var referenceMatches = ReadOptionalMatchesFromCsv(participants, "参考対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。大会記録に含めない場合だけ使います。");
        input = new QualityEvaluationInput(participants, matches, referenceMatches);
        return true;
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

    static void PrintQualityEvaluationContext(
        QualityEvaluationInput input,
        QualityEvaluationRuleDefinition ruleDefinition)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(ruleDefinition.TournamentRuleSetMode)}\n");
        Console.WriteLine($"Apex / Innov の分け方: {FinalStageGroupingRule.GetLabel(ruleDefinition.GroupingMode)}\n");
        if (ruleDefinition.UsesFinalStageGrouping)
        {
            Console.WriteLine($"本戦不出場Apexの扱い: {AdditionalApexPlacementRule.GetLabel(ruleDefinition.AdditionalApexPlacementMode)}\n");
            Console.WriteLine($"境界救済戦: {BoundaryRescueRule.GetLabel(ruleDefinition.BoundaryRescueMode)}\n");
            Console.WriteLine($"可変定員8ルール: {VariableTop8Rule.GetLabel(ruleDefinition.VariableTop8Mode)}\n");
        }

        if (input.ReferenceMatches.Count > 0)
        {
            PrintMatchesCsv(input.Participants, input.ReferenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {input.ReferenceMatches.Count}");
            Console.WriteLine("参考対局は品質評価に含めません。\n");
        }
    }

    static QualityEvaluationOutputOptions ReadQualitySummaryOutputOptions(QualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ReadExperimentalReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySummaryDefaultOutputPath(
            ruleDefinition.GroupingMode,
            ruleDefinition.AdditionalApexPlacementMode,
            ruleDefinition.BoundaryRescueMode,
            reportGroupingOptions,
            ruleDefinition.TournamentRuleSetMode);
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n品質評価サマリーCSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        return new QualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath);
    }

    static QualityEvaluationOutputOptions ReadQualitySweepOutputOptions(QualityEvaluationRuleDefinition ruleDefinition)
    {
        var reportGroupingOptions = ReadExperimentalReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySweepDefaultOutputPath(
            ruleDefinition.GroupingMode,
            ruleDefinition.AdditionalApexPlacementMode,
            ruleDefinition.BoundaryRescueMode,
            reportGroupingOptions,
            ruleDefinition.TournamentRuleSetMode);
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\nn%スイープ結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        return new QualityEvaluationOutputOptions(reportGroupingOptions, outputCsvPath);
    }

    static void WriteQualityEvaluationOutputs(
        QualityEvaluationRun qualityEvaluationRun,
        QualityEvaluationOutputOptions outputOptions)
    {
        WriteQualitySummaryCsv(outputOptions.OutputCsvPath, qualityEvaluationRun.Summary, outputOptions.ReportGroupingOptions);

        var participantCsvPath = BuildSiblingOutputCsvPath(outputOptions.OutputCsvPath, "quality_participants");
        WriteQualityParticipantCsv(participantCsvPath, qualityEvaluationRun.ParticipantRows);

        Console.WriteLine($"品質評価サマリーCSVを出力しました: {outputOptions.OutputCsvPath}");
        Console.WriteLine($"品質評価参加者別CSVを出力しました: {participantCsvPath}");
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

    static void PrintQualitySweepRows(IReadOnlyList<QualitySweepRow> sweepRows)
    {
        Console.WriteLine("n%スイープ結果:");
        Console.WriteLine("先手勝率    Spearman   平均順位ずれ   上位8残留   Elo1位総合1位");

        foreach (var row in sweepRows)
        {
            Console.WriteLine(
                row.BlackAdvantagePercent.ToString("F2", CultureInfo.InvariantCulture).PadLeft(8)
                + "%"
                + row.SpearmanCorrelation.ToString("F4", CultureInfo.InvariantCulture).PadLeft(12)
                + row.MeanAbsoluteRankError.ToString("F3", CultureInfo.InvariantCulture).PadLeft(14)
                + row.AverageTop8Retention.ToString("F3", CultureInfo.InvariantCulture).PadLeft(12)
                + ((row.EloTop1OverallTop1Probability * 100).ToString("F2", CultureInfo.InvariantCulture) + "%").PadLeft(16));
        }

        Console.WriteLine();
    }
}
