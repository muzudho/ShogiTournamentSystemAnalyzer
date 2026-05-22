using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRule;

internal static partial class Program
{
    static string BuildQualitySummaryDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        var fileName = $"quality_summary_{placementMode}_{boundaryRescueMode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!options.IsEnabled) return Path.GetFullPath(fileName);

        var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
        var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
        Directory.CreateDirectory(baseDirectory);
        return Path.Combine(baseDirectory, fileName);
    }

    static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var fileName = $"quality_summary_neutral_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            if (!options.IsEnabled) return Path.GetFullPath(fileName);

            var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
            var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
            Directory.CreateDirectory(baseDirectory);
            return Path.Combine(baseDirectory, fileName);
        }

        return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    static string BuildQualitySummaryDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var ruleName = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "twill" : "neutral";
            var fileName = $"quality_summary_{ruleName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            if (!options.IsEnabled) return Path.GetFullPath(fileName);

            var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
            var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
            Directory.CreateDirectory(baseDirectory);
            return Path.Combine(baseDirectory, fileName);
        }

        return BuildQualitySummaryDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    static string BuildQualitySweepDefaultOutputPath(AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        var fileName = $"quality_sweep_{placementMode}_{boundaryRescueMode}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        if (!options.IsEnabled) return Path.GetFullPath(fileName);

        var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
        var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
        Directory.CreateDirectory(baseDirectory);
        return Path.Combine(baseDirectory, fileName);
    }

    static string BuildTournamentQualitySweepReportDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var fileName = $"quality_sweep_neutral_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            if (!options.IsEnabled) return Path.GetFullPath(fileName);

            var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
            var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
            Directory.CreateDirectory(baseDirectory);
            return Path.Combine(baseDirectory, fileName);
        }

        return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }

    static string BuildTournamentQualitySweepReportDefaultOutputPath(FinalStageGroupingMode groupingMode, AdditionalApexPlacementMode placementMode, BoundaryRescueMode boundaryRescueMode, TournamentQualityEvaluationReportGroupingOptions options, TournamentRuleSetMode tournamentRuleSetMode)
    {
        if (groupingMode == FinalStageGroupingMode.Off)
        {
            var ruleName = tournamentRuleSetMode == TournamentRuleSetMode.Twill ? "twill" : "neutral";
            var fileName = $"quality_sweep_{ruleName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            if (!options.IsEnabled) return Path.GetFullPath(fileName);

            var outcomeFolderName = options.Outcome == TournamentQualityEvaluationReportOutcome.Bad ? "Bad" : "Good";
            var baseDirectory = Path.Combine(Path.GetFullPath("."), "docs", "Reports", outcomeFolderName);
            Directory.CreateDirectory(baseDirectory);
            return Path.Combine(baseDirectory, fileName);
        }

        return BuildQualitySweepDefaultOutputPath(placementMode, boundaryRescueMode, options);
    }
}

