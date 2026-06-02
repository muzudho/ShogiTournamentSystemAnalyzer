namespace ShogiTournamentSystemAnalyzer.Domain.Request;

using ShogiTournamentSystemAnalyzer.Domain.Request.PlayerList;
using ShogiTournamentSystemAnalyzer.Domain.Request.RankingSettings;
using ShogiTournamentSystemAnalyzer.Domain.Request.TournamentRule;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;

/// <summary>
/// ［依頼という境界］
/// </summary>
internal class RequestBoundary
{
    /// <summary>
    /// 適当なプロパティ
    /// </summary>
    public int Banana { get; set; }

    public PlayerListBoundary PlayerListBoundary { get; set; } = new();

    public RankingSettingsBoundary RankingSettingsBoundary { get; set; } = new();

    public TournamentRuleBoundary TournamentRuleBoundary { get; set;} = new();

    public AnalysisFlowMode AnalysisFlowMode { get; set; } = new();

    public RuleProfileMode RuleProfileMode { get; set; } = new();
}
