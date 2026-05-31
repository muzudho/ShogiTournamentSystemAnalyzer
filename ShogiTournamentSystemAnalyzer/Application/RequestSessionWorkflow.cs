/*
 * ［アプリケーション　＞　依頼入力セッション・ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.Shared;

/// <summary>
/// ［入力セッション中にやること］という責務
/// </summary>
internal static class RequestSessionWorkflow
{
    internal static void Run(RequestInputSession inputSource)
    {
        // ［依頼］を受け取って分析を始めるぜ（＾▽＾）！
        AnalysisWorkflow.Run();

        // 入力セッションの後片付けや完了処理を行うぜ（＾▽＾）！
        inputSource.Complete();
    }
}