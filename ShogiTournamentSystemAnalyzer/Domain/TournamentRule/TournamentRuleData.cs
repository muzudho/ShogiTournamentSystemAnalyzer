/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

/// <summary>
/// ［６大境界］のうち、［大会ルール］境界データだ。
/// </summary>
/// <param name="RuleProfileMode"></param>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="RuleFilePath"></param>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="RandomSeed"></param>
/// <param name="Note"></param>
sealed record class TournamentRuleData(
    RuleProfileMode RuleProfileMode,
    TournamentRuleSetMode? TournamentRuleSetMode,
    string? RuleFilePath,
    double? FirstPlayerWinRatePercent,
    int? RandomSeed,
    string? Note);
