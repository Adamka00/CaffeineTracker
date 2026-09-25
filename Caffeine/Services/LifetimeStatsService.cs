using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed record HeatmapDay(
    DateTime Day,
    double Mg,
    int Drinks,
    bool CaffeineFree);

public sealed record CategoryStat(
    string Category,
    int Drinks,
    double Mg);

public sealed class LifetimeStats
{
    public int DrinkCount { get; set; }

    public double TotalMg { get; set; }

    public double DailyAverage { get; set; }

    public double Average7 { get; set; }

    public double Average30 { get; set; }

    public DateTime? TrackingSince { get; set; }

    public Beverage? MostFrequent { get; set; }

    public List<CategoryStat> Categories { get; set; } = [];

    public int FreeDayCount { get; set; }

    public StreakSummary Streak { get; set; } =
        new(0, 0, false);

    public TimeSpan? FirstCaffeineTime { get; set; }

    public TimeSpan? LastCaffeineTime { get; set; }

    public HeatmapDay? HighestDay { get; set; }

    public List<HeatmapDay> Heatmap { get; set; } = [];

    public List<SleepInsight> Insights { get; set; } = [];
}

public sealed class LifetimeStatsService(
    AppDbContext db,
    StreakService streaks,
    SleepInsightService insights)
{
    public async Task<LifetimeStats> GetAsync(
        string userId,
        DateTime now)
    {
        var logs = await db.CaffeineLogs
            .AsNoTracking()
            .Include(l => l.Beverage)
            .Where(l =>
                l.UserId == userId &&
                l.ConsumedAt <= now)
            .ToListAsync();

        var free =
            (await db.CaffeineFreeDays
                .AsNoTracking()
                .Where(d =>
                    d.UserId == userId &&
                    d.Day <= now.Date)
                .Select(d => d.Day)
                .ToListAsync())
            .ToHashSet();

        var days = logs
            .GroupBy(l => l.ConsumedAt.Date)
            .ToDictionary(
                g => g.Key,
                g => g.ToArray());

        var start = days.Keys
            .Concat(free)
            .Select(d => (DateTime?)d)
            .Min();

        var caffeinatedDays = logs
            .Where(l => l.TotalCaffeineMg > 0)
            .GroupBy(l => l.ConsumedAt.Date)
            .ToArray();

        var total =
            logs.Sum(l => l.TotalCaffeineMg);

        var result = new LifetimeStats
        {
            DrinkCount = logs.Count,

            TotalMg = total,

            TrackingSince = start,

            DailyAverage =
                start.HasValue
                    ? total /
                      ((now.Date - start.Value).Days + 1)
                    : 0,

            Average7 =
                logs.Where(l =>
                        l.ConsumedAt >=
                        now.Date.AddDays(-6))
                    .Sum(l => l.TotalCaffeineMg)
                / 7,

            Average30 =
                logs.Where(l =>
                        l.ConsumedAt >=
                        now.Date.AddDays(-29))
                    .Sum(l => l.TotalCaffeineMg)
                / 30,

            MostFrequent =
                logs.GroupBy(l => l.BeverageId)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key)
                    .FirstOrDefault()
                    ?.First()
                    .Beverage,

            Categories =
                logs.GroupBy(l => l.Beverage.Category)
                    .Select(g =>
                        new CategoryStat(
                            g.Key,
                            g.Count(),
                            g.Sum(l =>
                                l.TotalCaffeineMg)))
                    .OrderByDescending(c =>
                        c.Drinks)
                    .ToList(),

            FreeDayCount = free.Count,

            Streak =
                await streaks.GetAsync(
                    userId,
                    now),

            FirstCaffeineTime =
                caffeinatedDays.Length == 0
                    ? null
                    : TimeSpan.FromMinutes(
                        caffeinatedDays.Average(g =>
                            g.Min(l =>
                                l.ConsumedAt.TimeOfDay
                                    .TotalMinutes))),

            LastCaffeineTime =
                caffeinatedDays.Length == 0
                    ? null
                    : TimeSpan.FromMinutes(
                        caffeinatedDays.Average(g =>
                            g.Max(l =>
                                l.ConsumedAt.TimeOfDay
                                    .TotalMinutes))),

            HighestDay =
                days.Select(g =>
                        new HeatmapDay(
                            g.Key,
                            g.Value.Sum(l =>
                                l.TotalCaffeineMg),
                            g.Value.Length,
                            free.Contains(g.Key)))
                    .OrderByDescending(d => d.Mg)
                    .ThenBy(d => d.Day)
                    .FirstOrDefault(),

            Insights =
                insights.Analyze(
                    await db.SleepLogs
                        .AsNoTracking()
                        .Where(s =>
                            s.UserId == userId &&
                            !s.IsImportedDuplicate)
                        .OrderByDescending(s =>
                            s.SleepDate)
                        .Take(90)
                        .ToListAsync())
        };

        for (var day = now.Date.AddDays(-364);
             day <= now.Date;
             day = day.AddDays(1))
        {
            var items =
                days.GetValueOrDefault(day) ?? [];

            result.Heatmap.Add(
                new HeatmapDay(
                    day,
                    items.Sum(l =>
                        l.TotalCaffeineMg),
                    items.Length,
                    free.Contains(day)));
        }

        return result;
    }
}