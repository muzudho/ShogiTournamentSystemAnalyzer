/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.Modes;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries.Request;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class TournamentQualityEvaluationInputReader
{
    internal static bool TryReadQualityEvaluationRuleDefinition(
        IReadOnlyList<Player> players,
        RuleProfileMode ruleProfileMode,
        out TournamentQualityEvaluationRuleDefinition ruleDefinition)
    {
        var groupingMode = ruleProfileMode == RuleProfileMode.FinalStage
            ? FinalStageGroupingMode.On
            : FinalStageGroupingMode.Off;
        var tournamentRuleSetMode = ruleProfileMode == RuleProfileMode.Standard
            ? ConsoleRuleReaders.ReadTournamentRuleSetMode()
            : TournamentRuleSetMode.Neutral;
        var groupMap = ruleProfileMode == RuleProfileMode.FinalStage
            ? ModeSupportHelpers.ReadOptionalFinalStageGroupMap(groupingMode, players)
            : null;

        var playersAreValid = groupingMode == FinalStageGroupingMode.On
            ? FinalStageValidators.ValidateFinalStagePlayers(players, groupMap!, out var errorMessage)
            : FinalStageValidators.ValidateFinalStagePlayers(players, out errorMessage);
        if (!playersAreValid)
        {
            var targetLabel = groupingMode == FinalStageGroupingMode.On ? "本戦選手" : "選手一覧";
            Console.WriteLine($"{targetLabel}の検証に失敗しました: {errorMessage}\n");
            ruleDefinition = default;
            return false;
        }

        List<Player> additionalApexPlayers;
        var additionalApexPlacementMode = AdditionalApexPlacementMode.Off;
        var effectiveAdditionalApexCount = 0;
        var boundaryRescueMode = BoundaryRescueMode.Off;
        var variableTop8Mode = VariableTop8Mode.Off;
        var promotedInnovCount = 0;
        if (groupingMode == FinalStageGroupingMode.On)
        {
            Console.WriteLine();
            additionalApexPlayers = ConsoleInputReaders.ReadOptionalPlayersFromCsv("本戦不出場Apex一覧CSVを貼り付けてください。");
            if (!FinalStageValidators.ValidateAdditionalApexPlayers(players, groupMap!, additionalApexPlayers, out errorMessage))
            {
                Console.WriteLine($"本戦不出場Apex一覧の検証に失敗しました: {errorMessage}\n");
                ruleDefinition = default;
                return false;
            }

            additionalApexPlacementMode = ConsoleRuleReaders.ReadAdditionalApexPlacementMode();
            effectiveAdditionalApexCount = AdditionalApexPlacementRule.GetEffectiveAdditionalApexCount(additionalApexPlayers.Count, additionalApexPlacementMode);
            boundaryRescueMode = ConsoleRuleReaders.ReadBoundaryRescueMode();
            variableTop8Mode = ConsoleRuleReaders.ReadVariableTop8Mode();
            promotedInnovCount = VariableTop8Rule.GetPromotedInnovCount(variableTop8Mode, additionalApexPlayers.Count);
        }
        else
        {
            additionalApexPlayers = new List<Player>();
        }

        ruleDefinition = new TournamentQualityEvaluationRuleDefinition(
            groupingMode,
            tournamentRuleSetMode,
            groupMap,
            additionalApexPlayers,
            additionalApexPlacementMode,
            effectiveAdditionalApexCount,
            boundaryRescueMode,
            variableTop8Mode,
            promotedInnovCount);
        return true;
    }

    internal static bool TryReadQualityEvaluationInput(
        IReadOnlyList<Player> players,
        TournamentQualityEvaluationRuleDefinition ruleDefinition,
        out TournamentQualityEvaluationInput input)
    {
        var matches = ConsoleInputReaders.ReadMatchesFromCsv(players);
        var matchesAreValid = ruleDefinition.UsesFinalStageGrouping
            ? FinalStageValidators.ValidateFinalStageMatches(players, ruleDefinition.GroupMap!, matches, out var errorMessage)
            : FinalStageValidators.ValidateFinalStageMatches(players, matches, out errorMessage);
        if (!matchesAreValid)
        {
            var matchLabel = ruleDefinition.UsesFinalStageGrouping ? "本戦対局" : "対局";
            Console.WriteLine($"{matchLabel}の検証に失敗しました: {errorMessage}\n");
            input = default;
            return false;
        }

        Console.WriteLine();
        var referenceMatches = ConsoleInputReaders.ReadOptionalMatchesFromCsv(players, "参考対局CSVまたは Round/FirstPlayer-SecondPlayer/対局記号表を貼り付けてください。大会記録に含めない場合だけ使います。");
        var innovExpectedRankOffsetMode = TournamentQualityEvaluationInnovExpectedRankOffsetMode.Off;
        var innovExpectedRankOffsetCount = 0;
        if (ruleDefinition.UsesFinalStageGrouping)
        {
            innovExpectedRankOffsetMode = ConsoleRuleReaders.ReadTournamentQualityEvaluationInnovExpectedRankOffsetMode();
            innovExpectedRankOffsetCount = TournamentQualityEvaluationInnovExpectedRankOffsetRule.GetComparisonRankOffset(
                ruleDefinition.EffectiveAdditionalApexCount,
                innovExpectedRankOffsetMode);
        }

        input = new TournamentQualityEvaluationInput(
            players,
            matches,
            referenceMatches,
            innovExpectedRankOffsetMode,
            innovExpectedRankOffsetCount);
        return true;
    }
}
