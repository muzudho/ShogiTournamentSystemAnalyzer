namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Domain.Request;

internal class AnalysisWorkflowNewVersion
{
    public static AnalysisResultModel Run(RequestModel requestModel)
    {
        return new AnalysisResultModel();
    }
}
