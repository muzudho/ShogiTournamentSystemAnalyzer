/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

sealed record class TournamentFrameworkRuleSet(
    IPairingRule PairingRule,
    IRankingRule RankingRule,
    ITerminationRule TerminationRule,
    IMatchResultResolver MatchResultResolver);
