/// <summary>
/// ［大会ルールデータ］だ。
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
/// ［プレイヤー一覧データ］だ。
/// </summary>
/// <param name="Players"></param>
sealed record class PlayerListData(
    IReadOnlyList<PlayerEntry> Players);

/// <summary>
/// ［順位付けの設定データ］だ。
/// </summary>
/// <param name="TournamentRuleSetMode"></param>
/// <param name="IsIntermediate"></param>
/// <param name="Note"></param>
sealed record class RankingSettingsData(
    TournamentRuleSetMode TournamentRuleSetMode,
    bool IsIntermediate,
    string? Note);

/// <summary>
/// ［大会結果データ］だ。
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
/// ［最終順位データ］だ。
/// </summary>
/// <param name="RankRows"></param>
/// <param name="IsIntermediate"></param>
/// <param name="Note"></param>
sealed record class FinalRankingData(
    IReadOnlyList<PlayerRankRow> RankRows,
    bool IsIntermediate,
    string? Note);

/// <summary>
/// ［大会品質レポート］だ。
/// </summary>
/// <param name="PlayerRows"></param>
/// <param name="Summary"></param>
/// <param name="CalculationMode"></param>
sealed record class TournamentQualityReportData(
    IReadOnlyList<QualityPlayerRow> PlayerRows,
    QualitySummary Summary,
    string CalculationMode);
