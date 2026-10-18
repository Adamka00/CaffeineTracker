using Caffeine.Data;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;
public sealed record CaffeineWeek(double TotalMg, int DrinkCount, int ActiveDays);
public sealed record WeeklySummary(DateTime Start, CaffeineWeek Current, CaffeineWeek Previous, SleepWeek Sleep)
{
    public bool CanCompare => Current.ActiveDays >= 3 && Previous.ActiveDays >= 3;
}

public sealed class WeeklySummaryService(AppDbContext db, SleepStatisticsService sleep)
{
    public async Task<WeeklySummary> GetAsync(int userId, DateTime start)
    {
        start = SleepStatisticsService.Monday(start);
        var owner = userId.ToString();
        var previous = start.AddDays(-7);
        var end = start.AddDays(7);
        var logs = await db.CaffeineLogs.AsNoTracking().Where(l => l.UserId == owner && l.ConsumedAt >= previous && l.ConsumedAt < end).ToListAsync();
        var free = await db.CaffeineFreeDays.AsNoTracking().Where(d => d.UserId == owner && d.Day >= previous && d.Day < end).Select(d => d.Day).ToListAsync();
        CaffeineWeek Calculate(DateTime from)
        {
            var to = from.AddDays(7);
            var rows = logs.Where(l => l.ConsumedAt >= from && l.ConsumedAt < to).ToArray();
            return new(rows.Sum(l => l.TotalCaffeineMg), rows.Length, rows.Select(l => l.ConsumedAt.Date).Concat(free.Where(d => d >= from && d < to)).Distinct().Count());
        }

        return new(start, Calculate(start), Calculate(previous), (await sleep.GetAsync(owner, start, end.AddDays(-1))).Current);
    }

    public static DateTime LatestDueWeek(DateTime local, int dayOfWeek, TimeSpan sendTime)
    {
        var monday = SleepStatisticsService.Monday(local);
        var due = monday.AddDays((dayOfWeek + 6) % 7).Add(sendTime);
        return local >= due ? monday.AddDays(-7) : monday.AddDays(-14);
    }
}
