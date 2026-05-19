/// <summary>
/// ［選手エントリー］だ。
/// </summary>
/// <param name="PlayerId"></param>
/// <param name="Name"></param>
/// <param name="Rating"></param>
readonly record struct PlayerEntry(
    int PlayerId,
    string Name,
    double Rating);

/// <summary>
/// ［ステージエントリー］だ。
/// </summary>
/// <param name="StageId"></param>
/// <param name="StageName"></param>
/// <param name="StageType"></param>
/// <param name="ParentStageId"></param>
/// <param name="OrderNo"></param>
readonly record struct StageEntry(
    int StageId,
    string StageName,
    string StageType,
    int? ParentStageId,
    int OrderNo);

/// <summary>
/// ［対局レコード］だ。
/// </summary>
/// <param name="MatchId"></param>
/// <param name="StageId"></param>
/// <param name="FirstPlayerId"></param>
/// <param name="SecondPlayerId"></param>
/// <param name="StartTime"></param>
/// <param name="EndTime"></param>
/// <param name="Status"></param>
/// <param name="ResultType"></param>
/// <param name="RoundNo"></param>
readonly record struct TournamentMatchRecord(
    int MatchId,
    int StageId,
    int FirstPlayerId,
    int SecondPlayerId,
    int StartTime,
    int EndTime,
    MatchStatus Status,
    MatchResultType ResultType,
    int? RoundNo);

/// <summary>
/// ［対局状態］だ。
/// </summary>
/// <param name="CurrentTime"></param>
/// <param name="Players"></param>
/// <param name="Stages"></param>
/// <param name="MatchRecords"></param>
sealed record class TournamentState(
    int CurrentTime,
    IReadOnlyList<PlayerEntry> Players,
    IReadOnlyList<StageEntry> Stages,
    IReadOnlyList<TournamentMatchRecord> MatchRecords);
