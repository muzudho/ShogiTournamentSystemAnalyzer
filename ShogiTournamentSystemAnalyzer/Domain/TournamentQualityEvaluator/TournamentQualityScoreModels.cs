/*
 * ［大会品質評価フロー域　＞　総合点］で使われるモデル
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［大会品質評価フロー域　＞　総合点ルール］だ。
/// </summary>
readonly record struct TournamentQualityScoreRule(
    string PresetName,
    int ScoreMax,
    double MeanRankErrorTolerance,
    int SpearmanWeight,
    int MeanRankErrorWeight,
    int Top8RetentionWeight,
    int EloTop1WinWeight)
{
    internal static TournamentQualityScoreRule Balanced()
    {
        return new TournamentQualityScoreRule(
            "Balanced",
            100000,
            4.0,
            40000,
            25000,
            20000,
            15000);
    }

    internal static TournamentQualityScoreRule ChampionFocused()
    {
        return new TournamentQualityScoreRule(
            "ChampionFocused",
            100000,
            4.0,
            30000,
            20000,
            15000,
            35000);
    }

    internal static TournamentQualityScoreRule Top8Focused()
    {
        return new TournamentQualityScoreRule(
            "Top8Focused",
            100000,
            4.0,
            30000,
            20000,
            35000,
            15000);
    }

    internal static TournamentQualityScoreRule FromPresetName(string presetName)
    {
        if (presetName.Equals("Balanced", StringComparison.OrdinalIgnoreCase)) return Balanced();
        if (presetName.Equals("ChampionFocused", StringComparison.OrdinalIgnoreCase)) return ChampionFocused();
        if (presetName.Equals("Top8Focused", StringComparison.OrdinalIgnoreCase)) return Top8Focused();

        throw new OperationCanceledException($"品質評価総合点の Preset は Balanced / ChampionFocused / Top8Focused のいずれかで入力してください: {presetName}");
    }
}

/// <summary>
/// ［大会品質評価フロー域　＞　総合点の信頼区分］だ。
/// </summary>
readonly record struct TournamentQualityScoreReliability(
    int SimulationCount,
    string Label,
    bool IsReferenceRecord,
    bool IsOfficialEvaluation)
{
    internal static TournamentQualityScoreReliability FromSimulationCount(int simulationCount)
    {
        return simulationCount switch
        {
            >= 20000 => new TournamentQualityScoreReliability(simulationCount, "本評価", false, true),
            >= 2000 => new TournamentQualityScoreReliability(simulationCount, "比較用", false, false),
            >= 200 => new TournamentQualityScoreReliability(simulationCount, "軽量確認", false, false),
            >= 1 => new TournamentQualityScoreReliability(simulationCount, "参考記録", true, false),
            _ => new TournamentQualityScoreReliability(simulationCount, "未解決", true, false),
        };
    }
}

/// <summary>
/// ［大会品質評価フロー域　＞　総合点内訳］だ。
/// </summary>
readonly record struct TournamentQualityScoreBreakdown(
    int TotalScore,
    int ScoreMax,
    string PresetName,
    TournamentQualityScoreReliability Reliability,
    double MeanRankErrorTolerance,
    double SpearmanScore,
    int SpearmanPoints,
    int SpearmanMaxPoints,
    double MeanRankErrorScore,
    int MeanRankErrorPoints,
    int MeanRankErrorMaxPoints,
    double Top8RetentionScore,
    int Top8RetentionPoints,
    int Top8RetentionMaxPoints,
    double EloTop1WinScore,
    int EloTop1WinPoints,
    int EloTop1WinMaxPoints);