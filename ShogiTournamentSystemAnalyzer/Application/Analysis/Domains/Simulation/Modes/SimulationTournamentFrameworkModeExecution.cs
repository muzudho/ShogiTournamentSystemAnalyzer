/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Infrastructure.Parsing;

/// <summary>
/// ［シミュレーション　＞　大会フレームワークモード］の主フロー
/// </summary>
internal static partial class SimulationTournamentFrameworkMode
{
    /// <summary>
    /// ［大会フレームワーク・モード］実行
    /// </summary>
    /// <param name="context"></param>
    static void ExecuteTournamentFrameworkMode(TournamentFrameworkModeContext context)
    {
        // ［選手一覧データ］読込
        var players = TournamentFrameworkCsvParsers.ReadPlayerEntriesFromCsvPath(context.PlayersCsvPath);

        // ［段階マスターデータ］読込
        var stages = TournamentFrameworkCsvParsers.ReadStageEntriesFromCsvPath(context.StagesCsvPath);
        var matchRecords = TournamentFrameworkCsvParsers.ReadTournamentMatchRecordsFromCsvPath(context.TournamentMatchRecordsCsvPath);

        // ［大会ルールＤＳＬ定義］
        TournamentDslDefinition? dslDefinition = null;
        if (!string.IsNullOrWhiteSpace(context.RuleFilePath))
        {
            dslDefinition = TournamentDslDefinitionParser.ParseTournamentDsl(File.ReadAllText(Path.GetFullPath(context.RuleFilePath)), context.RuleFilePath!);
            Console.WriteLine($"大会ルールDSLを読み込みました: {context.RuleFilePath}");
        }

        // ［大会進行フレームワーク実行設定］
        var executionSettings = TournamentFrameworkExecutionSettings.FromContext(context, dslDefinition);

        // ［順位設定データ］

        // ［初回状態］
        var initialState = new TournamentState(0, players, stages, matchRecords);

        // ［順位付けの設定］選択
        IRankingRule rankingRule = executionSettings.TournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => TwillTournamentRankingRule.Instance,
            TournamentRuleSetMode.TwillCommonOpponentWeighted => TwillTournamentRankingRule.CommonOpponentWeightedInstance,
            _ => ByFinishedResultsRankingRule.Instance,
        };

        // ［大会ルールセット］
        var ruleSet = new TournamentFrameworkRuleSet(
            FixedMatchPairingRule.Instance,
            rankingRule,
            AllMatchesFinishedTerminationRule.Instance,
            new StandardLikeMatchResultResolver(executionSettings.FirstPlayerWinRateRating));

        // ［大会エンジン］
        var engine = new TournamentEngine(ruleSet, executionSettings.RandomSeed);

        // ［集計結果］
        var aggregateResult = TournamentFrameworkSimulationCalculator.Execute(engine, initialState, players, executionSettings.TournamentRuleSetMode, executionSettings.FirstPlayerWinRateRating, context.SimulationCount);

        // ［実行結果］
        var executionResult = aggregateResult.RepresentativeExecutionResult;

        // ［最終順位付け結果］
        var finalRankingResult = FinalRankingDomain.BuildTournamentFrameworkFinalRankingResult(
            players,
            stages,
            executionResult.FinalState,
            executionResult.OverallRanking,
            executionResult.TickCount,
            executionResult.CompletedNaturally,
            aggregateResult.PlaceProbabilities,
            aggregateResult.RequestedSimulationCount,
            aggregateResult.CompletedSimulationCount,
            aggregateResult.IsExactCalculation,
            aggregateResult.TournamentRuleSetMode,
            executionSettings.FirstPlayerWinRatePercent);

        Console.WriteLine($"順位ルール: {TournamentRuleSetRule.GetLabel(finalRankingResult.TournamentRuleSetMode)}");

        if (aggregateResult.IsExactCalculation)
        {
            Console.WriteLine("計算種別: 厳密計算");
            Console.WriteLine($"進行Tick数: {aggregateResult.AverageTickCount:F2}");
            Console.WriteLine($"自然終了: {(aggregateResult.CompletedNaturallyCount > 0 ? "Yes" : "No")}");
        }
        else
        {
            Console.WriteLine($"集計試行回数: {aggregateResult.CompletedSimulationCount:N0}");
            Console.WriteLine($"平均進行Tick数: {aggregateResult.AverageTickCount:F2}");
            Console.WriteLine($"自然終了率: {aggregateResult.CompletedNaturallyCount:N0}/{aggregateResult.CompletedSimulationCount:N0}");
        }

        Console.WriteLine($"代表実行Tick数: {finalRankingResult.RepresentativeTournamentFinalState.TickCount}");
        Console.WriteLine($"代表実行の自然終了: {(finalRankingResult.RepresentativeTournamentFinalState.CompletedNaturally ? "Yes" : "No")}");
        Console.WriteLine($"ステージ数: {finalRankingResult.RepresentativeStages.Count}");
        Console.WriteLine($"総対局数: {finalRankingResult.RepresentativeTournamentFinalState.MatchRecords.Count}\n");
        if (executionSettings.DslDefinition is not null)
        {
            Console.WriteLine($"DSL TimeAxis: {executionSettings.DslDefinition.TimeAxis}");
            Console.WriteLine($"DSL OverallRanking: {executionSettings.DslDefinition.OverallRankingRuleName}\n");
        }

        FinalRankingDomain.PrintTournamentFrameworkSimulationResults(finalRankingResult);

        FinalRankingDomain.WriteTournamentFrameworkSimulationOutputs(
            context.OutputPath,
            finalRankingResult);
    }
}
