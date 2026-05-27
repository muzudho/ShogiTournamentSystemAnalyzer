/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes;

using ShogiTournamentSystemAnalyzer.Application.Helpers;
using ShogiTournamentSystemAnalyzer.Application.Modes.SimulationContext;
using ShogiTournamentSystemAnalyzer.Application.Validation;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static partial class SimulationModeInputReaders
{
    internal static bool TryReadFinalStageModeContext(out FinalStageModeSimulationContext context)
    {
        Console.WriteLine("補足: 空欄のまま Enter すると既定値 51 を使います。\n");
        var firstPlayerWinRatePercent = ConsolePromptReaders.ReadDoubleWithDefaultInRange("同Elo対局時の先手勝率(%)を入力してください [51]: ", 51.0, 0.0, 100.0);
        var firstPlayerWinRateRating = SimulationRatingMath.ConvertFirstPlayerWinRatePercentToRating(firstPlayerWinRatePercent);
        Console.WriteLine();

        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        var groupingMode = FinalStageGroupingMode.On;
        var tournamentRuleSetMode = TournamentRuleSetMode.Neutral;
        var groupMap = ModeSupportHelpers.ReadOptionalFinalStageGroupMap(groupingMode, players);
        string errorMessage;
        var playersAreValid = FinalStageValidators.ValidateFinalStagePlayers(players, groupMap!, out errorMessage);
        if (!playersAreValid)
        {
            Console.WriteLine($"本戦参加者の検証に失敗しました: {errorMessage}\n");
            context = default;
            return false;
        }

        List<Player> additionalApexPlayers;
        var additionalApexPlacementMode = AdditionalApexPlacementMode.Off;
        var effectiveAdditionalApexCount = 0;
        var boundaryRescueMode = BoundaryRescueMode.Off;
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine();
            additionalApexPlayers = ConsoleInputReaders.ReadOptionalPlayersFromCsv("本戦不出場Apex一覧CSVを貼り付けてください。");
            if (!FinalStageValidators.ValidateAdditionalApexPlayers(players, groupMap!, additionalApexPlayers, out errorMessage))
            {
                Console.WriteLine($"本戦不出場Apex一覧の検証に失敗しました: {errorMessage}\n");
                context = default;
                return false;
            }

            additionalApexPlacementMode = ConsoleRuleReaders.ReadAdditionalApexPlacementMode();
            effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexPlayers.Count, additionalApexPlacementMode);
            boundaryRescueMode = ConsoleRuleReaders.ReadBoundaryRescueMode();
        }
        else
        {
            additionalApexPlayers = new List<Player>();
        }

        var apexCount = groupMap?.Count(x => x.Value == FinalStageGroup.Apex) ?? 0;
        var innovCount = groupMap?.Count - apexCount ?? players.Count;

        Console.WriteLine("本戦参加者の入力を受け付けました。\n");

        var matches = ConsoleInputReaders.ReadMatchesFromCsv(players);
        var matchesAreValid = FinalStageValidators.ValidateFinalStageMatches(players, groupMap!, matches, out errorMessage);
        if (!matchesAreValid)
        {
            Console.WriteLine($"本戦対局の検証に失敗しました: {errorMessage}\n");
            context = default;
            return false;
        }

        Console.WriteLine();
        var referenceMatches = ConsoleInputReaders.ReadOptionalMatchesFromCsv(players, "参考対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。大会記録に含めない場合だけ使います。");

        context = new FinalStageModeSimulationContext(
            firstPlayerWinRatePercent,
            firstPlayerWinRateRating,
            players,
            groupingMode,
            tournamentRuleSetMode,
            groupMap,
            additionalApexPlayers,
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

