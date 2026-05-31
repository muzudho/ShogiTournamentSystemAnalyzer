/*
 * ［アプリケーション　＞　入力　＞　手動入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class ManualInput
{
    internal static RequestInputSession Start()
    {
        Console.WriteLine("■［手動入力］");
        var requestFileCreatePath = RequestFileCreatePrompt.ReadOutputPath();
        if (requestFileCreatePath is null)
        {
            Console.WriteLine();
            return RequestInputSession.FromManualInputWithoutRequestFileCreate();
        }

        var originalInput = Console.In;
        var recordingInput = new ManualInputRecordingTextReader(originalInput);
        Console.SetIn(recordingInput);
        Console.WriteLine($"分析中の入力を記録し、分析後に要求ファイルを作成します: {requestFileCreatePath}\n");
        return RequestInputSession.FromManualInputWithRequestFileCreate(originalInput, recordingInput, requestFileCreatePath);
    }
}