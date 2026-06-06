/*
 * ［アプリケーション　＞　実行　＞　シミュレーション要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal static class SimulationRequestDispatcher
{
    internal static bool TryExecute(AnalysisStepRequest step)
    {
        return SimulationDomain.TryExecute(step);
    }
}
