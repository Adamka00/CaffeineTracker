
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Caffeine.Services;

public sealed class DurationDisplay(IStringLocalizer<SharedResource> text)
{
    public string Hours(double? value) =>
        value.HasValue && double.IsFinite(value.Value)
            ? Minutes(value.Value * 60)
            : "—";

    public string Minutes(double value)
    {
        var total = (int)Math.Round(
            Math.Abs(value),
            MidpointRounding.AwayFromZero);

        return (value < 0 ? "−" : "") +
               text["DurationHoursMinutes", total / 60, total % 60];
    }
}

public sealed record SleepWeek(
    DateTime Start,
    int Count,
    double? AverageHours,
    double? MinHours,
    double? MaxHours,
    double? AverageRating,
    double? AverageAwakenings,
    int AwakeningCount,
    double? AverageDifficulty,
    int DifficultyCount,
    double? PlannedDifferenceMinutes,
    int PlannedCount,
    List<SleepDay> Days);

public sealed record SleepDay(
    DateTime Day,
    double? Hours,
    int? Rating);

public sealed record SleepStatistics(
    SleepWeek Current,
    SleepWeek Previous,
    DateTime Today);

public sealed class SleepStatisticsService(AppDbContext db)
{
    public static DateTime Monday(DateTime day) =>
        day.Date.AddDays(-((int)day.DayOfWeek + 6) % 7);

    private static double? Average(IEnumerable<double> source)
    {
        var a = source.ToArray();
        return a.Length > 0 ? a.Average() : null;
    }

    public static SleepWeek Calculate(
        DateTime start,
        IEnumerable<SleepLog> source)
    {
        var rows = source
            .Where(s =>
                !s.IsImportedDuplicate &&
                s.SleepDate >= start &&
                s.SleepDate < start.AddDays(7))
            .ToArray();

        var durations = rows
            .Select(s => s.DurationHours)
            .ToArray();

        return new(
            start,
            rows.Length,
            Average(durations),
            durations.Length > 0 ? durations.Min() : null,
            durations.Length > 0 ? durations.Max() : null,
            Average(rows.Select(s => (double)s.SleepRating)),
            Average(rows
                .Where(s => s.Awakenings.HasValue)
                .Select(s => (double)s.Awakenings!.Value)),
            rows.Count(s => s.Awakenings.HasValue),
            Average(rows
                .Where(s => s.FallingAsleep.HasValue)
                .Select(s => (double)s.FallingAsleep!.Value)),
            rows.Count(s => s.FallingAsleep.HasValue),
            Average(rows
                .Where(s => s.PlannedBedtime.HasValue)
                .Select(s =>
                    (s.ActualBedtime - s.PlannedBedtime!.Value)
                    .TotalMinutes)),
            rows.Count(s => s.PlannedBedtime.HasValue),
            Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = start.AddDays(i);
                    var row = rows.FirstOrDefault(s => s.SleepDate == day);

                    return new SleepDay(
                        day,
                        row?.DurationHours,
                        row?.SleepRating);
                })
                .ToList());
    }

    public async Task<SleepStatistics> GetAsync(
        string user,
        DateTime day,
        DateTime today)
    {
        var start = Monday(day);
        var previous = start.AddDays(-7);
        var end = start.AddDays(7);

        var rows = await db.SleepLogs
            .AsNoTracking()
            .Where(s =>
                s.UserId == user &&
                s.SleepDate >= previous &&
                s.SleepDate < end &&
                s.SleepDate <= today &&
                !s.IsImportedDuplicate)
            .ToListAsync();

        return new(
            Calculate(start, rows),
            Calculate(previous, rows),
            today);
    }
}
