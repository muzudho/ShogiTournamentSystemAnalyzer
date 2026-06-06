/*
 * ［アプリケーション　＞　実行　＞　分析実行コンテキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal sealed class AnalysisExecutionContext
{
    internal AnalysisStepRequest? LastSimulationRequest { get; private set; }

    internal SimulationDomainResult? LastSimulationResult { get; private set; }

    internal void SetSimulationResult(AnalysisStepRequest request, SimulationDomainResult? result)
    {
        LastSimulationRequest = request;
        LastSimulationResult = result;
    }
}
