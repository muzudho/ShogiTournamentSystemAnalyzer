/// <summary>
/// ［６大境界］のうち、［順位付けの設定］境界データだ。
/// </summary>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="IsIntermediate"></param>
/// <param name="Note"></param>
sealed record class RankingSettingsData(
    TournamentRuleSetMode TournamentRuleSetMode,
    bool IsIntermediate,
    string? Note);
