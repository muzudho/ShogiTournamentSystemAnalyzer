using System.Globalization;

internal static partial class Program
{
    static double ConvertFirstPlayerWinRatePercentToRating(double firstPlayerWinRatePercent)
    {
        const double epsilon = 1e-9;
        var probability = Math.Clamp(firstPlayerWinRatePercent / 100.0, epsilon, 1.0 - epsilon);
        return 400.0 * Math.Log10(probability / (1.0 - probability));
    }

    static double ConvertBlackAdvantagePercentToRating(double blackAdvantagePercent)
    {
        return ConvertFirstPlayerWinRatePercentToRating(blackAdvantagePercent);
    }

    static string FormatPercent(double value)
    {
        return (value * 100).ToString("F2", CultureInfo.InvariantCulture) + "%";
    }

    static string FormatOptionalPercent(double? value)
    {
        return value.HasValue ? FormatPercent(value.Value) : "-";
    }

    static string FormatOptionalPercentValue(double? value)
    {
        return value.HasValue
            ? (value.Value * 100).ToString("F2", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    static double CalculateEquivalentNeutralRating(IReadOnlyList<double> opponentRatings, double targetAverageScore)
    {
        if (opponentRatings.Count == 0)
        {
            return 0.0;
        }

        const double epsilon = 1e-9;
        var clampedScore = Math.Clamp(targetAverageScore, epsilon, 1.0 - epsilon);
        var lowerBound = opponentRatings.Min() - 4000.0;
        var upperBound = opponentRatings.Max() + 4000.0;

        for (var i = 0; i < 80; i++)
        {
            var mid = (lowerBound + upperBound) / 2.0;
            var averageScore = opponentRatings.Average(opponentRating => GetNeutralWinProbability(mid, opponentRating));

            if (averageScore < clampedScore)
            {
                lowerBound = mid;
            }
            else
            {
                upperBound = mid;
            }
        }

        return (lowerBound + upperBound) / 2.0;
    }

    static double GetNeutralWinProbability(double playerRating, double opponentRating)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (opponentRating - playerRating) / 400.0));
    }

    static string FormatRating(double value)
    {
        return Math.Round(value).ToString("F0", CultureInfo.InvariantCulture);
    }

    static string FormatSignedRating(double value)
    {
        return Math.Round(value).ToString("+0;-0;0", CultureInfo.InvariantCulture);
    }
}

