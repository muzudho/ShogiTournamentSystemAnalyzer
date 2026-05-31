/*
 * ［アプリケーション　＞　入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class InputSourceConfiguration
{
    internal static RequestInputSession ConfigureInputSource(IReadOnlyList<string> args)
    {
        var argumentResult = RequestFileArgumentReader.Read(args);
        if (argumentResult.HasError)
        {
            Console.WriteLine($"要求ファイルチェック: エラー有り: {argumentResult.ErrorMessage}");
            return ManualInput.Start();
        }

        if (argumentResult.HasInputFile)
        {
            return TryStartFromRequestFile(argumentResult.InputFilePath!);
        }

        return ManualInput.Start();
    }

    static RequestInputSession TryStartFromRequestFile(string inputFilePath)
    {
        if (RequestFileCheck.TryRead(inputFilePath, RequestInputFileReader.Read, out var checkedInputFile))
        {
            RequestInputApplier.Apply(checkedInputFile);
            return RequestInputSession.WithoutCompletion();
        }

        Console.WriteLine("入力ファイルにエラーがあったため、手動入力へ切り替えます。\n");
        return ManualInput.Start();
    }
}