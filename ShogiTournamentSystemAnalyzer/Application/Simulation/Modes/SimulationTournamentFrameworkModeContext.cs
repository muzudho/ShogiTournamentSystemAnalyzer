/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// 大会進行フレームワークをシミュレーション域で実行するための入力文脈。
/// </summary>
/// <param name="PlayersCsvPath"></param>
/// <param name="StagesCsvPath"></param>
/// <param name="TournamentMatchRecordsCsvPath"></param>
/// <param name="RuleFilePath"></param>
/// <param name="RandomSeed"></param>
/// <param name="SimulationCount"></param>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="FirstPlayerWinRateRating"></param>
/// <param name="OutputPath"></param>
internal readonly record struct TournamentFrameworkModeContext(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed,
    int? SimulationCount,
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent,
    double FirstPlayerWinRateRating,
    string? OutputPath);
