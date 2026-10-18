using Caffeine.Data;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;
public sealed class GuestPromptService(AppDbContext db, TrackerClock clock, IConfiguration config)
{
    public static bool Due(bool authenticated, int drinks, int days, DateTime? until, DateTime now, int minDrinks = 10, int minDays = 5) => !authenticated && (!until.HasValue || until <= now.Date) && (drinks >= Math.Max(2, minDrinks) || days >= Math.Max(2, minDays));
    public async Task<bool> ShowAsync(string user, bool authenticated)
    {
        if (authenticated)
            return false;
        var now = clock.Now;
        var logs = await db.CaffeineLogs.Where(l => l.UserId == user && l.ConsumedAt <= now).Select(l => l.ConsumedAt.Date).ToListAsync();
        var free = await db.CaffeineFreeDays.Where(d => d.UserId == user && d.Day <= now.Date).Select(d => d.Day).ToListAsync();
        var sleep = await db.SleepLogs.Where(s => s.UserId == user && s.SleepDate <= now.Date).Select(s => s.SleepDate).ToListAsync();
        var until = await db.GuestPromptPreferences.Where(p => p.UserId == user).Select(p => p.DismissedUntil).SingleOrDefaultAsync();
        return Due(false, logs.Count, logs.Concat(free).Concat(sleep).Distinct().Count(), until, now, config.GetValue("GuestPrompt:Drinks", 10), config.GetValue("GuestPrompt:Days", 5));
    }

    public Task<int> DismissAsync(string user) => db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO GuestPromptPreferences (UserId,DismissedUntil) VALUES ({user},{clock.Now.Date.AddDays(14)}) ON CONFLICT(UserId) DO UPDATE SET DismissedUntil=excluded.DismissedUntil");
}
