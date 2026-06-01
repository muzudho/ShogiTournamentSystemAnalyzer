namespace ShogiTournamentSystemAnalyzer.Application.ManualInput;

using ShogiTournamentSystemAnalyzer.Domain.Request;

internal class ManualInputWorkflow
{
    public static RequestModelProducer Run()
    {
        return new RequestModelProducer();
    }
}
