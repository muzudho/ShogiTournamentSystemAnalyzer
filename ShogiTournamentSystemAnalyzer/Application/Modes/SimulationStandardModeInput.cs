/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

internal static partial class Program
{
    static StandardModeContext ReadStandardModeContext()
    {
        var tournamentRuleSetMode = ReadTournamentRuleSetMode();
        Console.WriteLine("補足: 空欄のまま Enter すると既定値 51 を使います。\n");
        var firstPlayerWinRatePercent = ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var firstPlayerWinRateRating = ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);

        Console.WriteLine();
        var allParticipants = ReadPlayersFromCsv();
        var allMatches = ReadMatchesFromCsv(allParticipants);
        var (participants, matches) = FilterToScheduledParticipants(allParticipants, allMatches);

        return new StandardModeContext(
            tournamentRuleSetMode,
            firstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            allParticipants,
            participants,
            matches);
    }
}

