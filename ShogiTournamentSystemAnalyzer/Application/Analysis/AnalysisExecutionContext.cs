/*
 * ［アプリケーション　＞　実行　＞　分析実行コンテキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Application.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal sealed class AnalysisExecutionContext
{
    internal AnalysisStepRequest? LastSimulationRequest { get; private set; }

    internal SimulationDomainResult? LastSimulationResult { get; private set; }

    internal FinalRankingDomainInput? PendingFinalRanking { get; private set; }

    internal void SetSimulationResult(AnalysisStepRequest? request, SimulationDomainResult? result)
    {
        LastSimulationRequest = request;
        LastSimulationResult = result;
        PendingFinalRanking = result?.PendingFinalRankingInput;
    }

    internal void SetPendingFinalRanking(FinalRankingDomainInput input)
    {
        PendingFinalRanking = input;
    }

    internal void ClearPendingFinalRanking()
    {
        PendingFinalRanking = null;
    }
}
