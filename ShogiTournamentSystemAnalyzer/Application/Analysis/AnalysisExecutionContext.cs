/*
 * ［アプリケーション　＞　実行　＞　分析実行コンテキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;

internal sealed class AnalysisExecutionContext
{
    internal SimulationDomainResult? LastSimulationResult { get; private set; }

    internal void SetSimulationResult(SimulationDomainResult? result)
    {
        LastSimulationResult = result;
    }
}