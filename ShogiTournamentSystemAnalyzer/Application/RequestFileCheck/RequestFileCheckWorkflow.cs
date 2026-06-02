namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestFileCheckWorkflow
{
    public static RequestFileCheckResultVer2 Run(RequestFileArgumentReadResult argumentResult)
    {
        // パースする。
        if (!RequestFileInputSessionStarter.TryStart(argumentResult.InputFilePath!, out var inputSession))
        {
            // ファイルは有ったが、内容のパースエラー等の場合。
            Console.WriteLine($"●エラー終了：　［要求ファイル］パース中エラー。 {argumentResult.ErrorMessage!}");
            return new RequestFileCheckResultVer2(hasError: true, inputSession: null);
        }

        return new RequestFileCheckResultVer2(hasError: false, inputSession: inputSession);
    }
}
