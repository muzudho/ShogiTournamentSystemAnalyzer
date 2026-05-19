internal static partial class Program
{
    static TournamentFrameworkModeContext ReadTournamentFrameworkModeContext()
    {
        Console.WriteLine("補足: 空欄のまま Enter すると既定値 51 を使います。\n");
        var firstPlayerWinRatePercent = ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var firstPlayerWinRateRating = ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        Console.WriteLine();

        var playersCsvPath = ReadRequiredFilePath("選手一覧CSVのパスを入力してください: ");
        var stagesCsvPath = ReadRequiredFilePath("ステージ一覧CSVのパスを入力してください: ");
        var tournamentMatchRecordsCsvPath = ReadRequiredFilePath("大会対局記録CSVのパスを入力してください: ");
        var ruleFilePath = ReadOptionalFilePath("大会ルールDSLファイルのパスを入力してください（省略可）: ");
        var randomSeed = ReadOptionalInt("乱数シードを入力してください（省略可）: ");
        var outputPath = ReadOptionalFilePath("結果CSVの出力先パスまたはフォルダーパスを入力してください（省略可）: ");
        Console.WriteLine();

        return new TournamentFrameworkModeContext(
            playersCsvPath,
            stagesCsvPath,
            tournamentMatchRecordsCsvPath,
            ruleFilePath,
            randomSeed,
            firstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            outputPath);
    }
}
