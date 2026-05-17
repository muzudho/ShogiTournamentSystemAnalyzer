internal static partial class Program
{
    static bool TryReadFinalStageModeContext(out FinalStageModeContext context)
    {
        Console.WriteLine("補足: 空欄のまま Enter すると既定値 51 を使います。\n");
        var firstPlayerWinRatePercent = ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var firstPlayerWinRateRating = ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        Console.WriteLine();

        var participants = ReadPlayersFromCsv();
        Console.WriteLine();

        var groupingMode = FinalStageGroupingMode.On;
        var tournamentRuleSetMode = TournamentRuleSetMode.Neutral;
        var groupMap = ReadOptionalFinalStageGroupMap(groupingMode, participants);
        string errorMessage;
        var participantsAreValid = ValidateFinalStageParticipants(participants, groupMap!, out errorMessage);
        if (!participantsAreValid)
        {
            Console.WriteLine($"本戦参加者の検証に失敗しました: {errorMessage}\n");
            context = default;
            return false;
        }

        List<Player> additionalApexParticipants;
        var additionalApexPlacementMode = AdditionalApexPlacementMode.Off;
        var effectiveAdditionalApexCount = 0;
        var boundaryRescueMode = BoundaryRescueMode.Off;
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine();
            additionalApexParticipants = ReadOptionalPlayersFromCsv("本戦不出場Apex一覧CSVを貼り付けてください。");
            if (!ValidateAdditionalApexParticipants(participants, groupMap!, additionalApexParticipants, out errorMessage))
            {
                Console.WriteLine($"本戦不出場Apex一覧の検証に失敗しました: {errorMessage}\n");
                context = default;
                return false;
            }

            additionalApexPlacementMode = ReadAdditionalApexPlacementMode();
            effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexParticipants.Count, additionalApexPlacementMode);
            boundaryRescueMode = ReadBoundaryRescueMode();
        }
        else
        {
            additionalApexParticipants = new List<Player>();
        }

        var apexCount = groupMap?.Count(x => x.Value == FinalStageGroup.Apex) ?? 0;
        var innovCount = groupMap?.Count - apexCount ?? participants.Count;

        Console.WriteLine("本戦参加者の入力を受け付けました。\n");

        var matches = ReadMatchesFromCsv(participants);
        var matchesAreValid = ValidateFinalStageMatches(participants, groupMap!, matches, out errorMessage);
        if (!matchesAreValid)
        {
            Console.WriteLine($"本戦対局の検証に失敗しました: {errorMessage}\n");
            context = default;
            return false;
        }

        Console.WriteLine();
        var referenceMatches = ReadOptionalMatchesFromCsv(participants, "参考対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。大会記録に含めない場合だけ使います。");

        context = new FinalStageModeContext(
            firstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            participants,
            groupingMode,
            tournamentRuleSetMode,
            groupMap,
            additionalApexParticipants,
            additionalApexPlacementMode,
            effectiveAdditionalApexCount,
            boundaryRescueMode,
            apexCount,
            innovCount,
            matches,
            referenceMatches);
        return true;
    }
}

