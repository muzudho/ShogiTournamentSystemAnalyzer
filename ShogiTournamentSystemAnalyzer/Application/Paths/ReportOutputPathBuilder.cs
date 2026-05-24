/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static class ReportOutputPathBuilder
{
    static string NormalizeSnakeCaseToken(string value)
    {
        return value
            .Replace("+", "_plus_")
            .Replace("-", "_")
            .Replace(" ", "_")
            .Replace("__", "_")
            .ToLowerInvariant();
    }

    static string GetQualityRuleNameToken(TournamentRuleSetMode tournamentRuleSetMode)
    {
        return tournamentRuleSetMode switch
        {
            TournamentRuleSetMode.Twill => "twill",
            TournamentRuleSetMode.TwillCommonOpponentWeighted => "twill_commonopp",
            _ => "neutral",
        };
    }

    static string GetQualityConditionToken(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode)
    {
        return $"{NormalizeSnakeCaseToken(placementMode.ToString())}_{NormalizeSnakeCaseToken(boundaryRescueMode.ToString())}";
    }

    static string BuildTimestampedQualityFileName(string leadingContext, string artifactType)
    {
        return $"{leadingContext}_{artifactType}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
    }

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

    internal static string BuildTournamentQualityPlayersOutputPathFromSummary(string summaryOutputCsvPath)
    {
        var fullSummaryPath = Path.GetFullPath(summaryOutputCsvPath);
        var fullRootPath = Path.GetFullPath(".");
        var summaryDirectorySegment = Path.Combine(fullRootPath, "Output", "TournamentQualityEvaluator", "TournamentQualityReport", "Summary");
        var playersDirectorySegment = Path.Combine(fullRootPath, "Output", "TournamentQualityEvaluator", "TournamentQualityReport", "Players");

        var playerDirectoryPath = Path.GetDirectoryName(fullSummaryPath) ?? fullRootPath;
        if (playerDirectoryPath.StartsWith(summaryDirectorySegment, StringComparison.OrdinalIgnoreCase))
        {
            playerDirectoryPath = playersDirectorySegment + playerDirectoryPath[summaryDirectorySegment.Length..];
        }

        Directory.CreateDirectory(playerDirectoryPath);
        var playerFileName = BuildTournamentQualityPlayersFileNameFromSummary(Path.GetFileName(fullSummaryPath));
        return Path.Combine(playerDirectoryPath, playerFileName);
    }

    static string BuildTournamentQualityPlayersFileNameFromSummary(string summaryFileName)
    {
        if (summaryFileName.Contains("_quality_summary_", StringComparison.Ordinal))
        {
            return summaryFileName.Replace("_quality_summary_", "_quality_players_", StringComparison.Ordinal);
        }

        if (summaryFileName.Contains("_quality_summary.", StringComparison.Ordinal))
        {
            return summaryFileName.Replace("_quality_summary.", "_quality_players.", StringComparison.Ordinal);
        }

        return BuildTimestampedQualityFileName("generated", "quality_players");
    }

    internal static string BuildTournamentQualitySweepDefaultOutputPath(string fileName, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (!options.IsEnabled) return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Sweeps", fileName);

        var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
        return BuildOutputFilePath("TournamentQualityEvaluator", "TournamentQualityReport", "Sweeps", outcomeFolderName, fileName);
    }

    static string BuildQualitySummaryDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        var fileName = BuildTimestampedQualityFileName(GetQualityConditionToken(placementMode, boundaryRescueMode), "quality_summary");
        return BuildTournamentQualitySummaryDefaultOutputPath(fileName, options);
    }

    static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var fileName = BuildTimestampedQualityFileName("neutral", "quality_summary");
            return BuildTournamentQualitySummaryDefaultOutputPath(fileName, options);
        }

        return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    internal static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var ruleName = GetQualityRuleNameToken(tournamentRuleSetMode);
            var fileName = BuildTimestampedQualityFileName(ruleName, "quality_summary");
            return BuildTournamentQualitySummaryDefaultOutputPath(fileName, options);
        }

        return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    static string BuildQualitySweepDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        var fileName = BuildTimestampedQualityFileName(GetQualityConditionToken(placementMode, boundaryRescueMode), "quality_sweep");
        return BuildTournamentQualitySweepDefaultOutputPath(fileName, options);
    }

    static string BuildTournamentQualitySweepReportDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var fileName = BuildTimestampedQualityFileName("neutral", "quality_sweep");
            return BuildTournamentQualitySweepDefaultOutputPath(fileName, options);
        }

        return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    internal static string BuildTournamentQualitySweepReportDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var ruleName = GetQualityRuleNameToken(tournamentRuleSetMode);
            var fileName = BuildTimestampedQualityFileName(ruleName, "quality_sweep");
            return BuildTournamentQualitySweepDefaultOutputPath(fileName, options);
        }

        return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }
}

