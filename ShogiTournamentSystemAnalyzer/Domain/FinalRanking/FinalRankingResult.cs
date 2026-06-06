/*
 * ［最終順位付け域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.FinalRanking;

using ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// ［最終順位付け］の結果だ。
/// </summary>
/// <param name="Rows">最終順位表の行。</param>
internal sealed record class FinalRankingResult(IReadOnlyList<GeneralSimulationResultRow> Rows);