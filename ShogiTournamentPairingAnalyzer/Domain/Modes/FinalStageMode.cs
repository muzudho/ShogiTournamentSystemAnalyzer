internal static partial class Program
{
    static void RunFinalStageMode()
    {
        Console.WriteLine("本戦専用モード: Apex / Innov 定先戦を分析します。\n");

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

        var apexCount = groupMap.Count(x => x.Value == FinalStageGroup.Apex);
        var innovCount = groupMap.Count - apexCount;

        Console.WriteLine("本戦参加者の入力を受け付けました。\n");

        var matches = ReadMatchesFromCsv(participants);
        if (!ValidateFinalStageMatches(participants, groupMap, matches, out errorMessage))
        {
            Console.WriteLine($"本戦対局の検証に失敗しました: {errorMessage}\n");
            return;
        }

        Console.WriteLine($"Apex: {apexCount} 名");
        Console.WriteLine($"Innov: {innovCount} 名\n");
        Console.WriteLine($"本戦不出場Apex: {additionalApexParticipants.Count} 名\n");
        Console.WriteLine($"本戦不出場Apexの扱い: {GetAdditionalApexPlacementModeLabel(additionalApexPlacementMode)}\n");
        Console.WriteLine($"境界救済戦: {GetBoundaryRescueModeLabel(boundaryRescueMode)}\n");

        PrintMatchesCsv(participants, matches);
        Console.WriteLine($"本戦対局数: {matches.Count}\n");

        CalculationResult result;
        if (matches.Count <= 20)
        {
            Console.WriteLine("本戦専用の厳密計算を行います。\n");
            result = CalculateFinalStageExactly(participants, matches, groupMap, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating);
        }
        else
        {
            const int defaultSimulationCount = 200_000;
            var simulationCount = ReadIntWithDefault(
                $"局数が多いため本戦専用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                defaultSimulationCount,
                min: 1);

            Console.WriteLine();
            result = CalculateFinalStageBySimulation(participants, matches, groupMap, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating, simulationCount);
        }

        var resultRows = BuildFinalStageResultRows(participants, matches, result, blackAdvantagePercent, groupMap, effectiveAdditionalApexCount);
        PrintFinalStageResult(result, blackAdvantagePercent, resultRows);

        var defaultOutputCsvPath = Path.GetFullPath($"final_stage_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriteFinalStageResultCsv(outputCsvPath, result.Mode, blackAdvantagePercent, resultRows);
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
    }
}
