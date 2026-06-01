namespace ShogiTournamentSystemAnalyzer.Application;

internal class ApplicationWorkflowResultModel
{
    public ApplicationWorkflowResultModel(bool hasError)
    {
        this.HasError = hasError;
    }

    public bool HasError { get; init; }
}
