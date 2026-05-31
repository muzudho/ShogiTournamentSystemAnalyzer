/*
 * ［アプリケーション　＞　入力］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal static class InputSourceConfiguration
{
    internal static RequestInputSession ConfigureInputSource(IReadOnlyList<string> args)
    {
        var inputFilePath = RequestFileArgumentReader.TryGetInputFilePath(args);
        if (!string.IsNullOrWhiteSpace(inputFilePath))
        {
            if (RequestFileCheck.TryRead(inputFilePath, RequestInputFileReader.Read, out var checkedInputFile))
            {
                RequestInputApplier.Apply(checkedInputFile);
                return RequestInputSession.FromRequestFile();
            }

            Console.WriteLine("入力ファイルにエラーがあったため、手動入力へ切り替えます。\n");
            return ManualInput.Start();
        }

        return ManualInput.Start();
    }

}
