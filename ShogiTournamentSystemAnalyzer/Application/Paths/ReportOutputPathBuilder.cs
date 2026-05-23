/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static class ReportOutputPathBuilder
{
    static string BuildOutputFilePath(params string[] segments)
    {
        var fullSegments = new[] { Path.GetFullPath("."), "Output" }
            .Concat(segments)
            .ToArray();
        var directoryPath = Path.Combine(fullSegments[..^1]);
        Directory.CreateDirectory(directoryPath);
        return Path.Combine(directoryPath, fullSegments[^1]);
    }

    internal static string BuildFinalRankingDefaultOutputPath(string fileName)
    {
        return BuildOutputFilePath("Ranking", "FinalRanking", fileName);
    }

    internal static string BuildTournamentFinalStateDefaultOutputPath(string fileName)
    {
        return BuildOutputFilePath("Simulation", "TournamentFinalState", fileName);
    }

    internal static string BuildTournamentQualitySummaryDefaultOutputPath(string fileName, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (!options.IsEnabled) return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Summary", fileName);

        var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
        return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Summary", outcomeFolderName, fileName);
    }

    internal static string BuildTournamentQualityPlayersDefaultOutputPath(string fileName, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (!options.IsEnabled) return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Players", fileName);

        var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
        return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Players", outcomeFolderName, fileName);
    }

    internal static string BuildTournamentQualitySweepDefaultOutputPath(string fileName, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (!options.IsEnabled) return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Sweeps", fileName);

        var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
        return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Sweeps", outcomeFolderName, fileName);
    }

    static string BuildQualitySummaryDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        var fileName = $"quality_summary_{placementMode}_{boundaryRescueMode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return BuildTournamentQualitySummaryDefaultOutputPath(fileName, options);
    }

    static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var fileName = $"quality_summary_neutral_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return BuildTournamentQualitySummaryDefaultOutputPath(fileName, options);
        }

        return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    internal static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var ruleName = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "twill" : "neutral";
            var fileName = $"quality_summary_{ruleName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return BuildTournamentQualitySummaryDefaultOutputPath(fileName, options);
        }

        return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    static string BuildQualitySweepDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        var fileName = $"quality_sweep_{placementMode}_{boundaryRescueMode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return BuildTournamentQualitySweepDefaultOutputPath(fileName, options);
    }

    static string BuildTournamentQualitySweepReportDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var fileName = $"quality_sweep_neutral_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return BuildTournamentQualitySweepDefaultOutputPath(fileName, options);
        }

        return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    internal static string BuildTournamentQualitySweepReportDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var ruleName = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "twill" : "neutral";
            var fileName = $"quality_sweep_{ruleName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return BuildTournamentQualitySweepDefaultOutputPath(fileName, options);
        }

        return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }
}

