/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

sealed record class TournamentFrameworkRuleSet(
    IPairingRule PairingRule,
    IRankingRule RankingRule,
    ITerminationRule TerminationRule,
    IMatchResultResolver MatchResultResolver);
