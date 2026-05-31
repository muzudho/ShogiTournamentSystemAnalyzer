/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

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
