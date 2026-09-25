using Caffeine.Models;

namespace Caffeine.Services;

public sealed record SleepInsight(
    string MetricKey,
    string MessageKey,
    int Count,
    double Correlation);

public sealed class SleepInsightService
{
    public const int MinimumDays = 10;

    public List<SleepInsight> Analyze(
        IEnumerable<SleepLog> logs)
    {
        var items = logs
            .Where(s => !s.IsImportedDuplicate)
            .OrderByDescending(s => s.SleepDate)
            .Take(90)
            .ToArray();

        return
        [
            AnalyzePair(
                "BedtimeCaffeineVsRating",
                items.Select(s =>
                    (
                        s.EstimatedCaffeineAtBedtime,
                        (double)s.SleepRating
                    ))),

            AnalyzePair(
                "DailyCaffeineVsRating",
                items.Select(s =>
                    (
                        s.PreviousDayCaffeineMg,
                        (double)s.SleepRating
                    ))),

            AnalyzePair(
                "LateCaffeineVsDifficulty",
                items
                    .Where(s =>
                        s.LastCaffeineAt.HasValue &&
                        s.FallingAsleep.HasValue)
                    .Select(s =>
                        (
                            -(s.ActualBedtime -
                              s.LastCaffeineAt!.Value).TotalHours,

                            (double)s.FallingAsleep!.Value
                        ))),

            AnalyzePair(
                "DailyCaffeineVsDuration",
                items.Select(s =>
                    (
                        s.PreviousDayCaffeineMg,
                        s.DurationHours
                    )))
        ];
    }

    private static SleepInsight AnalyzePair(
        string metric,
        IEnumerable<(double X, double Y)> source)
    {
        var p = source
            .Where(p =>
                double.IsFinite(p.X) &&
                double.IsFinite(p.Y))
            .ToArray();

        if (p.Length < MinimumDays)
        {
            return new(
                metric,
                "InsufficientSleepData",
                p.Length,
                0);
        }

        var mx = p.Average(p => p.X);
        var my = p.Average(p => p.Y);

        var xx = p.Sum(p =>
            Math.Pow(p.X - mx, 2));

        var yy = p.Sum(p =>
            Math.Pow(p.Y - my, 2));

        if (xx < 0.000001 ||
            yy < 0.000001)
        {
            return new(
                metric,
                "SleepNoVariation",
                p.Length,
                0);
        }

        var correlation =
            p.Sum(p =>
                (p.X - mx) *
                (p.Y - my))
            / Math.Sqrt(xx * yy);

        return new(
            metric,

            correlation <= -0.3
                ? "SleepLowerAssociation"

                : correlation >= 0.3
                    ? "SleepHigherAssociation"

                    : "SleepNoClearAssociation",

            p.Length,
            Math.Round(correlation, 2));
    }
}