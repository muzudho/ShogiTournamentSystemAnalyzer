/*
 * ［アプリケーション　＞　アプリケーション・ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

/// <summary>
/// アプリケーション全体の最上位ワークフロー。
/// </summary>
internal static class ApplicationWorkflow
{
    internal static void Run(IReadOnlyList<string> args)
    {
        ApplicationStartup.Start();
        RequestWorkflow.Run(args);
    }
}