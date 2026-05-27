using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

sealed record class TournamentFrameworkRuleSet(
    IPairingRule PairingRule,
    IRankingRule RankingRule,
    ITerminationRule TerminationRule,
    IMatchResultResolver MatchResultResolver);
