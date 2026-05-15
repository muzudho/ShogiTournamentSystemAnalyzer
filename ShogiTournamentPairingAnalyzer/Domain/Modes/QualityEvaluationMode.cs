using System.Globalization;

internal static partial class Program
{
    static void RunQualityEvaluationMode()
    {
        Console.WriteLine("品質評価モード: 本戦ルールの実力反映性を評価します。\n");

        PrintFinalStageInputSample();
        var tournamentRuleSetMode = TournamentRuleSetRule.ReadMode();

        var participants = ReadParticipantsFromCsv();
        Console.WriteLine();

        var groupingMode = FinalStageGroupingRule.ReadMode();
        var groupMap = ReadOptionalFinalStageGroupMap(groupingMode, participants);
        string errorMessage;
        var participantsAreValid = groupingMode == FinalStageGroupingMode.On
            ? ValidateFinalStageParticipants(participants, groupMap!, out errorMessage)
            : ValidateFinalStageParticipants(participants, out errorMessage);
        if (!participantsAreValid)
        {
            Console.WriteLine($"本戦参加者の検証に失敗しました: {errorMessage}\n");
            return;
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
                return;
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

        var matches = ReadMatchesFromCsv(participants);
        var matchesAreValid = groupingMode == FinalStageGroupingMode.On
            ? ValidateFinalStageMatches(participants, groupMap!, matches, out errorMessage)
            : ValidateFinalStageMatches(participants, matches, out errorMessage);
        if (!matchesAreValid)
        {
            Console.WriteLine($"本戦対局の検証に失敗しました: {errorMessage}\n");
            return;
        }

        Console.WriteLine();
        var referenceMatches = ReadOptionalMatchesFromCsv(participants, "参考対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。大会記録に含めない場合だけ使います。");

        var sweepOptions = ReadQualitySweepOptions();

        int? simulationCount = null;
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            if (matches.Count <= 20)
            {
                Console.WriteLine("ニュートラルな品質評価用厳密計算を行います。\n");
            }
            else
            {
                const int defaultSimulationCount = 200_000;
                simulationCount = ReadIntWithDefault(
                    $"局数が多いためニュートラルな品質評価用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                    defaultSimulationCount,
                    min: 1);

                Console.WriteLine();
            }
        }
        else if (matches.Count <= 20)
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

        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(tournamentRuleSetMode)}\n");
        Console.WriteLine($"Apex / Innov の分け方: {FinalStageGroupingRule.GetLabel(groupingMode)}\n");
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine($"本戦不出場Apexの扱い: {AdditionalApexPlacementRule.GetLabel(additionalApexPlacementMode)}\n");
            Console.WriteLine($"境界救済戦: {BoundaryRescueRule.GetLabel(boundaryRescueMode)}\n");
            Console.WriteLine($"可変定員8ルール: {VariableTop8Rule.GetLabel(variableTop8Mode)}\n");
        }
        if (referenceMatches.Count > 0)
        {
            PrintMatchesCsv(participants, referenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {referenceMatches.Count}");
            Console.WriteLine("参考対局は品質評価に含めません。\n");
        }

        if (sweepOptions.IsEnabled)
        {
            RunQualitySweepExperiment(
                participants,
                matches,
                groupMap,
                groupingMode,
                additionalApexParticipants,
                additionalApexPlacementMode,
                effectiveAdditionalApexCount,
                boundaryRescueMode,
                promotedInnovCount,
                simulationCount,
                sweepOptions,
                tournamentRuleSetMode);
            return;
        }

        var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        Console.WriteLine();

        var qualityEvaluationRun = ExecuteQualityEvaluationRun(
            participants,
            matches,
            groupMap,
            groupingMode,
            additionalApexParticipants,
            additionalApexPlacementMode,
            effectiveAdditionalApexCount,
            boundaryRescueMode,
            promotedInnovCount,
            blackAdvantagePercent,
                simulationCount,
                tournamentRuleSetMode);

        PrintQualitySummary(qualityEvaluationRun.Summary);
        PrintQualityParticipantHighlights(qualityEvaluationRun.ParticipantRows);

        var reportGroupingOptions = ReadExperimentalReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySummaryDefaultOutputPath(groupingMode, additionalApexPlacementMode, boundaryRescueMode, reportGroupingOptions);
        var summaryCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n品質評価サマリーCSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriteQualitySummaryCsv(summaryCsvPath, qualityEvaluationRun.Summary, reportGroupingOptions);

        var participantCsvPath = BuildSiblingOutputCsvPath(summaryCsvPath, "quality_participants");
        WriteQualityParticipantCsv(participantCsvPath, qualityEvaluationRun.ParticipantRows);

        Console.WriteLine($"品質評価サマリーCSVを出力しました: {summaryCsvPath}");
        Console.WriteLine($"品質評価参加者別CSVを出力しました: {participantCsvPath}");
    }

    static void RunQualitySweepExperiment(
        IReadOnlyList<Participant> participants,
        IReadOnlyList<Match> matches,
        IReadOnlyDictionary<string, FinalStageGroup>? groupMap,
        FinalStageGroupingMode groupingMode,
        IReadOnlyList<Participant> additionalApexParticipants,
        AdditionalApexPlacementMode additionalApexPlacementMode,
        int effectiveAdditionalApexCount,
        BoundaryRescueMode boundaryRescueMode,
        int promotedInnovCount,
        int? simulationCount,
        QualitySweepOptions sweepOptions,
        TournamentRuleSetMode tournamentRuleSetMode)
    {
        var sweepRows = new List<QualitySweepRow>();
        for (var blackAdvantagePercent = sweepOptions.StartPercent; blackAdvantagePercent <= sweepOptions.EndPercent + 1e-9; blackAdvantagePercent += sweepOptions.StepPercent)
        {
            var qualityEvaluationRun = ExecuteQualityEvaluationRun(
                participants,
                matches,
                groupMap,
                groupingMode,
                additionalApexParticipants,
                additionalApexPlacementMode,
                effectiveAdditionalApexCount,
                boundaryRescueMode,
                promotedInnovCount,
                blackAdvantagePercent,
                simulationCount,
                tournamentRuleSetMode);

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
        }

        PrintQualitySweepRows(sweepRows);

        var reportGroupingOptions = ReadExperimentalReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySweepDefaultOutputPath(groupingMode, additionalApexPlacementMode, boundaryRescueMode, reportGroupingOptions);
        var sweepCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\nn%スイープ結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriteQualitySweepCsv(sweepCsvPath, sweepRows, reportGroupingOptions);

        Console.WriteLine($"n%スイープ結果CSVを出力しました: {sweepCsvPath}");
    }

    static QualityEvaluationRun ExecuteQualityEvaluationRun(
        IReadOnlyList<Participant> participants,
        IReadOnlyList<Match> matches,
        IReadOnlyDictionary<string, FinalStageGroup>? groupMap,
        FinalStageGroupingMode groupingMode,
        IReadOnlyList<Participant> additionalApexParticipants,
        AdditionalApexPlacementMode additionalApexPlacementMode,
        int effectiveAdditionalApexCount,
        BoundaryRescueMode boundaryRescueMode,
        int promotedInnovCount,
        double blackAdvantagePercent,
        int? simulationCount,
        TournamentRuleSetMode tournamentRuleSetMode)
    {
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
        var result = groupingMode == FinalStageGroupingMode.On
            ? simulationCount.HasValue
                ? CalculateFinalStageBySimulation(participants, matches, groupMap!, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating, simulationCount.Value, promotedInnovCount)
                : CalculateFinalStageExactly(participants, matches, groupMap!, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating, promotedInnovCount)
            : simulationCount.HasValue
                ? CalculateBySimulation(participants, matches, blackAdvantageRating, simulationCount.Value, tournamentRuleSetMode)
                : CalculateExactly(participants, matches, blackAdvantageRating, tournamentRuleSetMode);

        var resultRows = BuildResultRows(participants, matches, result, blackAdvantagePercent);
        var qualityParticipantRows = BuildQualityParticipantRows(resultRows, groupMap, additionalApexParticipants, additionalApexPlacementMode);
        var qualitySummary = BuildQualitySummary(qualityParticipantRows);
        return new QualityEvaluationRun(qualityParticipantRows, qualitySummary);
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
