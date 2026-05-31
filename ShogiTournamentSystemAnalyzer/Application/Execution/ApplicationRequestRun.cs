/*
 * ［アプリケーション　＞　実行　＞　依頼実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

using ShogiTournamentSystemAnalyzer.Application.Input;

internal static class ApplicationRequestRun
{
    internal static void Run(IReadOnlyList<string> args)
    {
        // ［依頼］が［要求ファイル］からか、［対話か］に応じて、入力導線を準備するぜ（＾▽＾）！
        using var inputSource = InputSourceConfiguration.ConfigureInputSource(args);

        // ［依頼］を受け取って分析を始めるぜ（＾▽＾）！
        Analysis.Run();

        // 入力セッションの後片付けや完了処理を行うぜ（＾▽＾）！
        inputSource.Complete();
    }
}