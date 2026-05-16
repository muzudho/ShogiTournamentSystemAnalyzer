internal static class FinalStageGroupingRule
{
    internal static FinalStageGroupingMode ReadMode()
    {
        Console.WriteLine("Apex / Innov の分け方を使いますか？");
        Console.WriteLine("1. On: Apex / Innov を使う");
        Console.WriteLine("2. Off: ニュートラルに扱う\n");

        while (true)
        {
            Console.Write("モード番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return FinalStageGroupingMode.On;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return FinalStageGroupingMode.Off;
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    internal static string GetLabel(FinalStageGroupingMode mode)
    {
        return mode == FinalStageGroupingMode.On
            ? "On（Apex / Innov を使う）"
            : "Off（ニュートラルに扱う）";
    }
}
