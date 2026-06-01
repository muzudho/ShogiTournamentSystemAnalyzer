namespace ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestModelProducer
{
    public RequestModelProducer()
    {
        this.RequestModel = new RequestModel();
    }

    public bool HasError { get; set; }

    public RequestModel RequestModel { get; private set; }
}
