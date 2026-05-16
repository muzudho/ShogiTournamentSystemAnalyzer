internal static partial class Program
{
    static void ExecuteStandardMode(StandardModeContext context)
    {
        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(context.TournamentRuleSetMode)}\n");

        if (context.ExcludedParticipantCount > 0)
        {
            Console.WriteLine($"未対局の選手 {context.ExcludedParticipantCount} 人を結果から除外します。\n");
        }

        PrintMatchesCsv(context.Participants, context.Matches);
        Console.WriteLine($"\n総対局数: {context.Matches.Count}");

        var result = ExecuteStandardModeCalculation(context);
        var resultRows = BuildResultRows(context.Participants, context.Matches, result, context.BlackAdvantagePercent);
        PrintResult(context.Participants.Count, result, context.BlackAdvantagePercent, resultRows);
        if (result.Mode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var defaultOutputCsvPath = Path.GetFullPath($"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var outputCsvPath = ResolveOutputCsvPath(ReadTextWithDefault(
            $"\n結果CSVの出力先パスまたはフォルダーパスを入力してください [{defaultOutputCsvPath}]: ",
            defaultOutputCsvPath));
        WriteResultCsv(outputCsvPath, result.Mode, context.BlackAdvantagePercent, resultRows);
        Console.WriteLine($"結果CSVを出力しました: {outputCsvPath}");
    }

    static CalculationResult ExecuteStandardModeCalculation(StandardModeContext context)
    {
        if (context.Matches.Count <= 20)
        {
            Console.WriteLine("厳密計算を行います。\n");
            return CalculateExactly(context.Participants, context.Matches, context.BlackAdvantageRating, context.TournamentRuleSetMode);
        }

        const int defaultSimulationCount = 200_000;
        var simulationCount = ReadIntWithDefault(
            $"局数が多いためシミュレーションで近似します。試行回数を入力してください [{defaultSimulationCount}]: ",
            defaultSimulationCount,
            min: 1);

        Console.WriteLine();
        using var simulationBudget = BeginSimulationBudget();
        return CalculateBySimulation(context.Participants, context.Matches, context.BlackAdvantageRating, simulationCount, context.TournamentRuleSetMode);
    }
}

