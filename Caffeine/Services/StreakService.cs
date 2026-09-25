using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed record StreakSummary(int Current, int Best, bool TodayCompleted);

public sealed class StreakService(AppDbContext context)
{
    public async Task<StreakSummary> GetAsync(string userId, DateTime now)
    {
        // Use persisted caffeine snapshots, and calendar dates rather than 24-hour intervals.
        var loggedDays = await context.CaffeineLogs.AsNoTracking()
            .Where(l => l.UserId == userId && l.ConsumedAt <= now)
            .Select(l => l.ConsumedAt.Date).Distinct().ToListAsync();

        var freeDays = await context.CaffeineFreeDays.AsNoTracking()
            .Where(d => d.UserId == userId && d.Day <= now.Date)
            .Select(d => d.Day).ToListAsync();

        return Calculate(loggedDays.Concat(freeDays), now.Date);
    }

    public static StreakSummary Calculate(IEnumerable<DateTime> completedDays, DateTime today)
    {
        today = today.Date;

        var days = completedDays
            .Select(d => d.Date)
            .Where(d => d <= today)
            .Distinct()
            .Order()
            .ToArray();

        var run = 0;
        var best = 0;
        DateTime? previous = null;

        foreach (var day in days)
        {
            run = previous.HasValue && (day - previous.Value).Days == 1
                ? run + 1
                : 1;

            best = Math.Max(best, run);
            previous = day;
        }

        // Yesterday's run remains alive until the end of today.
        var current = previous.HasValue && (today - previous.Value).Days <= 1
            ? run
            : 0;

        return new(current, best, previous == today);
    }

    public async Task<bool> MarkCaffeineFreeAsync(string userId, DateTime day)
    {
        day = day.Date;
        var end = day.AddDays(1);

        // One atomic statement: repeat submits are harmless, and caffeine blocks the marker.
        var changed = await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT OR IGNORE INTO CaffeineFreeDays (UserId, Day)
            SELECT {userId}, {day}
            WHERE NOT EXISTS (
                SELECT 1 FROM CaffeineLogs
                WHERE UserId = {userId}
                  AND ConsumedAt >= {day}
                  AND ConsumedAt < {end}
                  AND TotalCaffeineMg > 0);
            """);

        return changed > 0 ||
               await context.CaffeineFreeDays.AsNoTracking()
                   .AnyAsync(d => d.UserId == userId && d.Day == day);
    }

    public Task<int> UnmarkCaffeineFreeAsync(string userId, DateTime day) =>
        context.CaffeineFreeDays
            .Where(d => d.UserId == userId && d.Day == day.Date)
            .ExecuteDeleteAsync();

    public async Task SaveLogChangesAsync()
    {
        var caffeinatedDays = context.ChangeTracker.Entries<CaffeineLog>()
            .Where(e => e.State == EntityState.Added && e.Entity.TotalCaffeineMg > 0)
            .Select(e => new
            {
                e.Entity.UserId,
                Day = e.Entity.ConsumedAt.Date
            })
            .Distinct()
            .ToArray();

        // Insert the log and clear contradictory markers together, including quick-add.
        await using var transaction = await context.Database.BeginTransactionAsync();

        await context.SaveChangesAsync();

        foreach (var item in caffeinatedDays)
        {
            await context.CaffeineFreeDays
                .Where(d => d.UserId == item.UserId && d.Day == item.Day)
                .ExecuteDeleteAsync();
        }

        await transaction.CommitAsync();
    }
}