/*
 * ［アプリケーション　＞　アプリケーション・ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

internal static class ApplicationWorkflow
{
    internal static void Run(IReadOnlyList<string> args)
    {
        ApplicationStartup.Start();
        RequestWorkflow.Run(args);
    }
}