namespace ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestModelFromManualProducer
{
    public RequestModelFromManualProducer()
    {
        this.RequestModel = new RequestModel();
    }

    public bool HasError { get; set; }

    public bool ShallSave { get; set; }

    public RequestModel RequestModel { get; private set; }
}
