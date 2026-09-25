using Caffeine.Data;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

// Called inside the existing repository's guest-transfer transaction.
public static class UserFeatureData
{
    public static async Task TransferAsync(
        AppDbContext db,
        string guest,
        string owner)
    {
        var dates = (await db.SleepLogs
                .Where(s => s.UserId == owner && !s.IsImportedDuplicate)
                .Select(s => s.SleepDate)
                .ToListAsync())
            .ToHashSet();

        foreach (var sleep in await db.SleepLogs
                     .Where(s => s.UserId == guest)
                     .OrderBy(s => s.Id)
                     .ToListAsync())
        {
            sleep.UserId = owner;

            if (!dates.Add(sleep.SleepDate))
                sleep.IsImportedDuplicate = true;
        }

        var pref = await db.TrackerPreferences
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == guest);

        if (pref != null)
        {
            if (!await db.TrackerPreferences.AnyAsync(p => p.UserId == owner))
            {
                pref.UserId = owner;
                db.TrackerPreferences.Add(pref);
            }

            await db.TrackerPreferences
                .Where(p => p.UserId == guest)
                .ExecuteDeleteAsync();
        }

        var notice = await db.NotificationPreferences
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == guest);

        if (notice != null)
        {
            if (!await db.NotificationPreferences.AnyAsync(p => p.UserId == owner))
            {
                notice.UserId = owner;
                db.NotificationPreferences.Add(notice);
            }

            await db.NotificationPreferences
                .Where(p => p.UserId == guest)
                .ExecuteDeleteAsync();
        }

        foreach (var sub in await db.BrowserPushSubscriptions
                     .Where(s => s.UserId == guest)
                     .ToListAsync())
        {
            sub.UserId = owner;
        }
    }

    public static async Task DeleteAsync(
        AppDbContext db,
        string userId)
    {
        db.SleepLogs.RemoveRange(
            await db.SleepLogs
                .Where(s => s.UserId == userId)
                .ToListAsync());

        db.TrackerPreferences.RemoveRange(
            await db.TrackerPreferences
                .Where(p => p.UserId == userId)
                .ToListAsync());

        db.NotificationPreferences.RemoveRange(
            await db.NotificationPreferences
                .Where(p => p.UserId == userId)
                .ToListAsync());

        db.BrowserPushSubscriptions.RemoveRange(
            await db.BrowserPushSubscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync());
    }
}