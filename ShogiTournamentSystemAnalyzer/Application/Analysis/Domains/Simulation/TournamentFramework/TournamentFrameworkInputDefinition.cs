/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

sealed record class TournamentFrameworkInputDefinition(
    string PlayersCsvPath,
    string StagesCsvPath,
    string TournamentMatchRecordsCsvPath,
    string? RuleFilePath,
    int? RandomSeed);
