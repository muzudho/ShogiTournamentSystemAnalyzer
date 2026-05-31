/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

sealed class TournamentTickRunner
{
    readonly TournamentFrameworkRuleSet _ruleSet;
    readonly Random _random;

    internal TournamentTickRunner(TournamentFrameworkRuleSet ruleSet, int? randomSeed = null)
    {
        _ruleSet = ruleSet;
        _random = randomSeed is null ? new Random() : new Random(randomSeed.Value);
    }

    internal TournamentState RunTick(TournamentState state)
    {
        var pairedState = ApplyPairing(state);
        var updatedMatches = pairedState.MatchRecords
            .Select(match => AdvanceMatch(pairedState, match))
            .ToArray();

        return pairedState with
        {
            CurrentTime = pairedState.CurrentTime + 1,
            MatchRecords = updatedMatches,
        };
    }

    TournamentState ApplyPairing(TournamentState state)
    {
        var stageIds = state.Stages.Select(stage => stage.StageId).Distinct().ToArray();
        var currentState = state;
        foreach (var stageId in stageIds)
        {
            currentState = _ruleSet.PairingRule.Apply(currentState, stageId);
        }

        return currentState;
    }

    TournamentMatchRecord AdvanceMatch(TournamentState state, TournamentMatchRecord match)
    {
        if (match.Status is MatchStatus.Finished or MatchStatus.Cancelled) return match;

        if (state.CurrentTime < match.StartTime) return match with { Status = MatchStatus.Scheduled };

        if (state.CurrentTime < match.EndTime) return match with { Status = MatchStatus.Running };

        var resolvedMatch = _ruleSet.MatchResultResolver.Resolve(state, match, _random);
        return resolvedMatch with { Status = MatchStatus.Finished };
    }
}
