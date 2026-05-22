/// <summary>
/// ［６大境界］のうち、［大会ルール］境界データだ。
/// </summary>
/// <param name="RuleProfileMode"></param>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="RuleFilePath"></param>
/// <param name="FirstPlayerWinRatePercent"></param>
/// <param name="RandomSeed"></param>
/// <param name="Note"></param>
sealed record class TournamentRuleData(
    RuleProfileMode RuleProfileMode,
    TournamentRuleSetMode? TournamentRuleSetMode,
    string? RuleFilePath,
    double? FirstPlayerWinRatePercent,
    int? RandomSeed,
    string? Note);

/// <summary>
/// ［６大境界］のうち、［プレイヤー一覧］境界データだ。
/// </summary>
/// <param name="Players"></param>
sealed record class PlayerListData(
    IReadOnlyList<PlayerEntry> Players);

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

/// <summary>
/// ［６大境界］のうち、［大会結果］境界データだ。
/// </summary>
/// <param name="MatchRecords"></param>
/// <param name="CurrentTime"></param>
/// <param name="TickCount"></param>
/// <param name="CompletedNaturally"></param>
sealed record class TournamentResultData(
    IReadOnlyList<TournamentMatchRecord> MatchRecords,
    int CurrentTime,
    int TickCount,
    bool CompletedNaturally);

/// <summary>
/// ［６大境界］のうち、［最終順位］境界データだ。
/// </summary>
/// <param name="RankRows"></param>
/// <param name="IsIntermediate"></param>
/// <param name="Note"></param>
sealed record class FinalRankingData(
    IReadOnlyList<PlayerRankRow> RankRows,
    bool IsIntermediate,
    string? Note);

/// <summary>
/// ［６大境界］のうち、［大会品質レポート］境界データだ。
/// </summary>
/// <param name="PlayerRows"></param>
/// <param name="Summary"></param>
/// <param name="CalculationMode"></param>
sealed record class TournamentQualityReportData(
    IReadOnlyList<TournamentQualityReportPlayerRow> PlayerRows,
    TournamentQualityReportSummary Summary,
    string CalculationMode);

/// <summary>
/// ［６大境界］のうち、［大会品質レポート］境界データのスイープ実験版だ。
/// </summary>
/// <param name="SweepRows"></param>
/// <param name="StoppedByTimeout"></param>
sealed record class TournamentQualitySweepReportData(
    IReadOnlyList<TournamentQualitySweepReportRow> SweepRows,
    bool StoppedByTimeout);
