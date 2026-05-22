/// <summary>
/// ［６大境界］のうち、［プレイヤー一覧］境界データだ。
/// </summary>
/// <param name="Players"></param>
sealed record class PlayerListData(
    IReadOnlyList<PlayerEntry> Players);
