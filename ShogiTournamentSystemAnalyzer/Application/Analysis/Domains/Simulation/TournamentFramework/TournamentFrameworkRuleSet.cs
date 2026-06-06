/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// 大会進行フレームワークのシミュレーションで使う進行ルール一式。
/// </summary>
/// <param name="PairingRule"></param>
/// <param name="RankingRule"></param>
/// <param name="TerminationRule"></param>
/// <param name="MatchResultResolver"></param>
sealed record class TournamentFrameworkRuleSet(
    IPairingRule PairingRule,
    IRankingRule RankingRule,
    ITerminationRule TerminationRule,
    IMatchResultResolver MatchResultResolver);
