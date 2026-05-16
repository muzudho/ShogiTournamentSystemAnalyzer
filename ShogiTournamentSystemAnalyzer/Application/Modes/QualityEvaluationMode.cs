internal static partial class Program
{
    static void RunQualityEvaluationMode(RuleProfileMode ruleProfileMode)
    {
        if (ruleProfileMode == RuleProfileMode.Standard)
        {
            Console.WriteLine("品質評価 / 通常ルール: 総当たり戦向けルールの実力反映性を評価します。\n");
            PrintQualityEvaluationStandardOverview();
        }
        else
        {
            Console.WriteLine("品質評価 / 本戦ルール: 本戦ルールの実力反映性を評価します。\n");
            PrintQualityEvaluationFinalStageOverview();
        }

        var players = ReadPlayersFromCsv();
        Console.WriteLine();

        if (!TryReadQualityEvaluationRuleDefinition(players, ruleProfileMode, out var ruleDefinition))
        {
            return;
        }

        if (!TryReadQualityEvaluationInput(players, ruleDefinition, out var input))
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
        IReadOnlyList<Player> players,
        RuleProfileMode ruleProfileMode,
        out QualityEvaluationRuleDefinition ruleDefinition)
    {
        var groupingMode = ruleProfileMode == RuleProfileMode.FinalStage
            ? FinalStageGroupingMode.On
            : FinalStageGroupingMode.Off;
        var tournamentRuleSetMode = ruleProfileMode == RuleProfileMode.Standard
            ? ReadTournamentRuleSetMode()
            : TournamentRuleSetMode.Neutral;
        var groupMap = ruleProfileMode == RuleProfileMode.FinalStage
            ? ReadOptionalFinalStageGroupMap(groupingMode, players)
            : null;

        var playersAreValid = groupingMode == FinalStageGroupingMode.On
            ? ValidateFinalStageParticipants(players, groupMap!, out var errorMessage)
            : ValidateFinalStageParticipants(players, out errorMessage);
        if (!playersAreValid)
        {
            var targetLabel = groupingMode == FinalStageGroupingMode.On ? "本戦選手" : "選手一覧";
            Console.WriteLine($"{targetLabel}の検証に失敗しました: {errorMessage}\n");
            ruleDefinition = default;
            return false;
        }

        List<Player> additionalApexPlayers;
        var additionalApexPlacementMode = AdditionalApexPlacementMode.Off;
        var effectiveAdditionalApexCount = 0;
        var boundaryRescueMode = BoundaryRescueMode.Off;
        var variableTop8Mode = VariableTop8Mode.Off;
        var promotedInnovCount = 0;
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine();
            additionalApexPlayers = ReadOptionalPlayersFromCsv("本戦不出場Apex一覧CSVを貼り付けてください。");
            if (!ValidateAdditionalApexParticipants(players, groupMap!, additionalApexPlayers, out errorMessage))
            {
                Console.WriteLine($"本戦不出場Apex一覧の検証に失敗しました: {errorMessage}\n");
                ruleDefinition = default;
                return false;
            }

            additionalApexPlacementMode = ReadAdditionalApexPlacementMode();
            effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexPlayers.Count, additionalApexPlacementMode);
            boundaryRescueMode = ReadBoundaryRescueMode();
            variableTop8Mode = ReadVariableTop8Mode();
            promotedInnovCount = VariableTop8Rule.GetPromotedInnovCount(variableTop8Mode, additionalApexPlayers.Count);
        }
        else
        {
            additionalApexPlayers = new List<Player>();
        }

        ruleDefinition = new QualityEvaluationRuleDefinition(
            groupingMode,
            tournamentRuleSetMode,
            groupMap,
            additionalApexPlayers,
            additionalApexPlacementMode,
            effectiveAdditionalApexCount,
            boundaryRescueMode,
            variableTop8Mode,
            promotedInnovCount);
        return true;
    }

    static bool TryReadQualityEvaluationInput(
        IReadOnlyList<Player> players,
        QualityEvaluationRuleDefinition ruleDefinition,
        out QualityEvaluationInput input)
    {
        var matches = ReadMatchesFromCsv(players);
        var matchesAreValid = ruleDefinition.UsesFinalStageGrouping
            ? ValidateFinalStageMatches(players, ruleDefinition.GroupMap!, matches, out var errorMessage)
            : ValidateFinalStageMatches(players, matches, out errorMessage);
        if (!matchesAreValid)
        {
            var matchLabel = ruleDefinition.UsesFinalStageGrouping ? "本戦対局" : "対局";
            Console.WriteLine($"{matchLabel}の検証に失敗しました: {errorMessage}\n");
            input = default;
            return false;
        }

        Console.WriteLine();
        var referenceMatches = ReadOptionalMatchesFromCsv(players, "参考対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。大会記録に含めない場合だけ使います。");
        input = new QualityEvaluationInput(players, matches, referenceMatches);
        return true;
    }

}

