/*
 * ［アプリケーション　＞　依頼入力セッション・ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.Shared;

/// <summary>
/// 入力セッションの有効期間内で、分析とセッション完了処理を順に実行するワークフロー。
/// </summary>
internal static class RequestSessionWorkflow
{
    internal static void Run(RequestInputSession inputSession)
    {
        AnalysisWorkflow.Run();
        inputSession.Complete();
    }
}