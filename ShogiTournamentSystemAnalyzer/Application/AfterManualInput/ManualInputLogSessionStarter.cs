/*
 * ［アプリケーション　＞　手動入力後　＞　手入力ログセッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class ManualInputLogSessionStarter
{
    internal static RequestInputSession Start(string manualInputLogPath)
    {
        var recordedLines = ConsoleInput.StartRecording();
        var manualInputLogCompletionTarget = new ManualInputLogCompletionTarget(manualInputLogPath, recordedLines);

        Console.WriteLine($"分析中の入力を記録し、分析後に手入力ログを作成します: {manualInputLogPath}\n");
        return new RequestInputSession(requestFileInputText: null, manualInputLogCompletionTarget);
    }
}
