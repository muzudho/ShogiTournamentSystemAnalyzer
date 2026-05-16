internal static partial class Program
{
    static StandardModeContext ReadStandardModeContext()
    {
        var tournamentRuleSetMode = ReadTournamentRuleSetMode();
        var blackAdvantagePercent = ReadDoubleWithDefaultInRange("同Elo対局時の黒番勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var blackAdvantageRating = ConvertBlackAdvantagePercentToRating(blackAdvantagePercent);

        Console.WriteLine();
        var allParticipants = ReadPlayersFromCsv();
        var allMatches = ReadMatchesFromCsv(allParticipants);
        var (participants, matches) = FilterToScheduledParticipants(allParticipants, allMatches);

        return new StandardModeContext(
            tournamentRuleSetMode,
            blackAdvantagePercent,
            blackAdvantageRating,
            allParticipants,
            participants,
            matches);
    }
}

