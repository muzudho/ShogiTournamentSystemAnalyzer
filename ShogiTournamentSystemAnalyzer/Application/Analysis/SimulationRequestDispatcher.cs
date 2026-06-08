/*
 * ［アプリケーション　＞　実行　＞　シミュレーション要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Simulation.UseCases;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal static class SimulationRequestDispatcher
{
    internal static bool TryExecute(AnalysisStepRequest step)
    {
        return TryExecute(step, out _);
    }

    internal static bool TryExecute(AnalysisStepRequest step, out SimulationDomainResult? result)
    {
        return SimulationDomain.TryExecute(step, out result);
    }
}
