internal static partial class Program
{
    static void RunQualityEvaluationMode()
    {
        Console.WriteLine("品質評価モード: 本戦ルールの実力反映性を評価します。\n");

        PrintFinalStageInputSample();

        var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
        Console.WriteLine();

        var participants = ReadParticipantsFromCsv();
        Console.WriteLine();

        var groupMap = ReadFinalStageGroupMap();
        if (!ValidateFinalStageParticipants(participants, groupMap, out var errorMessage))
        {
            Console.WriteLine($"本戦参加者の検証に失敗しました: {errorMessage}\n");
            return;
        }

        Console.WriteLine();
        var additionalApexParticipants = ReadOptionalParticipantsFromCsv("本戦不出場Apex一覧CSVを貼り付けてください。");
        if (!ValidateAdditionalApexParticipants(participants, groupMap, additionalApexParticipants, out errorMessage))
        {
            Console.WriteLine($"本戦不出場Apex一覧の検証に失敗しました: {errorMessage}\n");
            return;
        }

        var additionalApexPlacementMode = ReadAdditionalApexPlacementMode();
        var effectiveAdditionalApexCount = GetEffectiveAdditionalApexCount(additionalApexParticipants.Count, additionalApexPlacementMode);
        var boundaryRescueMode = ReadBoundaryRescueMode();

        var matches = ReadMatchesFromCsv(participants);
        if (!ValidateFinalStageMatches(participants, groupMap, matches, out errorMessage))
        {
            Console.WriteLine($"本戦対局の検証に失敗しました: {errorMessage}\n");
            return;
        }

        CalculationResult result;
        if (matches.Count <= 20)
        {
            Console.WriteLine("品質評価用の厳密計算を行います。\n");
            result = CalculateFinalStageExactly(participants, matches, groupMap, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating);
        }
        else
        {
            const int defaultSimulationCount = 200_000;
            var simulationCount = ReadIntWithDefault(
                $"局数が多いため品質評価用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                defaultSimulationCount,
                min: 1);

            Console.WriteLine();
            result = CalculateFinalStageBySimulation(participants, matches, groupMap, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating, simulationCount);
        }

        var resultRows = BuildResultRows(participants, matches, result, blackAdvantagePercent);
        var qualityParticipantRows = BuildQualityParticipantRows(resultRows, groupMap, additionalApexParticipants, additionalApexPlacementMode);
        var qualitySummary = BuildQualitySummary(qualityParticipantRows);

        Console.WriteLine($"本戦不出場Apexの扱い: {GetAdditionalApexPlacementModeLabel(additionalApexPlacementMode)}\n");
        Console.WriteLine($"境界救済戦: {GetBoundaryRescueModeLabel(boundaryRescueMode)}\n");
        PrintQualitySummary(qualitySummary);
        PrintQualityParticipantHighlights(qualityParticipantRows);

        var reportGroupingOptions = ReadExperimentalReportGroupingOptions();
        var defaultOutputCsvPath = BuildQualitySummaryDefaultOutputPath(additionalApexPlacementMode, boundaryRescueMode, reportGroupingOptions);
        var summaryCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n品質評価サマリーCSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriteQualitySummaryCsv(summaryCsvPath, qualitySummary, reportGroupingOptions);

        var participantCsvPath = BuildSiblingOutputCsvPath(summaryCsvPath, "quality_participants");
        WriteQualityParticipantCsv(participantCsvPath, qualityParticipantRows);

        Console.WriteLine($"品質評価サマリーCSVを出力しました: {summaryCsvPath}");
        Console.WriteLine($"品質評価参加者別CSVを出力しました: {participantCsvPath}");
    }
}
