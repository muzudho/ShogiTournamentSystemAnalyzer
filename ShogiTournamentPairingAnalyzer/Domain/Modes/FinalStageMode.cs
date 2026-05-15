internal static partial class Program
{
    static void RunFinalStageMode()
    {
        Console.WriteLine("本戦専用モード: Apex / Innov 分割またはニュートラルな対局表を分析します。\n");

        PrintFinalStageInputSample();

        var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);
        Console.WriteLine();

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
        }
        else
        {
            additionalApexParticipants = new List<Participant>();
        }

        var apexCount = groupMap?.Count(x => x.Value == FinalStageGroup.Apex) ?? 0;
        var innovCount = groupMap?.Count - apexCount ?? participants.Count;

        Console.WriteLine("本戦参加者の入力を受け付けました。\n");

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

        Console.WriteLine($"Apex / Innov の分け方: {FinalStageGroupingRule.GetLabel(groupingMode)}\n");
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine($"Apex: {apexCount} 名");
            Console.WriteLine($"Innov: {innovCount} 名\n");
            Console.WriteLine($"本戦不出場Apex: {additionalApexParticipants.Count} 名\n");
            Console.WriteLine($"本戦不出場Apexの扱い: {AdditionalApexPlacementRule.GetLabel(additionalApexPlacementMode)}\n");
            Console.WriteLine($"境界救済戦: {BoundaryRescueRule.GetLabel(boundaryRescueMode)}\n");
        }
        else
        {
            Console.WriteLine($"対局者数: {participants.Count} 名\n");
        }

        PrintMatchesCsv(participants, matches);
        Console.WriteLine($"本戦対局数: {matches.Count}\n");
        if (referenceMatches.Count > 0)
        {
            PrintMatchesCsv(participants, referenceMatches, "参考対局CSV:");
            Console.WriteLine($"参考対局数: {referenceMatches.Count}");
            Console.WriteLine("参考対局は順位計算に含めません。\n");
        }

        CalculationResult result;
        IReadOnlyList<ResultRow>? standardResultRows = null;
        IReadOnlyList<FinalStageResultRow>? finalStageResultRows = null;
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            if (matches.Count <= 20)
            {
                Console.WriteLine("ニュートラルな厳密計算を行います。\n");
                result = CalculateExactly(participants, matches, blackAdvantageRating);
            }
            else
            {
                const int defaultSimulationCount = 200_000;
                var simulationCount = ReadIntWithDefault(
                    $"局数が多いためニュートラルなシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                    defaultSimulationCount,
                    min: 1);

                Console.WriteLine();
                result = CalculateBySimulation(participants, matches, blackAdvantageRating, simulationCount);
            }

            standardResultRows = BuildResultRows(participants, matches, result, blackAdvantagePercent);
            PrintResult(participants.Count, result, blackAdvantagePercent, standardResultRows);
        }
        else if (matches.Count <= 20)
        {
            Console.WriteLine("本戦専用の厳密計算を行います。\n");
            result = CalculateFinalStageExactly(participants, matches, groupMap!, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating);

            finalStageResultRows = BuildFinalStageResultRows(participants, matches, result, blackAdvantagePercent, groupMap!, effectiveAdditionalApexCount);
            PrintFinalStageResult(result, blackAdvantagePercent, finalStageResultRows);
        }
        else
        {
            const int defaultSimulationCount = 200_000;
            var simulationCount = ReadIntWithDefault(
                $"局数が多いため本戦専用シミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                defaultSimulationCount,
                min: 1);

            Console.WriteLine();
            result = CalculateFinalStageBySimulation(participants, matches, groupMap!, effectiveAdditionalApexCount, boundaryRescueMode, blackAdvantageRating, simulationCount);

            finalStageResultRows = BuildFinalStageResultRows(participants, matches, result, blackAdvantagePercent, groupMap!, effectiveAdditionalApexCount);
            PrintFinalStageResult(result, blackAdvantagePercent, finalStageResultRows);
        }

        var defaultOutputCsvPath = Path.GetFullPath($"final_stage_result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        if (groupingMode == FinalStageGroupingMode.On)
        {
            WriteFinalStageResultCsv(outputCsvPath, result.Mode, blackAdvantagePercent, finalStageResultRows!);
        }
        else
        {
            WriteResultCsv(outputCsvPath, result.Mode, blackAdvantagePercent, standardResultRows!);
        }
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");

        if (referenceMatches.Count > 0)
        {
            var referenceMatchesCsvPath = BuildSiblingOutputCsvPath(outputCsvPath, "reference_matches");
            WriteReferenceMatchCsv(referenceMatchesCsvPath, participants, referenceMatches);
            Console.WriteLine($"参考対局CSVを出力しました: {referenceMatchesCsvPath}");
        }
    }
}
