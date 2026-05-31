/*
 * ［アプリケーション　＞　実行　＞　アプリケーション実行］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Execution;

internal static class ApplicationRun
{
    internal static void Run(IReadOnlyList<string> args)
    {
        ApplicationStartup.Start();
        ApplicationRequestRun.Run(args);
    }
}