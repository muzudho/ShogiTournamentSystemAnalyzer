/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Application.Helpers;

internal static partial class Program
{
    static StandardModeContext ReadStandardModeContext()
    {
        var tournamentRuleSetMode = ConsoleRuleReaders.ReadTournamentRuleSetMode();
        Console.WriteLine("補足: 空欄のまま Enter すると既定値 51 を使います。\n");
        var firstPlayerWinRatePercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);

        Console.WriteLine();
        var allParticipants = ConsoleInputReaders.ReadPlayersFromCsv();
        var allMatches = ConsoleInputReaders.ReadMatchesFromCsv(allParticipants);
        var (participants, matches) = ModeSupportHelpers.FilterToScheduledParticipants(allParticipants, allMatches);

        return new StandardModeContext(
            tournamentRuleSetMode,
            firstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            allParticipants,
            participants,
            matches);
    }
}

