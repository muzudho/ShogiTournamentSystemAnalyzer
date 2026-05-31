/*
 * ［分析　＞　境界　＞　出力パス共通］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

internal static partial class ReportOutputPathBuilder
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
}