internal static partial class Program
{
    static void RunQualityEvaluationMode()
    {
        Console.WriteLine("品質評価モード: 本戦ルールの実力反映性を評価します。\n");

        PrintFinalStageInputSample();

        var participants = ReadPlayersFromCsv();
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
                executionOptions);
            return;
        }

        var qualityEvaluationRun = ExecuteQualityEvaluationRun(
            input,
            ruleDefinition,
            executionOptions);

        PrintQualitySummary(qualityEvaluationRun.Summary);
        PrintQualityPlayerHighlights(qualityEvaluationRun.PlayerRows);
        if (qualityEvaluationRun.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var outputOptions = ReadQualitySummaryOutputOptions(ruleDefinition);
        WriteQualityEvaluationOutputs(qualityEvaluationRun, outputOptions);
    }

    static bool TryReadQualityEvaluationRuleDefinition(
        IReadOnlyList<Player> participants,
        out QualityEvaluationRuleDefinition ruleDefinition)
    {
        var groupingMode = ReadFinalStageGroupingMode();
        var tournamentRuleSetMode = groupingMode == FinalStageGroupingMode.Off
            ? ReadTournamentRuleSetMode()
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

        List<Player> additionalApexParticipants;
        var additionalApexPlacementMode = AdditionalApexPlacementMode.Off;
        var effectiveAdditionalApexCount = 0;
        var boundaryRescueMode = BoundaryRescueMode.Off;
        var variableTop8Mode = VariableTop8Mode.Off;
        var promotedInnovCount = 0;
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine();
            additionalApexParticipants = ReadOptionalPlayersFromCsv("本戦不出場Apex一覧CSVを貼り付けてください。");
            if (!ValidateAdditionalApexParticipants(participants, groupMap!, additionalApexParticipants, out errorMessage))
            {
                Console.WriteLine($"本戦不出場Apex一覧の検証に失敗しました: {errorMessage}\n");
                ruleDefinition = default;
                return false;
            }

            additionalApexPlacementMode = ReadAdditionalApexPlacementMode();
            effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexParticipants.Count, additionalApexPlacementMode);
            boundaryRescueMode = ReadBoundaryRescueMode();
            variableTop8Mode = ReadVariableTop8Mode();
            promotedInnovCount = VariableTop8Rule.GetPromotedInnovCount(variableTop8Mode, additionalApexParticipants.Count);
        }
        else
        {
            additionalApexParticipants = new List<Player>();
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
        IReadOnlyList<Player> participants,
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

}

