/*
 * ［アプリケーション　＞　実行　＞　品質評価要求］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentQualityEvaluator.UseCases;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal static class QualityEvaluationRequestDispatcher
{
    internal static bool TryExecute(AnalysisStepRequest step)
    {
        return QualityEvaluationDomain.TryExecute(step);
    }
}
