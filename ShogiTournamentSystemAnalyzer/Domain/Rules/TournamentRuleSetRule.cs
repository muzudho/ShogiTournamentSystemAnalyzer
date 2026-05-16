internal static class TournamentRuleSetRule
{
    internal static TournamentRuleSetMode ReadMode()
    {
        Console.WriteLine("順位ルールを選んでください。");
        Console.WriteLine("1. Neutral: 勝ち数ベースのニュートラル順位");
        Console.WriteLine("2. Twill: 比較グラフと重箱表で順位を決める\n");

        while (true)
        {
            Console.Write("ルール番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return TournamentRuleSetMode.Neutral;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return TournamentRuleSetMode.Twill;
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

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
