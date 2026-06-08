/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Application.Simulation.TournamentFramework;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// 大会進行フレームワークを実行するための解決済み設定。
/// </summary>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="RuleFilePath"></param>
/// <param name="DslDefinition"></param>
/// <param name="RandomSeed"></param>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="FirstPlayerWinRateRating"></param>
internal readonly record struct TournamentFrameworkExecutionSettings(
    TournamentRuleSetMode TournamentRuleSetMode,
    string? RuleFilePath,
    TournamentDslDefinition? DslDefinition,
    int? RandomSeed,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating)
{
    internal static TournamentFrameworkExecutionSettings FromContext(
        TournamentFrameworkModeContext context,
        TournamentDslDefinition? dslDefinition)
    {
        return new TournamentFrameworkExecutionSettings(
            context.TournamentRuleSetMode,
            context.RuleFilePath,
            dslDefinition,
            context.RandomSeed,
            context.FirstPlayerWinRatePercent,
            context.FirstPlayerWinRateRating);
    }
}