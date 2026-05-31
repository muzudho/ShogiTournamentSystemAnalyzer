/*
 * ［アプリケーション　＞　実行　＞　依頼実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;



internal static class ApplicationRequestRun
{
    internal static void Run(IReadOnlyList<string> args)
    {
        // ［依頼］が［要求ファイル］からか、［対話か］に応じて、入力導線を準備するぜ（＾▽＾）！
        using var inputSource = InputSourceConfiguration.ConfigureInputSource(args);
        ApplicationRequestSessionRun.Run(inputSource);
    }
}