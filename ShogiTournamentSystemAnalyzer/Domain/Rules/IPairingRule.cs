interface IPairingRule
{
    TournamentState Apply(TournamentState state, int stageId);
}
