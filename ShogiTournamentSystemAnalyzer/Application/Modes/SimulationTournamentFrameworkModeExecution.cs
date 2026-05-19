internal static partial class Program
{
    static void ExecuteTournamentFrameworkMode(TournamentFrameworkModeContext context)
    {
        Console.WriteLine("大会進行フレームワークの入口は追加済みですが、既存 Standard の載せ替えは次の段階で行います。\n");
        Console.WriteLine($"- PlayersCsvPath: {context.PlayersCsvPath}");
        Console.WriteLine($"- StagesCsvPath: {context.StagesCsvPath}");
        Console.WriteLine($"- TournamentMatchRecordsCsvPath: {context.TournamentMatchRecordsCsvPath}");
        Console.WriteLine($"- RuleFilePath: {context.RuleFilePath ?? "(なし)"}");
        Console.WriteLine($"- RandomSeed: {(context.RandomSeed?.ToString() ?? "(なし)")}\n");
    }
}
