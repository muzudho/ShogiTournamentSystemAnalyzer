/*
 * ［アプリケーション　＞　大会フレームワーク］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.TournamentFramework;

using ShogiTournamentSystemAnalyzer.Domain.Ranking;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// ［大会進行フレームワーク］の実行結果を表すクラス。
/// シミュレーション域から順位付け域へ渡す大会最終状態を含む。
/// </summary>
/// <param name="FinalState">大会の最終状態</param>
/// <param name="OverallRanking">大会の最終順位</param>
/// <param name="TickCount">大会の進行Tick数</param>
/// <param name="CompletedNaturally">大会が自然終了したかどうか</param>
sealed record class TournamentFrameworkExecutionResult(
    TournamentState FinalState,
    IReadOnlyList<PlayerRankRow> OverallRanking,
    int TickCount,
    bool CompletedNaturally);
