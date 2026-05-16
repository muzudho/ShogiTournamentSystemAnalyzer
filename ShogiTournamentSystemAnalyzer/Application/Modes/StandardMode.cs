internal static partial class Program
{
    static void RunStandardMode()
    {
        Console.WriteLine("通常モード: 総当たり戦の順位分布を計算します。\n");
        Console.WriteLine("前提: 各対局は黒番・白番を持ち、勝率は Elo レーティング差と黒番有利率から計算します。\n");

        PrintInputSample();
        var tournamentRuleSetMode = TournamentRuleSetRule.ReadMode();
        var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の黒番勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);

        Console.WriteLine();
        var allParticipants = ReadParticipantsFromCsv();
        var allMatches = ReadMatchesFromCsv(allParticipants);
        var (participants, matches) = FilterToScheduledParticipants(allParticipants, allMatches);

        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(tournamentRuleSetMode)}\n");

        if (participants.Count != allParticipants.Count)
        {
            Console.WriteLine($"未対局の選手 {allParticipants.Count - participants.Count} 人を結果から除外します。\n");
        }

        PrintMatchesCsv(participants, matches);

        Console.WriteLine($"\n総対局数: {matches.Count}");

        CalculationResult result;
        if (matches.Count <= 20)
        {
            Console.WriteLine("厳密計算を行います。\n");
            result = CalculateExactly(participants, matches, blackAdvantageRating, tournamentRuleSetMode);
        }
        else
        {
            const int defaultSimulationCount = 200_000;
            var simulationCount = ReadIntWithDefault(
                $"局数が多いためシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
                defaultSimulationCount,
                min: 1);

            Console.WriteLine();
            using var simulationBudget = BeginSimulationBudget();
            result = CalculateBySimulation(participants, matches, blackAdvantageRating, simulationCount, tournamentRuleSetMode);
        }

        var resultRows = BuildResultRows(participants, matches, result, blackAdvantagePercent);
        PrintResult(participants.Count, result, blackAdvantagePercent, resultRows);
        if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var defaultOutputCsvPath = Path.GetFullPath($"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriteResultCsv(outputCsvPath, result.Mode, blackAdvantagePercent, resultRows);
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
    }
}
