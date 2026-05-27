/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes;

using ShogiTournamentSystemAnalyzer.Application.Helpers;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

internal static partial class SimulationModeInputReaders
{
    internal static StandardModeContext ReadStandardModeContext()
    {
        var tournamentRuleSetMode = ConsoleRuleReaders.ReadTournamentRuleSetMode();
        Console.WriteLine("補足: 空欄のまま Enter すると既定値 51 を使います。\n");
        var firstPlayerWinRatePercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);

        Console.WriteLine();
        var allPlayers = ConsoleInputReaders.ReadPlayersFromCsv();
        var allMatches = ConsoleInputReaders.ReadMatchesFromCsv(allPlayers);
        var (players, matches) = ModeSupportHelpers.FilterToScheduledPlayers(allPlayers, allMatches);

        return new StandardModeContext(
            tournamentRuleSetMode,
            firstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            allPlayers,
            players,
            matches);
    }

    internal static TournamentFrameworkModeContext ReadTournamentFrameworkModeContext()
    {
        Console.WriteLine("補足: 空欄のまま Enter すると、先手勝率は既定値 51、試行回数は既定値 200000 を使います。\n");
        var firstPlayerWinRatePercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        var tournamentRuleSetMode = ConsoleRuleReaders.ReadTournamentRuleSetMode();
        Console.WriteLine();

        var playersCsvPath = ConsoleInputReaders.ReadRequiredFilePath("選手一覧CSVのパスを入力してください: ");
        var stagesCsvPath = ConsoleInputReaders.ReadRequiredFilePath("ステージ一覧CSVのパスを入力してください: ");
        var tournamentMatchRecordsCsvPath = ConsoleInputReaders.ReadRequiredFilePath("大会対局記録CSVのパスを入力してください: ");
        var ruleFilePath = ConsoleInputReaders.ReadOptionalFilePath("大会ルールDSLファイルのパスを入力してください（省略可）: ");
        var randomSeed = ConsoleInputReaders.ReadOptionalInt("乱数シードを入力してください（省略可）: ");
        var simulationCount = ConsoleInputReaders.ReadOptionalInt("試行回数を入力してください（省略可）: ");
        var outputPath = ConsoleInputReaders.ReadOptionalFilePath("結果CSVの出力先パスまたはフォルダーパスを入力してください（省略可）: ");
        Console.WriteLine();

        return new TournamentFrameworkModeContext(
            playersCsvPath,
            stagesCsvPath,
            tournamentMatchRecordsCsvPath,
            ruleFilePath,
            randomSeed,
            simulationCount,
            tournamentRuleSetMode,
            firstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            outputPath);
    }
}
