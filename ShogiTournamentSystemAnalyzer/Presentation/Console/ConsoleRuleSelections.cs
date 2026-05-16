internal static partial class Program
{
    static TournamentRuleSetMode ReadTournamentRuleSetMode()
    {
        Console.WriteLine("順位ルールを選んでください。");
        Console.WriteLine("通常ルールでは Neutral / Twill をここで切り替えます。");
        Console.WriteLine("1. Neutral: 勝ち数ベースのニュートラル順位");
        Console.WriteLine("2. Twill: 比較グラフと重箱表で順位を決める\n");
        Console.WriteLine("3. Twill+CommonOpp: 共通相手比較に信頼度を入れたツイル改良案\n");

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

            if (input == "3")
            {
                Console.WriteLine();
                return TournamentRuleSetMode.TwillCommonOpponentWeighted;
            }

            Console.WriteLine("1、2、3 のいずれかを入力してください。\n");
        }
    }

    static FinalStageGroupingMode ReadFinalStageGroupingMode()
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

    static AdditionalApexPlacementMode ReadAdditionalApexPlacementMode()
    {
        Console.WriteLine("本戦不出場Apexの扱いを選んでください。");
        Console.WriteLine("本戦選手とは別にいる Apex を総合順位へどう反映するかを選びます。");
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

    static BoundaryRescueMode ReadBoundaryRescueMode()
    {
        Console.WriteLine("境界救済戦を使いますか？");
        Console.WriteLine("Apex 最下位相当と Innov 最上位相当の入れ替わり余地を補う設定です。");
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

    static VariableTop8Mode ReadVariableTop8Mode()
    {
        Console.WriteLine("可変定員8ルールを使いますか？");
        Console.WriteLine("本戦不出場Apexの人数に応じて Innov 側の総合上位8人数を補正する設定です。");
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
}

