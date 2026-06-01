namespace ShogiTournamentSystemAnalyzer.Application.ManualInput;

using ShogiTournamentSystemAnalyzer.Domain.Request;

internal class ManualInputWorkflow
{
    public static RequestModelFromManualProducer Run()
    {
        return new RequestModelFromManualProducer();
    }
}
