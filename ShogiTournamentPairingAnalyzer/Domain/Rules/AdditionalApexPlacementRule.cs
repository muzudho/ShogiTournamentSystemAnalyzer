internal static class AdditionalApexPlacementRule
{
    internal static AdditionalApexPlacementMode ReadMode()
    {
        Console.WriteLine("本戦不出場Apexの扱いを選んでください。");
        Console.WriteLine("1. Off: Innov より前に順位帯を確保する（現行案）");
        Console.WriteLine("2. On: 総合順位へ挿入しない（改善案A）\n");

        while (true)
        {
            Console.Write("モード番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input == "1")
            {
                Console.WriteLine();
                return AdditionalApexPlacementMode.Off;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return AdditionalApexPlacementMode.On;
            }

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }

    internal static int GetEffectiveAdditionalApexCount(int additionalApexCount, AdditionalApexPlacementMode placementMode)
    {
        return placementMode == AdditionalApexPlacementMode.On ? 0 : additionalApexCount;
    }

    internal static string GetLabel(AdditionalApexPlacementMode placementMode)
    {
        return placementMode == AdditionalApexPlacementMode.On
            ? "On（改善案A: 総合順位へ挿入しない）"
            : "Off（現行案: Innov より前に順位帯を確保する）";
    }
}
