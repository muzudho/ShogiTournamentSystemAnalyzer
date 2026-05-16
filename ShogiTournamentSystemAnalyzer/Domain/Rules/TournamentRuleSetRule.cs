internal static class TournamentRuleSetRule
{
    internal static string GetLabel(TournamentRuleSetMode mode)
    {
        return mode == TournamentRuleSetMode.Twill
            ? "Twill（ツイル式トーナメント）"
            : "Neutral（ニュートラル）";
    }
}

internal enum TournamentRuleSetMode
{
    Neutral,
    Twill,
}
