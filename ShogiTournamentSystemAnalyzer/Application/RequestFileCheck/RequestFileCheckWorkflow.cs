namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestFileCheckWorkflow
{
    public static RequestModelFromFileCheckProducer Run(IReadOnlyList<string> args)
    {
        return new RequestModelFromFileCheckProducer();
    }
}
