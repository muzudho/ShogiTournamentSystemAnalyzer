/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
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

        // ［大会ルールセット］
        var ruleSet = TournamentFrameworkRuleSetFactory.Create(
            executionSettings.TournamentRuleSetMode,
            executionSettings.FirstPlayerWinRateRating);

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

        TournamentFrameworkExecutionSummaryPrinter.Print(
            aggregateResult,
            finalRankingResult,
            executionSettings.DslDefinition);

        FinalRankingDomain.PrintTournamentFrameworkSimulationResults(finalRankingResult);

        FinalRankingDomain.WriteTournamentFrameworkSimulationOutputs(
            context.OutputPath,
            finalRankingResult);
    }
}
