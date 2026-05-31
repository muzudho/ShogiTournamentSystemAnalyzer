/*
 * ［アプリケーション　＞　要求ファイルチェック前　＞　要求ファイル入力セッション開始］
 */
namespace ShogiTournamentSystemAnalyzer.Application.BeforeRequestFileCheck;

using System.Diagnostics.CodeAnalysis;
using RequestFileCheckWorkflow = ShogiTournamentSystemAnalyzer.Application.RequestFileCheck.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.Shared;

internal static class RequestFileInputSessionStarter
{
    internal static bool TryStart(string inputFilePath, [NotNullWhen(true)] out RequestInputSession? inputSession)
    {
        if (RequestFileCheckWorkflow.TryRead(inputFilePath, RequestInputFileReader.Read, out var checkedInputFile))
        {
            RequestInputApplier.Apply(checkedInputFile);
            inputSession = RequestInputSession.WithoutCompletion();
            return true;
        }

        inputSession = null;
        return false;
    }
}