namespace ShogiTournamentSystemAnalyzer.Domain.Request;

internal class RequestModelFromFileCheckProducer
{
    public RequestModelFromFileCheckProducer()
    {
    }

    public bool HasError { get; set; }

    public void Produce(RequestBoundary requestBoundary)
    {
        // XXX: 適当な、値をセットする例。
        requestBoundary.Banana = 123;
    }
}
