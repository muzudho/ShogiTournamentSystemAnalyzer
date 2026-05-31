/*
 * ［アプリケーション　＞　実行　＞　依頼入力セッション実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using AnalysisRun = ShogiTournamentSystemAnalyzer.Application.Analysis.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Shared;



internal static class ApplicationRequestSessionRun
{
    internal static void Run(RequestInputSession inputSource)
    {
        // ［依頼］を受け取って分析を始めるぜ（＾▽＾）！
        AnalysisRun.Run();

        // 入力セッションの後片付けや完了処理を行うぜ（＾▽＾）！
        inputSource.Complete();
    }
}