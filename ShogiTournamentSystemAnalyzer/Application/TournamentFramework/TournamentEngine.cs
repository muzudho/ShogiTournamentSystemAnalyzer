sealed class TournamentEngine
{
    readonly TournamentFrameworkRuleSet _ruleSet;
    readonly TournamentTickRunner _tickRunner;
    readonly int _maxTicks;

    internal TournamentEngine(TournamentFrameworkRuleSet ruleSet, int? randomSeed = null, int maxTicks = 10_000)
    {
        _ruleSet = ruleSet;
        _tickRunner = new TournamentTickRunner(ruleSet, randomSeed);
        _maxTicks = maxTicks;
    }

    internal TournamentFrameworkExecutionResult Run(TournamentState initialState)
    {
        var state = initialState;
        var tickCount = 0;
        while (!_ruleSet.TerminationRule.ShouldFinish(state) && tickCount < _maxTicks)
        {
            state = _tickRunner.RunTick(state);
            tickCount++;
        }

        var ranking = _ruleSet.RankingRule.Rank(state, stageId: null);
        return new TournamentFrameworkExecutionResult(
            state,
            ranking,
            tickCount,
            _ruleSet.TerminationRule.ShouldFinish(state));
    }
}
