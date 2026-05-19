/// <summary>
/// ［選手のランキングの行］だ。
/// </summary>
/// <param name="PlayerId"></param>
/// <param name="Rank"></param>
/// <param name="Points"></param>
/// <param name="Note"></param>
readonly record struct PlayerRankRow(
    int PlayerId,
    int Rank,
    int Points,
    string Note);
