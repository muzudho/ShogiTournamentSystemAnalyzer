namespace ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestModelFromFileCheckProducer
{
    public RequestModelFromFileCheckProducer()
    {
        this.RequestModel = new RequestModel();
    }

    public bool HasError { get; set; }

    public RequestModel RequestModel { get; private set; }
}
