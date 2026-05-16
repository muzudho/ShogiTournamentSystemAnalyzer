internal static class VariableTop8Rule
{
    internal static VariableTop8Mode ReadMode()
    {
        Console.WriteLine("可変定員8ルールを使いますか？");
        Console.WriteLine("1. Off: 使わない");
        Console.WriteLine("2. On: 本戦不出場Apex人数ぶんだけ Innov 上位を総合上位8へ引っ張り上げる\n");

        while (true)
        {
            Console.Write("モード番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return VariableTop8Mode.Off;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return VariableTop8Mode.On;
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    internal static int GetPromotedInnovCount(VariableTop8Mode mode, int additionalApexCount)
    {
        return mode == VariableTop8Mode.On ? additionalApexCount : 0;
    }

    internal static string GetLabel(VariableTop8Mode mode)
    {
        return mode == VariableTop8Mode.On
            ? "On（本戦不出場Apex人数ぶん Innov 上位を総合上位8へ引き上げる）"
            : "Off（定員8固定）";
    }
}
