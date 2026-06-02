namespace ShogiTournamentSystemAnalyzer.Domain.Request;

using ShogiTournamentSystemAnalyzer.Application.Shared;

internal class RequestFileCheckResultVer2
{
    public RequestFileCheckResultVer2(bool hasError, RequestInputSession? inputSession)
    {
        HasError = hasError;
        InputSession = inputSession;
    }

    public bool HasError { get; set; }

    public RequestInputSession? InputSession { get; set; }
}
