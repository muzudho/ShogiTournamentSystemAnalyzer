/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

/// <summary>
/// TODO: "TournamentFramework" は［４大域］のいずれかに含めてほしいぜ（＾～＾）
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
