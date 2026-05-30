/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Modes;

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
