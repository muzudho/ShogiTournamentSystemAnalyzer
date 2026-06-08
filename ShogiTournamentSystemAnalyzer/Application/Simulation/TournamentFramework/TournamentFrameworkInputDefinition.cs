/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.TournamentFramework;

/// <summary>
/// 大会進行フレームワークをシミュレーション域で使うための入力ファイル定義。
/// </summary>
/// <param name="PlayersCsvPath"></param>
/// <param name="StagesCsvPath"></param>
/// <param name="TournamentMatchRecordsCsvPath"></param>
/// <param name="RuleFilePath"></param>
/// <param name="RandomSeed"></param>
sealed record class TournamentFrameworkInputDefinition(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed);
