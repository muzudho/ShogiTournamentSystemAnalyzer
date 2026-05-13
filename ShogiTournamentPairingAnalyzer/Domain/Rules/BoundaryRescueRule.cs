internal static class BoundaryRescueRule
{
    internal static BoundaryRescueMode ReadMode()
    {
        Console.WriteLine("境界救済戦を使いますか？");
        Console.WriteLine("1. Off: 使わない");
        Console.WriteLine("2. On: Apex最下位相当とInnov最上位相当で救済戦を行う\n");

        while (true)
        {
            Console.Write("モード番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return BoundaryRescueMode.Off;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return BoundaryRescueMode.On;
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    internal static string GetLabel(BoundaryRescueMode boundaryRescueMode)
    {
        return boundaryRescueMode == BoundaryRescueMode.On
            ? "On（境界救済戦あり）"
            : "Off（境界救済戦なし）";
    }
}
