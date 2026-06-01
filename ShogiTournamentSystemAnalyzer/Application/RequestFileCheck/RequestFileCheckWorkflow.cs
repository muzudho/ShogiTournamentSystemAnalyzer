namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestFileCheckWorkflow
{
    public static RequestModelProducer Run(IReadOnlyList<string> args)
    {
        return new RequestModelProducer();
    }
}
