/*
 * ［最終順位付け域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// 大会進行フレームワーク用の最終順位付け結果だ。
/// 
/// TODO: "TournamentFramework" は［４大域］のいずれかに含めてほしいぜ（＾～＾）
/// </summary>
/// <param name="StandardPlayers">既存の順位表表示・書き出しで使う標準形式の選手。</param>
/// <param name="StandardMatches">既存の順位表表示・書き出しで使う標準形式の対局。</param>
/// <param name="RepresentativeStages">代表実行のステージ一覧。</param>
/// <param name="RepresentativePlayers">代表実行の選手一覧。</param>
/// <param name="RepresentativeTournamentFinalState">代表実行の大会最終状態。</param>
/// <param name="RepresentativeExecutionRankRows">代表実行の順位行。</param>
/// <param name="AggregateCalculationResult">aggregate 順位確率の計算結果。</param>
/// <param name="AggregateFinalRankingResult">aggregate 最終順位表。</param>
internal sealed record class TournamentFrameworkFinalRankingResult(
    IReadOnlyList<Player> StandardPlayers,
    IReadOnlyList<Match> StandardMatches,
    IReadOnlyList<StageEntry> RepresentativeStages,
    IReadOnlyList<PlayerEntry> RepresentativePlayers,
    TournamentFinalStateData RepresentativeTournamentFinalState,
    IReadOnlyList<RepresentativeExecutionRankRow> RepresentativeExecutionRankRows,
    CalculationResult AggregateCalculationResult,
    FinalRankingResult AggregateFinalRankingResult,
    TournamentRuleSetMode TournamentRuleSetMode,
    double FirstPlayerWinRatePercent);
