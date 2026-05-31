/*
 * ［アプリケーション　＞　アプリケーション・ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

/// <summary>
/// ［アプリケーション］という責務
/// </summary>
internal static class ApplicationWorkflow
{
    internal static void Run(IReadOnlyList<string> args)
    {
        // 開始
        ApplicationStartup.Start();
        // 要求の責務
        RequestWorkflow.Run(args);
    }
}