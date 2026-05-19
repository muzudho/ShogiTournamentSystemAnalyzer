internal static partial class Program
{
    static TournamentFrameworkModeContext ReadTournamentFrameworkModeContext()
    {
        var playersCsvPath = ReadRequiredFilePath("選手一覧CSVのパスを入力してください: ");
        var stagesCsvPath = ReadRequiredFilePath("ステージ一覧CSVのパスを入力してください: ");
        var tournamentMatchRecordsCsvPath = ReadRequiredFilePath("大会対局記録CSVのパスを入力してください: ");
        var ruleFilePath = ReadOptionalFilePath("大会ルールDSLファイルのパスを入力してください（省略可）: ");
        var randomSeed = ReadOptionalInt("乱数シードを入力してください（省略可）: ");
        Console.WriteLine();

        return new TournamentFrameworkModeContext(
            playersCsvPath,
            stagesCsvPath,
            tournamentMatchRecordsCsvPath,
            ruleFilePath,
            randomSeed);
    }
}
