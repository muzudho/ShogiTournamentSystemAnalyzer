/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
using ShogiTournamentSystemAnalyzer.Application.Helpers;
using ShogiTournamentSystemAnalyzer.Application.Validation;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static partial class Program
{
    static void RunTournamentQualityEvaluationMode(RuleProfileMode ruleProfileMode)
    {
        if (ruleProfileMode == RuleProfileMode.Standard)
        {
            Console.WriteLine("品質評価 / 通常ルール: 総当たり戦向けルールの実力反映性を評価します。\n");
            ConsoleSamplePrinter.PrintQualityEvaluationStandardOverview();
        }
        else
        {
            Console.WriteLine("品質評価 / 本戦ルール: 本戦ルールの実力反映性を評価します。\n");
            ConsoleSamplePrinter.PrintQualityEvaluationFinalStageOverview();
        }

        var players = ConsoleInputReaders.ReadPlayersFromCsv();
        Console.WriteLine();

        if (!TryReadQualityEvaluationRuleDefinition(players, ruleProfileMode, out var ruleDefinition)) return;

        if (!TryReadQualityEvaluationInput(players, ruleDefinition, out var input)) return;


        var executionOptions = ReadTournamentQualityEvaluationExecutionOptions(input, ruleDefinition);
        TournamentQualityEvaluationOutputCoordinator.PrintTournamentQualityEvaluationContext(input, ruleDefinition);

        if (executionOptions.IsSweep)
        {
            RunTournamentQualitySweepExperiment(
                input,
                ruleDefinition,
                executionOptions);
            return;
        }

        var tournamentQualityReportData = ExecuteTournamentQualityReport(
            input,
            ruleDefinition,
            executionOptions);

        ConsoleResultPrinter.PrintTournamentQualityReportSummary(tournamentQualityReportData);
        ConsoleResultPrinter.PrintTournamentQualityReportPlayerHighlights(tournamentQualityReportData);
        if (tournamentQualityReportData.CalculationMode.Contains("時間切れ", StringComparison.Ordinal))
        {
            Console.WriteLine($"シミュレーションは時間上限 {SimulationTimeBudget.SimulationTimeLimit.TotalMinutes:F0} 分で打ち切りました。\n");
        }

        var outputOptions = TournamentQualityEvaluationOutputCoordinator.ReadTournamentQualityReportOutputOptions(ruleDefinition);
        TournamentQualityEvaluationOutputCoordinator.WriteTournamentQualityReportOutputs(tournamentQualityReportData, outputOptions);
    }

    static bool TryReadQualityEvaluationRuleDefinition(
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

    static bool TryReadQualityEvaluationInput(
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
        var referenceMatches = ConsoleInputReaders.ReadOptionalMatchesFromCsv(players, "参考対局CSVまたは Round/Black-White/対局記号表を貼り付けてください。大会記録に含めない場合だけ使います。");
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

