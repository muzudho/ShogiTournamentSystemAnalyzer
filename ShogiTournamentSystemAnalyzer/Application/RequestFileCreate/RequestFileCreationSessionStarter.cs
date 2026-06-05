/*
 * ［アプリケーション　＞　手動入力後　＞　要求ファイル作成セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;

using ShogiTournamentSystemAnalyzer.Application.Shared;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class RequestFileCreationSessionStarter
{
    internal static RequestInputSession Start(string requestFilePath)
    {
        var recordedLines = ConsoleInput.StartRecording();

        Console.WriteLine($"分析中の入力を記録し、分析後に要求ファイルを作成します: {requestFilePath}\n");
        return new RequestInputSession(
            requestFileInputText: null,
            requestFilePath,
            recordedLines);
    }
}
