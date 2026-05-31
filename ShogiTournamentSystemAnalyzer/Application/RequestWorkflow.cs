/*
 * ［アプリケーション　＞　依頼ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;


/// <summary>
/// ［入力元を確定する］という責務
/// </summary>
internal static class RequestWorkflow
{
    internal static void Run(IReadOnlyList<string> args)
    {
        // ［要求セッション］の責務
        // ［依頼］が［要求ファイル］からか、［対話か］に応じて、入力導線を準備するぜ（＾▽＾）！
        using var inputSource = InputSourceConfiguration.ConfigureInputSource(args);
        RequestSessionWorkflow.Run(inputSource);
    }
}