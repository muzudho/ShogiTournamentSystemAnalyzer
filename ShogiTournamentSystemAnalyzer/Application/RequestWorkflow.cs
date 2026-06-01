/*
 * ［アプリケーション　＞　依頼ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

/// <summary>
/// 依頼の入力セッションを準備し、そのセッション内の処理へ渡すワークフロー。
/// </summary>
internal static class RequestWorkflow
{
    internal static void Run(IReadOnlyList<string> args)
    {
        using var inputSession = InputSourceConfiguration.ConfigureInputSource(args);
        if (inputSession is null) return;

        RequestSessionWorkflow.Run(inputSession);
    }
}
